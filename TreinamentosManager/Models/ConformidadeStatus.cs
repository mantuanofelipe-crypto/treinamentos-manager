namespace TreinamentosManager.Models
{
    public static class ConformidadeStatus
    {
        public const string Atrasada = "Atrasada";
        public const string EmAndamento = "Em andamento";
        public const string ForaDoPrazo = "Fora do prazo";
        public const string Pendente = "Pendente";
        public const string Ok = "Ok";

        public static readonly string[] Todos =
        {
            Atrasada,
            EmAndamento,
            ForaDoPrazo,
            Pendente,
            Ok
        };
    }
}
