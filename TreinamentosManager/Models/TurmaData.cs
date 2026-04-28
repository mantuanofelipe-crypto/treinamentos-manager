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

        [Range(0.25, 24)]
        [Display(Name = "Duração da Aula")]
        public decimal DuracaoHoras { get; set; } = 1;

        public string? TeamsMeetingId { get; set; }

        public string? TeamsMeetingUrl { get; set; }

        public DateTime Fim => Data.AddHours((double)DuracaoHoras);
    }
}
