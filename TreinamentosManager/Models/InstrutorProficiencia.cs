using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class InstrutorProficiencia
    {
        public int Id { get; set; }

        [Required]
        public string InstrutorId { get; set; } = default!;
        public Instrutor Instrutor { get; set; } = default!;

        [Required]
        public int SoftwareId { get; set; }
        public Software Software { get; set; } = default!;

        [Required]
        [Range(1, 5)]
        [Display(Name = "Proficiência (estrelas)")]
        public int Estrelas { get; set; }
    }
}
