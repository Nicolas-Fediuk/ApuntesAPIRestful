using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics;
using System.Xml.Linq;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;
using System.Linq.Dynamic.Core;
using WebApiAutores.Servicios.V1;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    [Route("api/v2/autores")]
    // El controlador solo se peude acceder con un usuario logeado, menos los endpoint que tengan [AllowAnonymous]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]

    public class AutoresController : ControllerBase
    {

        private readonly ApplicationDbContex context;
        private readonly IConfiguration configuration;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<AutoresController> logger;
        private readonly IServicioAutores servicioAutoresV1;

        //private readonly IOutpuCacheStore outpuCacheStore;
        //private const string cache = "autores-obtener";
        private const string contenedor = "autores";

        public IMapper Mapper { get; }

        public AutoresController(ApplicationDbContex context, IMapper mapper, IConfiguration configuration,
            IAlmacenadorArchivos almacenadorArchivos, ILogger<AutoresController> logger/*,
            IOutpuCacheStore outpuCacheStore*/, IServicioAutores servicioAutoresV1)
        {

            this.context = context;
            Mapper = mapper;
            this.configuration = configuration;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
            this.servicioAutoresV1 = servicioAutoresV1;
            //this.outpuCacheStore = outpuCacheStore;
        }

        //para usar los datos del appsetings.Development.json
        [HttpGet("configuraciones")]
        public ActionResult<string> ObtenerConfiguracion()
        {
            //return configuration["Apellido"];

            //return configuration["connectionStrings:defaultConnection"];

            //Para cambiar de desarrollo a producción: click derecho al proyecto >> propiedades >> Debug >> General >> ASPNETCORE_ENVIRONMENT = production, VariableDeAmbiente = variable de ambiente

            //return configuration["VariableDeAmbiente"];


            // desde el user secret, que solo funciona desde la maquina del desarrollador, no es parte del codigo fuente
            return configuration["apellido"];
        }

        [HttpGet(Name = "obtenerAutoresV2")] //api/autores
        //para que el endpoint lo pueda acceder alguin autenticaado
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // puede consumir este endpoint cualquiera
        [AllowAnonymous]

        //Para que almacene en cache, (Tags = [cache]) para que se lime el cache
        //[OutputCache(Tags = [cache])]

        //Filtro para ejecutar antes y despues del Request
        //[ServiceFilter<MiFiltroDeAccion>()]

        public async Task<IEnumerable<AutorDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            //var autores = await context.Autores.ToListAsync();

            /*var queryable = context.Autores.AsQueryable();
            await HttpContext.InsertarParametroPaginacionCabecera(queryable);
            var autores = await queryable.OrderBy(x => x.Id).Paginar(paginacionDTO).ToListAsync();
            var autoresDTO = Mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;*/

            return await servicioAutoresV1.Get(paginacionDTO);


            //mapea los campos de Autor en AutorDTO
            //return Mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpGet("{id:int}", Name = "obtenerAutorV2")]
        [AllowAnonymous]
        //[EndpointSummary("Obtiene autor por ID")] solo en .net 9
        //[EndpointDescription("Obtiene autor por ID. Incluye Libros.")] solo en .net 9

        //Para mostrar el tipo que puede retornar
        //[ProducesResponseType<AutorDTOConLibros>(StatusCodes.Status200OK)]
        //[ProducesResponseType<AutorDTOConLibros>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AutorDTOConLibros>> Get([FromRoute]/*[Description("el id del autor")]*/ int id, bool incluirLibros = false)
        {
            var queryable = context.Autores.AsQueryable();

            if (incluirLibros)
            {
                queryable = queryable.Include(x => x.AutoresLibros).ThenInclude(x => x.Libro);
            }

            var autor = await queryable
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }

            var dto = Mapper.Map<AutorDTOConLibros>(autor);

            //muestro la respuesta y los enlaces que puede accceder el usuario
            GenerarEnlaces(dto);

            return dto;
        }

        [HttpGet("filtro")]
        [AllowAnonymous]
        public async Task<ActionResult> Filtrar([FromQuery] AutorFiltroDTO autorFiltroDTO)
        {
            var queryable = context.Autores.AsQueryable();

            if (!string.IsNullOrEmpty(autorFiltroDTO.Nombre))
            {
                queryable = queryable.Where(x => x.Nombre.Contains(autorFiltroDTO.Nombre));
            }

            if (autorFiltroDTO.IncluirLibros)
            {
                queryable = queryable.Include(x => x.AutoresLibros).ThenInclude(x => x.Libro);
            }

            if (autorFiltroDTO.TieneFoto.HasValue)
            {
                if (autorFiltroDTO.TieneFoto.Value)
                {
                    queryable = queryable.Where(x => x.Foto != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Foto == null);
                }
            }

            if (autorFiltroDTO.TieneLibros.HasValue)
            {
                if (autorFiltroDTO.TieneLibros.Value)
                {
                    queryable = queryable.Where(x => x.AutoresLibros.Any());
                }
                else
                {
                    queryable = queryable.Where(x => !x.AutoresLibros.Any());
                }
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.TituloLibro))
            {
                queryable = queryable.Where(x => x.AutoresLibros.Any(x => x.Libro.Titulo.Contains(autorFiltroDTO.TituloLibro)));
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.CampoOrdenar))
            {
                var tipoOrden = autorFiltroDTO.OrdernarAscendente ? "ascending" : "descending";

                try
                {
                    queryable = queryable.OrderBy($"{autorFiltroDTO.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Nombre);
                    logger.LogError(ex.Message, ex);
                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Nombre);
            }

            var autores = await queryable.Paginar(autorFiltroDTO.PaginacionDTO).ToListAsync();

            if (autorFiltroDTO.IncluirLibros)
            {
                var autoresDTO = Mapper.Map<IEnumerable<AutorDTOConLibros>>(autores);
                return Ok(autoresDTO);
            }
            else
            {
                var autoresDTO = Mapper.Map<IEnumerable<AutorDTO>>(autores);
                return Ok(autoresDTO);
            }

        }

        private void GenerarEnlaces(AutorDTO autorDTO)
        {
            autorDTO.Enlaces.Add(new DatosHEATEOAS(
                enlace: Url.Link("obtenerAutor", new { id = autorDTO.Id }),
                descripcion: "self",
                metodo: "GET"));

            autorDTO.Enlaces.Add(new DatosHEATEOAS(
                enlace: Url.Link("actualizarAutor", new { id = autorDTO.Id }),
                descripcion: "autor-actualizar",
                metodo: "PUT"));

            autorDTO.Enlaces.Add(new DatosHEATEOAS(
                enlace: Url.Link("eliminarAutor", new { id = autorDTO.Id }),
                descripcion: "self",
                metodo: "DELETE"));
        }

        [HttpGet("{nombre}", Name = "obtenerAutorPorNombreV2")]
        public async Task<ActionResult<List<AutorDTO>>> Get(string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return Mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutorV2")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            //validacion en el controlador
            var ExisteAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);

            if (ExisteAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre: {autorCreacionDTO.Nombre}");
            }

            //var autor = new Autor()
            //{
            //    Nombre = autorCreacionDTO.Nombre
            //};

            //mapeo: agrego los valores el objeto AutorCreacionDTO a Autor
            //La configuracion el mapeo esta en: Utilidades >> AutoMapperProfile
            var autor = Mapper.Map<Autor>(autorCreacionDTO);

            context.Add(autor);
            await context.SaveChangesAsync();

            //Para limpiar la cache
            //await outputCacheStore.EvictByTagAsync(cache, default);

            var autorDTO = Mapper.Map<AutorDTO>(autor);

            //esto retorna una URL del objeto creado, llama al endpoint con el name, pasa el Id, y retornar el objeto 
            return CreatedAtRoute("obtenerAutorV2", new { id = autor.Id }, autorDTO);
        }

        [HttpPost("con-foto")]
        [AllowAnonymous]
        //[FromForm] para recibir archivos
        public async Task<ActionResult> PostConFoto([FromForm] AutorCreacionDTOconFoto autorCreacionDTOconFoto)
        {
            var ExisteAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTOconFoto.Nombre);

            if (ExisteAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre: {autorCreacionDTOconFoto.Nombre}");
            }

            var autor = Mapper.Map<Autor>(autorCreacionDTOconFoto);

            if (autorCreacionDTOconFoto.Foto is not null)
            {
                var url = await almacenadorArchivos.Almacenar(contenedor, autorCreacionDTOconFoto.Foto);
                autor.Foto = url;
            }

            context.Add(autor);
            await context.SaveChangesAsync();

            //await outputCacheStore.EvictByTagAsync(cache, default);

            var autorDTO = Mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("obtenerAutorV2", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutorV2")]
        [AllowAnonymous]
        public async Task<ActionResult> Put([FromForm] AutorCreacionDTOconFoto autorCreacionDTO, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = Mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            if (autorCreacionDTO.Foto is not null)
            {
                var fotoActual = await context.Autores.Where(x => x.Id == id).Select(x => x.Foto).FirstAsync();
                var url = await almacenadorArchivos.Editar(fotoActual, contenedor, autorCreacionDTO.Foto);
                autor.Foto = url;
            }

            context.Update(autor);

            //await outputCacheStore.EvictByTagAsync(cache, default);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "eliminarAutorV2")]
        [AllowAnonymous]
        public async Task<ActionResult> Delete(int id)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }

            context.Remove(autor);

            await context.SaveChangesAsync();

            await almacenadorArchivos.Borrar(autor.Foto, contenedor);

            return NoContent();
        }
    }
}
