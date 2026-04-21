using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class InstrutorACP
    {
        public int Id { get; set; }

        [Required]
        public string InstrutorId { get; set; } = default!;
        public Instrutor Instrutor { get; set; } = default!;

        [Required]
        public int SoftwareId { get; set; }
        public Software Software { get; set; } = default!;

        [Required]
        [Display(Name = "Expiração ACP")]
        [DataType(DataType.Date)]
        public DateTime DataExpiracao { get; set; }
    }
}
