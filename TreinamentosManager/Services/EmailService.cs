using MailKit.Net.Smtp;
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

        public async Task EnviarEmailInstrutor(Turma turma)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sistema de Treinamentos", "noreply@treinamentos.com"));
            message.To.Add(new MailboxAddress(turma.Instrutor?.Nome ?? "Instrutor", turma.Instrutor?.Email ?? ""));
            message.Subject = $"Nova Turma Agendada: {turma.Software?.Nome ?? "Treinamento"}";

            var body = $@"
                <h2>Nova Turma Agendada</h2>
                <p><strong>Cliente:</strong> {turma.Cliente?.Nome}</p>
                <p><strong>Software:</strong> {turma.Software?.Nome}</p>
                <p><strong>Periodo:</strong> {turma.Inicio:dd/MM/yyyy HH:mm} a {turma.Fim:dd/MM/yyyy HH:mm}</p>
                <p><strong>Carga Horaria:</strong> {turma.CargaHoraria}h</p>
                <p><strong>Dias da Semana:</strong> {turma.DiasDaSemana}</p>
                {CriarListaEncontrosHtml(turma)}
            ";

            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, false);
            await client.AuthenticateAsync("seuemail@gmail.com", "suasenha");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task EnviarConviteCliente(Turma turma, string emailCliente)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sistema de Treinamentos", "noreply@treinamentos.com"));
            message.To.Add(new MailboxAddress(turma.Cliente?.Nome ?? "Cliente", emailCliente));
            message.Subject = $"Convite: Treinamento {turma.Software?.Nome ?? "Software"}";

            var body = $@"
                <h2>Convite para Treinamento</h2>
                <p>Prezado(a) {turma.Cliente?.Nome ?? "Cliente"},</p>
                <p>Voce esta convidado(a) para participar do treinamento de {turma.Software?.Nome ?? "Software"}.</p>
                <p><strong>Periodo:</strong> {turma.Inicio:dd/MM/yyyy HH:mm} a {turma.Fim:dd/MM/yyyy HH:mm}</p>
                <p><strong>Carga Horaria:</strong> {turma.CargaHoraria}h</p>
                <p><strong>Instrutor:</strong> {turma.Instrutor?.Nome}</p>
                {CriarListaEncontrosHtml(turma)}
                <p>Atenciosamente,<br>Equipe de Treinamentos</p>
            ";

            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, false);
            await client.AuthenticateAsync("seuemail@gmail.com", "suasenha");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string CriarListaEncontrosHtml(Turma turma)
        {
            if (!turma.Datas.Any())
                return "";

            var itens = string.Join("", turma.Datas.OrderBy(d => d.Data).Select(d =>
            {
                var link = string.IsNullOrWhiteSpace(d.TeamsMeetingUrl)
                    ? ""
                    : $" - <a href='{d.TeamsMeetingUrl}'>Entrar no Teams</a>";
                return $"<li>{d.Data:dd/MM/yyyy HH:mm} - {d.Fim:HH:mm} ({d.DuracaoHoras:0.##}h){link}</li>";
            }));

            return $"<p><strong>Datas das aulas:</strong></p><ul>{itens}</ul>";
        }
    }
}
