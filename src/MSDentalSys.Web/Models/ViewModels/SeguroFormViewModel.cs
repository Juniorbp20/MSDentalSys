using System.ComponentModel.DataAnnotations;

namespace MSDentalSys.Web.Models.ViewModels
{
    public class SeguroFormViewModel
    {
        public int SeguroId { get; set; }

        [Required(ErrorMessage = "El nombre del seguro es obligatorio.")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
