using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome da Empresa")]
        public string Nome { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "CNPJ")]
        public string? Empresa { get; set; }

        [Display(Name = "Total de Alunos")]
        public int? TotalAlunos { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
    }
}