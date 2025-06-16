using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")] //tambien puede ser "api/[controller]" y toma el valor por defecto
    public class AutoresController : ControllerBase
    {

        private readonly ApplicationDbContex context;
        private readonly ILogger<AutoresController> logger;

        public AutoresController(ApplicationDbContex context, ILogger<AutoresController> logger) {

            this.context = context;
            this.logger = logger;
        }

        
        //puedo tenes varias rutas para el mismo endpoint
        [HttpGet] //api/autores
        [HttpGet("listado")] //api/autores/listado
        [HttpGet("/listado")] // listado
        // despeus de hacer la peticion http, si lo ahce otra ves antes de 10 segundos la info viene de la cache, la info no varia, despues de los 10 seg si
        // Guarda en memoria que caduca dentro de 10 seg
        //Muy bueno
        //[ResponseCache(Duration = 10)]

        [ServiceFilter(typeof(MiFiltroDeAccion))]
        public async Task<List<Autor>> Get()
        {
            logger.LogInformation("EStamos obteniendo los autores");
            return await context.Autores.Include(x => x.Libros).ToListAsync();
        }

        [HttpGet("primero")] //api/autores/primero   [FromHeader] cuando los valores viene de la cabecera
                             // api/autores/primero?nombre=nicolas&apellido=fediuk ? = indica el inicio de los queryString [FromQuery] queryString, llave y valor
                                                                                // & agrega otro parametro al queryString
        public async Task<Autor> PrimerAutor([FromHeader] int miValor, [FromQuery] string nombre)
        {
            return await context.Autores.FirstOrDefaultAsync();
        }

        [HttpGet("{id:int}")] // [FromRoute] indica que el parametro viene de la ruta
        public async Task<ActionResult<Autor>> Get([FromRoute] int id)
        {
            //throw new NotImplementedException();

            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if(autor is null)
            {
                return NotFound();
            }

            return autor;
        }

        [HttpGet("{nombre}")]
        //[HttpGet("{nombre}/{apellido}")] para tener mas parametros
        //[HttpGet("{nombre}/{apellido?}")] para tener parametro opcional 
        //[HttpGet("{nombre}/{apellido=fediuk}")] para tener parametro por defecto
        public async Task<ActionResult<Autor>> Get(string nombre)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Nombre.Contains(nombre));

            if (autor is null)
            {
                return NotFound();
            }

            return autor;
        }

        [HttpPost] // [FromBody] indica que el parametro viene del cuerpo de la peticion http 
        public async Task<ActionResult> Post([FromBody] Autor autor)
        {
            //validacion en el controlador
            var ExisteAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autor.Nombre);

            if (ExisteAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre: {autor.Nombre}");
            }


            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        //paraemtro de ruta
        [HttpPut("{id:int}")] //api/autores/1
        public async Task<ActionResult> Put(Autor autor, int id)
        {
            if(autor.Id != id)
            {
                return BadRequest("El id del autor no coincide con el id de la URL");
            }

            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Update(autor);
            await context.SaveChangesAsync();    
            return Ok();
        }

        //paraemtro de ruta
        [HttpDelete("{id:int}")]//api/autores/1
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if(!existe)
            {
                return NotFound();  

            }

            context.Remove(new Autor() { Id = id });

            await context.SaveChangesAsync();

            return Ok();
        }

    }
}
