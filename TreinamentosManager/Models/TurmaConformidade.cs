namespace TreinamentosManager.Models
{
    public class TurmaConformidade
    {
        public int Id { get; set; }

        public int TurmaId { get; set; }
        public Turma Turma { get; set; } = default!;

        public string FichaInscricao { get; set; } = ConformidadeStatus.Pendente;
        public string Pauta { get; set; } = ConformidadeStatus.Pendente;
        public string Convite { get; set; } = ConformidadeStatus.Pendente;
        public string ConfiguracaoHub { get; set; } = ConformidadeStatus.Pendente;
        public string ScriptAula { get; set; } = ConformidadeStatus.Pendente;
        public string MaterialAula { get; set; } = ConformidadeStatus.Pendente;
        public string BancoQuestoes { get; set; } = ConformidadeStatus.Pendente;
        public string Nuvem { get; set; } = ConformidadeStatus.Pendente;
        public string PreenchimentoPauta { get; set; } = ConformidadeStatus.Pendente;
        public string Certificado { get; set; } = ConformidadeStatus.Pendente;

        public string? JustificativaInstrutor { get; set; }
        public string? JustificativaCoordenacao { get; set; }

        public decimal? ConformidadeInstrutor => CalcularPercentual(new[]
        {
            ScriptAula,
            MaterialAula,
            BancoQuestoes,
            PreenchimentoPauta,
            Nuvem
        });

        public decimal? ConformidadeCoordenacao => CalcularPercentual(new[]
        {
            FichaInscricao,
            Pauta,
            Convite,
            ConfiguracaoHub,
            Certificado
        });

        public decimal? ConformidadeGeral
        {
            get
            {
                if (!ConformidadeInstrutor.HasValue || !ConformidadeCoordenacao.HasValue)
                    return null;

                return Math.Round((ConformidadeInstrutor.Value + ConformidadeCoordenacao.Value) / 2m, 1);
            }
        }

        private static decimal? CalcularPercentual(IEnumerable<string> status)
        {
            var considerados = status
                .Where(s => !string.Equals(s, ConformidadeStatus.EmAndamento, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!considerados.Any())
                return null;

            var oks = considerados.Count(s => string.Equals(s, ConformidadeStatus.Ok, StringComparison.OrdinalIgnoreCase));
            return Math.Round(oks * 100m / considerados.Count, 1);
        }
    }
}
