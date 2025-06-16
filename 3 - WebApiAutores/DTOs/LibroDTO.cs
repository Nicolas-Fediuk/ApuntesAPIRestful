using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class LibroDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime? FechaDePublicacion { get; set; }
        
        //public List<ComentarioDto> Comentarios { get; set; }
    }
}
