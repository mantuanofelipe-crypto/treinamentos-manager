using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class Software
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome do Software")]
        public string Nome { get; set; }

        // Relacionamento com Turmas
        public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
    }
}