using WebApiAutores.DTOs;

namespace WebApiAutores.Utilidades
{
    public static class IQueryableExtension
    {
        //para saltar las paginas
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginacionDTO paginacionDTO)
        {
            return queryable.Skip((paginacionDTO.Pagina - 1) * paginacionDTO.RecodsPorPagina)
                            .Take(paginacionDTO.RecodsPorPagina);
        }
    }
}
