using WebApiAutores.DTOs;

namespace WebApiAutores.Servicios.V1
{
    public interface IServicioAutores
    {
        Task<IEnumerable<AutorDTO>> Get(PaginacionDTO paginacionDTO);
    }
}
