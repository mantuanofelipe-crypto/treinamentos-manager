using Microsoft.Graph;
using Microsoft.Graph.Models;
using TreinamentosManager.Models;

namespace TreinamentosManager.Services
{
    public class TeamsService
    {
        // Nota: Para produção, seria necessário configurar autenticação OAuth com Azure AD
        // Este é um exemplo simplificado
        public async Task<string> CriarReuniaoTeams(Turma turma)
        {
            // Simulação - em produção, usar Graph API autenticado
            var meetingUrl = $"https://teams.microsoft.com/l/meetup-join/19%3ameeting_{Guid.NewGuid()}%40thread.v2/0?context=%7b%22Tid%22%3a%22{ Guid.NewGuid()}%22%2c%22Oid%22%3a%22{Guid.NewGuid()}%22%7d";

            // Aqui seria o código real:
            /*
            var graphClient = new GraphServiceClient(authProvider);

            var onlineMeeting = new OnlineMeeting
            {
                StartDateTime = turma.Inicio,
                EndDateTime = turma.Fim,
                Subject = $"Treinamento: {turma.Software?.Nome ?? "Software"} - {turma.Cliente?.Nome ?? "Cliente"}",
                Participants = new MeetingParticipants
                {
                    Attendees = new List<MeetingParticipantInfo>
                    {
                        new MeetingParticipantInfo
                        {
                            Identity = new IdentitySet
                            {
                                User = new Identity
                                {
                                    Id = turma.InstrutorId
                                }
                            }
                        }
                    }
                }
            };

            var result = await graphClient.Me.OnlineMeetings.Request().AddAsync(onlineMeeting);
            return result.JoinWebUrl;
            */

            return meetingUrl;
        }
    }
}