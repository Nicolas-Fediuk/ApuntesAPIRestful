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

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    // El controlador solo se peude acceder con un usuario logeado, menos los endpoint que tengan [AllowAnonymous]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {

        private readonly ApplicationDbContex context;
        private readonly IConfiguration configuration;

        public IMapper Mapper { get; }

        public AutoresController(ApplicationDbContex context, IMapper mapper, IConfiguration configuration) {

            this.context = context;
            Mapper = mapper;
            this.configuration = configuration;
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

        [HttpGet(Name = "obtenerAutores")] //api/autores
        //para que el endpoint lo pueda acceder alguin autenticaado
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // puede consumir este endpoint cualquiera
        [AllowAnonymous]
        public async Task<List<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();

            //mapea los campos de Autor en AutorDTO
            return Mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpGet("{id:int}", Name = "obtenerAutor")] 
        public async Task<ActionResult<AutorDTOConLibros>> Get([FromRoute] int id)
        {
            var autor = await context.Autores
                .Include(autorDB => autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(autor is null)
            {
                return NotFound();
            }

            var dto =  Mapper.Map<AutorDTOConLibros>(autor);

            //muestro la respuesta y los enlaces que puede accceder el usuario
            GenerarEnlaces(dto);    

            return dto;
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

        [HttpGet("{nombre}", Name = "obtenerAutorPorNombre")]
        public async Task<ActionResult<List<AutorDTO>>> Get(string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return Mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutor")] 
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

            var autorDTO = Mapper.Map<AutorDTO>(autor);

            //esto retorna una URL del objeto creado, llama al endpoint con el name, pasa el Id, y retornar el objeto 
            return CreatedAtRoute("obtenerAutor", new {id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutor")] 
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = Mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;   

            context.Update(autor);
            await context.SaveChangesAsync();    
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "eliminarAutor")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if(!existe)
            {
                return NotFound();  
            }

            context.Remove(new Autor() { Id = id });

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
