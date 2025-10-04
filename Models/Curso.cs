using System.ComponentModel.DataAnnotations;

namespace Parcial_VargasMedina.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Los créditos deben ser mayor que 0")]
        public int Creditos { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El cupo máximo debe ser mayor que 0")]
        public int CupoMaximo { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeOnly HorarioInicio { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeOnly HorarioFin { get; set; }

        public bool Activo { get; set; } = true;

        // Propiedad calculada para obtener matriculas activas
        public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

        public int CupoDisponible => CupoMaximo - Matriculas.Count(m => m.Estado != EstadoMatricula.Cancelada);
    }
}