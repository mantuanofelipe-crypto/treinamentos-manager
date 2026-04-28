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
            ValorConfigurado("Teams:TenantId") &&
            ValorConfigurado("Teams:ClientId") &&
            ValorConfigurado("Teams:ClientSecret") &&
            ValorConfigurado("Teams:OrganizerUserId");

        public async Task CriarReunioesTeams(Turma turma)
        {
            if (!EstaConfigurado)
            {
                _logger.LogWarning("Microsoft Teams nao esta configurado. As reunioes nao foram criadas.");
                return;
            }

            Microsoft.Graph.GraphServiceClient graphClient;
            string organizerUserId;

            try
            {
                var credential = new ClientSecretCredential(
                    _configuration["Teams:TenantId"],
                    _configuration["Teams:ClientId"],
                    _configuration["Teams:ClientSecret"]);

                graphClient = new Microsoft.Graph.GraphServiceClient(
                    credential,
                    new[] { "https://graph.microsoft.com/.default" });

                organizerUserId = _configuration["Teams:OrganizerUserId"]!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Configuracao do Microsoft Teams invalida. Verifique TenantId, ClientId, ClientSecret e OrganizerUserId.");
                return;
            }

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

        private bool ValorConfigurado(string chave)
        {
            var valor = _configuration[chave];

            if (string.IsNullOrWhiteSpace(valor))
                return false;

            return !valor.StartsWith("SEU_", StringComparison.OrdinalIgnoreCase) &&
                   !valor.StartsWith("ID_DO_", StringComparison.OrdinalIgnoreCase);
        }
    }
}
