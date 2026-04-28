using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class TurmaData
    {
        public int Id { get; set; }

        [Required]
        public int TurmaId { get; set; }
        public Turma Turma { get; set; } = default!;

        [Required]
        [Display(Name = "Data da Turma")]
        public DateTime Data { get; set; }
    }
}
