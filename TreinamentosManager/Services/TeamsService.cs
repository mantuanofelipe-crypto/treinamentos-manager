using Azure.Identity;
using Microsoft.Graph.Models;
using TreinamentosManager.Models;

namespace TreinamentosManager.Services
{
    public class TeamsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TeamsService> _logger;

        public TeamsService(IConfiguration configuration, ILogger<TeamsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public bool EstaConfigurado =>
            !string.IsNullOrWhiteSpace(_configuration["Teams:TenantId"]) &&
            !string.IsNullOrWhiteSpace(_configuration["Teams:ClientId"]) &&
            !string.IsNullOrWhiteSpace(_configuration["Teams:ClientSecret"]) &&
            !string.IsNullOrWhiteSpace(_configuration["Teams:OrganizerUserId"]);

        public async Task CriarReunioesTeams(Turma turma)
        {
            if (!EstaConfigurado)
            {
                _logger.LogWarning("Microsoft Teams nao esta configurado. As reunioes nao foram criadas.");
                return;
            }

            var credential = new ClientSecretCredential(
                _configuration["Teams:TenantId"],
                _configuration["Teams:ClientId"],
                _configuration["Teams:ClientSecret"]);

            var graphClient = new Microsoft.Graph.GraphServiceClient(
                credential,
                new[] { "https://graph.microsoft.com/.default" });

            var organizerUserId = _configuration["Teams:OrganizerUserId"]!;

            foreach (var data in turma.Datas.OrderBy(d => d.Data))
            {
                if (!string.IsNullOrWhiteSpace(data.TeamsMeetingUrl))
                    continue;

                var onlineMeeting = new OnlineMeeting
                {
                    StartDateTime = data.Data,
                    EndDateTime = data.Fim,
                    Subject = CriarAssunto(turma, data)
                };

                try
                {
                    var result = await graphClient.Users[organizerUserId].OnlineMeetings.PostAsync(onlineMeeting);
                    data.TeamsMeetingId = result?.Id;
                    data.TeamsMeetingUrl = result?.JoinWebUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao criar reuniao do Teams para a turma {TurmaId} em {Data}.", turma.Id, data.Data);
                }
            }

            turma.TeamsMeetingUrl = turma.Datas
                .OrderBy(d => d.Data)
                .Select(d => d.TeamsMeetingUrl)
                .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url));
        }

        private static string CriarAssunto(Turma turma, TurmaData data)
        {
            var software = turma.Software?.Nome ?? "Treinamento";
            var cliente = turma.Cliente?.Nome ?? "Cliente";
            return $"{software} - {cliente} ({data.Data:dd/MM/yyyy HH:mm})";
        }
    }
}
