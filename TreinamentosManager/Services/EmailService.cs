using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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
            ValorConfigurado("Smtp:Host") &&
            ValorConfigurado("Smtp:Port") &&
            ValorConfigurado("Smtp:FromEmail");

        public IReadOnlyList<string> ObterVariaveisPendentes()
        {
            var pendentes = new List<string>();

            AdicionarSePendente(pendentes, "Smtp:Host", "Smtp__Host");
            AdicionarSePendente(pendentes, "Smtp:Port", "Smtp__Port");
            AdicionarSePendente(pendentes, "Smtp:FromEmail", "Smtp__FromEmail");

            return pendentes;
        }

        public async Task EnviarComunicadoAsync(IEnumerable<string> destinatarios, string assunto, string corpo)
        {
            if (!EstaConfigurado)
                throw new InvalidOperationException($"SMTP não configurado. Verifique: {string.Join(", ", ObterVariaveisPendentes())}");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Smtp:FromName"] ?? "Desk Class",
                _configuration["Smtp:FromEmail"]!));

            foreach (var email in destinatarios)
            {
                message.To.Add(MailboxAddress.Parse(email));
            }

            message.Subject = assunto;
            message.Body = new BodyBuilder
            {
                HtmlBody = ConverterTextoParaHtml(corpo),
                TextBody = corpo
            }.ToMessageBody();

            using var client = new SmtpClient();
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
