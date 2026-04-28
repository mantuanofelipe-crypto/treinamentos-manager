using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TreinamentosManager.Models;

namespace TreinamentosManager.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool EstaConfigurado =>
            ResendConfigurado ||
            SmtpConfigurado;

        private bool ResendConfigurado =>
            ValorConfigurado("Resend:ApiKey") &&
            ValorConfigurado("Resend:FromEmail");

        private bool SmtpConfigurado =>
            string.Equals(_configuration["Smtp:Enabled"], "true", StringComparison.OrdinalIgnoreCase) &&
            ValorConfigurado("Smtp:Host") &&
            ValorConfigurado("Smtp:Port") &&
            ValorConfigurado("Smtp:FromEmail");

        public IReadOnlyList<string> ObterVariaveisPendentes()
        {
            var pendentes = new List<string>();

            if (!ResendConfigurado)
            {
                AdicionarSePendente(pendentes, "Resend:ApiKey", "Resend__ApiKey");
                AdicionarSePendente(pendentes, "Resend:FromEmail", "Resend__FromEmail");
            }

            if (string.Equals(_configuration["Smtp:Enabled"], "true", StringComparison.OrdinalIgnoreCase) && !SmtpConfigurado)
            {
                AdicionarSePendente(pendentes, "Smtp:Host", "Smtp__Host");
                AdicionarSePendente(pendentes, "Smtp:Port", "Smtp__Port");
                AdicionarSePendente(pendentes, "Smtp:FromEmail", "Smtp__FromEmail");
            }

            return pendentes;
        }

        public async Task EnviarComunicadoAsync(
            IEnumerable<string> destinatarios,
            string assunto,
            string corpo,
            IEnumerable<EmailAttachment>? anexos = null)
        {
            if (!EstaConfigurado)
                throw new InvalidOperationException($"Email não configurado. Configure Resend no Railway: {string.Join(", ", ObterVariaveisPendentes())}. SMTP direto só será usado se Smtp__Enabled=true.");

            if (ResendConfigurado)
            {
                await EnviarViaResendAsync(destinatarios, assunto, corpo, anexos);
                return;
            }

            await EnviarViaSmtpAsync(destinatarios, assunto, corpo, anexos);
        }

        private async Task EnviarViaResendAsync(
            IEnumerable<string> destinatarios,
            string assunto,
            string corpo,
            IEnumerable<EmailAttachment>? anexos)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration["Resend:ApiKey"]);

            var payload = new
            {
                from = $"{_configuration["Resend:FromName"] ?? "Desk Class"} <{_configuration["Resend:FromEmail"]}>",
                to = destinatarios.ToArray(),
                subject = assunto,
                html = ConverterTextoParaHtml(corpo),
                text = corpo,
                attachments = anexos?.Select(a => new
                {
                    filename = a.FileName,
                    content = Convert.ToBase64String(a.Content)
                }).ToArray()
            };

            var response = await httpClient.PostAsync(
                "https://api.resend.com/emails",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Resend retornou {(int)response.StatusCode}: {responseBody}");
            }
        }

        private async Task EnviarViaSmtpAsync(
            IEnumerable<string> destinatarios,
            string assunto,
            string corpo,
            IEnumerable<EmailAttachment>? anexos)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Smtp:FromName"] ?? "Desk Class",
                _configuration["Smtp:FromEmail"]!));

            foreach (var email in destinatarios)
            {
                message.To.Add(MailboxAddress.Parse(email));
            }

            message.Subject = assunto;
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = ConverterTextoParaHtml(corpo),
                TextBody = corpo
            };

            foreach (var anexo in anexos ?? Enumerable.Empty<EmailAttachment>())
            {
                bodyBuilder.Attachments.Add(anexo.FileName, anexo.Content, ContentType.Parse(anexo.ContentType));
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 15000;
            var port = int.Parse(_configuration["Smtp:Port"]!);
            var secureSocketOptions = ObterSegurancaSmtp(port);

            await client.ConnectAsync(_configuration["Smtp:Host"], port, secureSocketOptions);

            if (ValorConfigurado("Smtp:Username"))
                await client.AuthenticateAsync(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task EnviarEmailInstrutor(Turma turma)
        {
            if (turma.Instrutor == null || string.IsNullOrWhiteSpace(turma.Instrutor.Email))
                return;

            var corpo = $"""
                Nova Turma Agendada

                Cliente: {turma.Cliente?.Nome}
                Software: {turma.Software?.Nome}
                Período: {turma.Inicio:dd/MM/yyyy HH:mm} a {turma.Fim:dd/MM/yyyy HH:mm}
                Carga horária: {turma.CargaHoraria}h
                Dias da semana: {turma.DiasDaSemana}

                {CriarListaEncontrosTexto(turma)}
                """;

            await EnviarComunicadoAsync(new[] { turma.Instrutor.Email }, $"Nova Turma Agendada: {turma.Software?.Nome ?? "Treinamento"}", corpo);
        }

        public async Task EnviarConviteCliente(Turma turma, string emailCliente)
        {
            var corpo = Comunicados.ComunicadoBuilder.CriarTexto(turma);
            await EnviarComunicadoAsync(new[] { emailCliente }, $"Convite: Treinamento {turma.Software?.Nome ?? "Software"}", corpo);
        }

        private static string CriarListaEncontrosTexto(Turma turma)
        {
            if (!turma.Datas.Any())
                return "";

            return "Datas das aulas:\n" + string.Join("\n", turma.Datas.OrderBy(d => d.Data).Select(d =>
                $"- {d.Data:dd/MM/yyyy HH:mm} - {d.Fim:HH:mm} ({d.DuracaoHoras:0.##}h)"));
        }

        private static string ConverterTextoParaHtml(string texto)
        {
            return "<div style=\"font-family:Arial,sans-serif;font-size:14px;line-height:1.45\">" +
                   System.Net.WebUtility.HtmlEncode(texto).Replace("\n", "<br>") +
                   "</div>";
        }

        private static SecureSocketOptions ObterSegurancaSmtp(int port)
        {
            return port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
        }

        private bool ValorConfigurado(string chave)
        {
            return !string.IsNullOrWhiteSpace(_configuration[chave]);
        }

        private void AdicionarSePendente(List<string> pendentes, string chave, string nomeVariavel)
        {
            if (!ValorConfigurado(chave))
                pendentes.Add(nomeVariavel);
        }
    }
}
