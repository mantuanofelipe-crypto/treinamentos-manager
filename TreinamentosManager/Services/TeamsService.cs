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

        public IReadOnlyList<string> ObterVariaveisPendentes()
        {
            var pendentes = new List<string>();

            AdicionarSePendente(pendentes, "Teams:TenantId", "Teams__TenantId");
            AdicionarSePendente(pendentes, "Teams:ClientId", "Teams__ClientId");
            AdicionarSePendente(pendentes, "Teams:ClientSecret", "Teams__ClientSecret");
            AdicionarSePendente(pendentes, "Teams:OrganizerUserId", "Teams__OrganizerUserId");

            return pendentes;
        }

        public async Task<TeamsCriacaoResultado> CriarReunioesTeams(Turma turma)
        {
            var resultado = new TeamsCriacaoResultado();

            if (!EstaConfigurado)
            {
                _logger.LogWarning("Microsoft Teams nao esta configurado. As reunioes nao foram criadas.");
                resultado.Erros.Add("Microsoft Teams nao esta configurado.");
                return resultado;
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
                resultado.Erros.Add($"Configuracao do Microsoft Teams invalida: {ex.Message}");
                return resultado;
            }

            foreach (var data in turma.Datas.OrderBy(d => d.Data))
            {
                if (!string.IsNullOrWhiteSpace(data.TeamsMeetingUrl))
                {
                    resultado.Ignoradas++;
                    continue;
                }

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

                    if (!string.IsNullOrWhiteSpace(data.TeamsMeetingUrl))
                        resultado.Criadas++;
                    else
                        resultado.Erros.Add($"Graph nao retornou link para {data.Data:dd/MM/yyyy HH:mm}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao criar reuniao do Teams para a turma {TurmaId} em {Data}.", turma.Id, data.Data);
                    resultado.Erros.Add($"{data.Data:dd/MM/yyyy HH:mm}: {ex.Message}");
                }
            }

            turma.TeamsMeetingUrl = turma.Datas
                .OrderBy(d => d.Data)
                .Select(d => d.TeamsMeetingUrl)
                .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url));

            return resultado;
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

        private void AdicionarSePendente(List<string> pendentes, string chave, string nomeVariavel)
        {
            if (!ValorConfigurado(chave))
                pendentes.Add(nomeVariavel);
        }
    }

    public class TeamsCriacaoResultado
    {
        public int Criadas { get; set; }
        public int Ignoradas { get; set; }
        public List<string> Erros { get; } = new();
        public bool Sucesso => Criadas > 0 && !Erros.Any();
    }
}
