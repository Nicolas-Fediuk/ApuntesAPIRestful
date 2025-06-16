using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Xml.Linq;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    //dependicia: un comentario depende de un libro
    [Route("api/libros/{libroId:int}/comentarios")]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContex contex;
        private readonly UserManager<IdentityUser> userManager;

        public IMapper Mapper { get; }

        public ComentariosController(ApplicationDbContex contex, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.contex = contex;
            Mapper = mapper;
            this.userManager = userManager;
        }
        [HttpGet(Name = "obtenerComentarioLibro")]
        public async Task<ActionResult<List<ComentarioDto>>> Get(int libroId)
        {
            var existeLibro = await contex.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentarios = await contex.Comentarios.Where(x => x.LibroId == libroId).ToListAsync();

            return Mapper.Map<List<ComentarioDto>>(comentarios);
        }

        [HttpGet("{id:int}", Name = "obtenerComentario")]
        public async Task<ActionResult<ComentarioDto>> GetPorId(int id)
        {
            var comentario = await contex.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if(comentario is null)
            {
                return NotFound();
            }

            return Mapper.Map<ComentarioDto>(comentario);
        }

        [HttpPost(Name = "crearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            //busco el mail del usuario loggeados. SI O SI FUNCIONA BAJO EL AUTHORIZE.
            //sino, arroja un 500, para arreglar eso hay que poner el endpoin como un [AllowAnonymous]
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            
            //guardo el mail
            var email = emailClaim.Value;

            //busco el usuario con el mail
            var usuario = await userManager.FindByEmailAsync(email);

            //obtengo el ID del usuario
            var usuarioId = usuario.Id;
            var existeLibro = await contex.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();  
            }

            var comentario = Mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;    
            comentario.UsuarioId = usuarioId;
            contex.Add(comentario); 
            await contex.SaveChangesAsync();

            var comentarioDTO = Mapper.Map<ComentarioDto>(comentario);

            return CreatedAtRoute("obtenerComentario",new {id = comentario.Id, libroId = comentario.LibroId }, comentarioDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarComentario")]

        public async Task<ActionResult> Put(int libroId, int id, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await contex.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var existeComentario = await contex.Comentarios.AnyAsync(x => x.Id == id);

            if (!existeComentario)
            {
                return NotFound();  
            }

            var comentario = Mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.Id = id;
            comentario.LibroId = libroId; 
            contex.Update(comentario);  
            await contex.SaveChangesAsync();
            return NoContent();
        }
    }
}
