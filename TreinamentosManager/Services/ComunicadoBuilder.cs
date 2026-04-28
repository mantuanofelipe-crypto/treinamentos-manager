using System.Globalization;
using TreinamentosManager.Models;

namespace TreinamentosManager.Services.Comunicados
{
    public static class ComunicadoBuilder
    {
        public static string CriarAssunto(Turma turma)
        {
            return $"Convite de treinamento - {turma.Software?.Nome ?? "Curso"}";
        }

        public static string CriarTexto(Turma turma)
        {
            var datas = turma.Datas.OrderBy(d => d.Data).ToList();
            var primeiraData = datas.FirstOrDefault()?.Data ?? turma.Inicio;
            var ultimaData = datas.LastOrDefault()?.Data ?? turma.Fim;
            var primeiroFim = datas.FirstOrDefault()?.Fim ?? turma.Fim;

            var diasSemana = !string.IsNullOrWhiteSpace(turma.DiasDaSemana)
                ? turma.DiasDaSemana
                : string.Join(", ", datas.Select(d => CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetDayName(d.Data.DayOfWeek)).Distinct());

            return $"""
                CONVITE DE TREINAMENTO

                Caro(a) aluno(a), você foi inscrito(a) para participar do treinamento de {turma.Software?.Nome ?? "Nome do curso"}.

                Venha participar dessa jornada de conhecimento com a Deskgraphics!

                Para esclarecimento de dúvidas contatar:
                treinamento@deskgraphics.com.br

                TREINAMENTO AO VIVO ON-LINE

                {turma.Instrutor?.Nome ?? "Instrutor"} .................................................. Instrutor Técnico Especialista

                Aulas gravadas disponíveis em até 24 horas na nossa plataforma de vídeos DeskHub.
                Acesse: https://deskhub.deskgraphics.com.br

                SERÁ TRANSMITIDO PELA PLATAFORMA MICROSOFT TEAMS

                {diasSemana} das {primeiraData:HH:mm} às {primeiroFim:HH:mm}

                DISPONIBILIDADE DAS GRAVAÇÕES

                Carga horária total do treinamento: {turma.CargaHoraria}h

                {primeiraData:dd/MM/yyyy} a {ultimaData:dd/MM/yyyy}

                Datas das aulas:
                {CriarListaDatas(datas)}
                """;
        }

        private static string CriarListaDatas(List<TurmaData> datas)
        {
            if (!datas.Any())
                return "- Datas a confirmar";

            return string.Join("\n", datas.Select(d => $"- {d.Data:dd/MM/yyyy HH:mm} às {d.Fim:HH:mm} ({d.DuracaoHoras:0.##}h)"));
        }
    }
}
