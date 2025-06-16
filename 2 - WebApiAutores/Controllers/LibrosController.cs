using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContex context;
        public IMapper Mapper { get; }

        public LibrosController(ApplicationDbContex context, IMapper mapper)
        {
            this.context = context;
            Mapper = mapper;
        }

        [HttpGet]
        public async Task<List<LibroDTO>> Get()
        {
            var libros = await context.Libros.ToListAsync();

            return Mapper.Map<List<LibroDTO>>(libros);
        }



        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            //que traiga los libros y los comentarios
            var libro = await context.Libros
                .Include(libroDB => libroDB.AutoresLibros)
                .ThenInclude(AutorLibroDB => AutorLibroDB.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(libro is null)
            {
                return NotFound();  
            }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return Mapper.Map<LibroDTOConAutores>(libro); 
        }

        [HttpPost(Name = "crearLibro")]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if(libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se peude crear un libro sin autores");
            }
            
            //me traigo los id de autores de la bd que sea igual a los ids del parametro del endpoint
            var autoresIds = await context.Autores.Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(); 

            if(autoresIds.Count != libroCreacionDTO.AutoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }
            
            var Libro = Mapper.Map<Libro>(libroCreacionDTO);

            AsignarOrdenAutores(Libro);

            context.Add(Libro);
            await context.SaveChangesAsync();

            var libroDTO = Mapper.Map<LibroDTO>(Libro);

            return CreatedAtRoute("obtenerLibro", new {id = Libro.Id }, libroDTO);
        }

        [HttpPut("{id:int}", Name = "actuallizarLibro")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libroDB = await context.Libros
                          .Include(x => x.AutoresLibros)
                          .FirstOrDefaultAsync(x => x.Id == id);

            if(libroDB is null)
            {
                return NotFound();
            }

            AsignarOrdenAutores(libroDB);

            //el AutoMapper ya lo actualiza
            libroDB = Mapper.Map(libroCreacionDTO, libroDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }

        //tengo que instalar el Microsoft.AspNetCore.Mvc.Newtosoft.Json y agregar el srevicio en el services.AddControllers
        //parametros 
        //[
        //  {
        //    "path": "/titulo",
        //    "op": "replace",
        //    "value": "Titulo modificado desde Patch"
        //  }
        //]
        [HttpPatch("{id:int}", Name = "PatchLibro")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPachDTO> patchDocument)
        {
            if(patchDocument is null)
            {
                return BadRequest();
            }

            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);    

            if(libroDB is null)
            {
                return NotFound();
            }

            var libroDTO = Mapper.Map<LibroPachDTO>(libroDB);

            patchDocument.ApplyTo(libroDTO, ModelState);

            var EsValido = TryValidateModel(libroDTO);

            if (!EsValido)
            {
                return BadRequest();
            }

            Mapper.Map(libroDTO, libroDB);

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "EliminarLibro")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Libro() { Id = id });

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
