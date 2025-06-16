using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Servicios.V1
{
    public class ServicioAutores : IServicioAutores
    {
        private readonly ApplicationDbContex contex;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public ServicioAutores(ApplicationDbContex contex, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            this.contex = contex;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AutorDTO>> Get(PaginacionDTO paginacionDTO)
        {

            var queryable = contex.Autores.AsQueryable();
            await httpContextAccessor.HttpContext.InsertarParametroPaginacionCabecera(queryable);
            var autores = await queryable.OrderBy(x => x.Id).Paginar(paginacionDTO).ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;

        }
    }
}
