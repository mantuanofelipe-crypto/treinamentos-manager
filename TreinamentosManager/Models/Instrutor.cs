using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class Instrutor
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Relacionamento com Turmas
        public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
    }
}