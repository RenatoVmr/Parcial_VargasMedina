using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Parcial_VargasMedina.Models
{
    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }

    public class Matricula
    {
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }
        public virtual Curso Curso { get; set; } = null!;

        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public virtual IdentityUser Usuario { get; set; } = null!;

        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required]
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
    }
}