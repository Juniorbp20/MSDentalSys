using System.ComponentModel.DataAnnotations;

namespace MSDentalSys.Data.Models
{
    public class Seguro
    {
        public int SeguroId { get; set; }

        [Required]
        [StringLength(120)]
        public string Nombre { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
    }
}
