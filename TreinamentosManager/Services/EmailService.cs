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
            message.Subject = $"Nova Turma Agendada: {turma.Software}";

            var body = $@"
                <h2>Nova Turma Agendada</h2>
                <p><strong>Cliente:</strong> {turma.Cliente}</p>
                <p><strong>Software:</strong> {turma.Software}</p>
                <p><strong>Data Início:</strong> {turma.Inicio:dd/MM/yyyy HH:mm}</p>
                <p><strong>Data Fim:</strong> {turma.Fim:dd/MM/yyyy HH:mm}</p>
                <p><strong>Carga Horária:</strong> {turma.CargaHoraria}h</p>
                <p><strong>Dias da Semana:</strong> {turma.DiasDaSemana}</p>
                {(string.IsNullOrEmpty(turma.TeamsMeetingUrl) ? "" : $"<p><strong>Link da Reunião:</strong> <a href='{turma.TeamsMeetingUrl}'>Entrar na Reunião</a></p>")}
            ";

            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            // Configurar SMTP - exemplo com Gmail (ajustar conforme necessário)
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
                <p>Você está convidado(a) para participar do treinamento de {turma.Software?.Nome ?? "Software"}.</p>
                <p><strong>Data Início:</strong> {turma.Inicio:dd/MM/yyyy HH:mm}</p>
                <p><strong>Data Fim:</strong> {turma.Fim:dd/MM/yyyy HH:mm}</p>
                <p><strong>Carga Horária:</strong> {turma.CargaHoraria}h</p>
                <p><strong>Instrutor:</strong> {turma.Instrutor?.Nome}</p>
                {(string.IsNullOrEmpty(turma.TeamsMeetingUrl) ? "" : $"<p><strong>Link da Reunião:</strong> <a href='{turma.TeamsMeetingUrl}'>Entrar na Reunião</a></p>")}
                <p>Atenciosamente,<br>Equipe de Treinamentos</p>
            ";

            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, false);
            await client.AuthenticateAsync("seuemail@gmail.com", "suasenha");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}