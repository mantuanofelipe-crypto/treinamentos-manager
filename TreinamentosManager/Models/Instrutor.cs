using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class Instrutor
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Expiração ACI")]
        [DataType(DataType.Date)]
        public DateTime? DataExpiracaoACI { get; set; }

        public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
        public ICollection<InstrutorProficiencia> Proficiencias { get; set; } = new List<InstrutorProficiencia>();
        public ICollection<InstrutorACP> ACPs { get; set; } = new List<InstrutorACP>();
    }
}
