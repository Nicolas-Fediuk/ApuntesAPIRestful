using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    [Route("api/v2/autores-coleccion")]
    public class AutoresColeccionControllers : ControllerBase
    {
        private readonly ApplicationDbContex contex;
        private readonly IMapper mapper;

        public AutoresColeccionControllers(ApplicationDbContex contex, IMapper mapper)
        {
            this.contex = contex;
            this.mapper = mapper;
        }

        [HttpGet("{ids}", Name = "ObtenerAutoresPorIdsV2")] // api/autores-coleccion/1,2,3
        [AllowAnonymous]
        public async Task<ActionResult<List<AutorDTOConLibros>>> Get(string ids)
        {
            var idsColeccion = new List<int>();

            foreach (var id in ids.Split(","))
            {
                if (int.TryParse(id, out int idInt))
                {
                    idsColeccion.Add(idInt);
                }

            }

            if (!idsColeccion.Any())
            {
                ModelState.AddModelError(nameof(ids), "Ningun Id fue encontrado");
                return ValidationProblem();
            }

            var autores = await contex.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(x => x.Libro)
                .Where(x => idsColeccion.Contains(x.Id))
                .ToListAsync();

            if (autores.Count != idsColeccion.Count)
            {
                return NotFound();
            }

            var autoresDTO = mapper.Map<List<AutorDTOConLibros>>(autores);

            return autoresDTO;
        }

        //asi hago un insert de forma masiva 
        [HttpPost]
        public async Task<ActionResult> Post(IEnumerable<AutorCreacionDTO> autorCreacionDTOs)
        {
            var autores = mapper.Map<IEnumerable<Autor>>(autorCreacionDTOs);

            //sirve para agregar un listado
            contex.AddRange(autores);

            await contex.SaveChangesAsync();

            var autoresDTO = mapper.Map<AutorDTOConLibros>(autores);
            var ids = autores.Select(x => x.Id);
            var idsString = string.Join(",", ids);

            return CreatedAtRoute("ObtenerAutoresPorIdsV2", new { ids = idsString }, autoresDTO);

        }
    }
}
