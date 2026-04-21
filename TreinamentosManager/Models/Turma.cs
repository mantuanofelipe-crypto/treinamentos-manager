using System.ComponentModel.DataAnnotations;

namespace TreinamentosManager.Models
{
    public class Turma
    {
        public int Id { get; set; }

        // Status calculado automaticamente
        public string Status
        {
            get
            {
                var now = DateTime.Now;
                if (Fim < now)
                    return "Concluída";
                else if (Inicio <= now && Fim >= now)
                    return "Em andamento";
                else
                    return "Futura";
            }
        }

        public string? IdAutodesk { get; set; }

        [Display(Name = "Modalidade")]
        public string Modalidade { get; set; } = "Presencial";

        // Relacionamento com Cliente
        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // Relacionamento com Software
        [Required]
        public int SoftwareId { get; set; }
        public Software? Software { get; set; }

        public int CargaHoraria { get; set; } // Carga Horária

        public string? DiasDaSemana { get; set; } // Dias da Semana

        [Required]
        public DateTime Inicio { get; set; } // Data de início

        [Required]
        public DateTime Fim { get; set; } // Data de fim

        // Relacionamento com Instrutor
        public string? InstrutorId { get; set; }
        public Instrutor? Instrutor { get; set; }

        // Campo para armazenar o link da reunião Teams
        public string? TeamsMeetingUrl { get; set; }

        // Método auxiliar para obter cor do status
        public string GetStatusColor()
        {
            return Status switch
            {
                "Concluída" => "success",
                "Em andamento" => "warning",
                "Futura" => "primary",
                _ => "secondary"
            };
        }
    }
}