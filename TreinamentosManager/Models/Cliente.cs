using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome do Cliente")]
        public string Nome { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Display(Name = "Empresa")]
        public string? Empresa { get; set; }

        // Relacionamento com Turmas
        public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
    }
}