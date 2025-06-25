using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class RootController : ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRootV1")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DatosHEATEOAS>>> Get()
        {
            var datosHeateoas = new List<DatosHEATEOAS>();

            //para saber si es admin
            var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");

            //Las acciones que cualquiera pueda realizar

            //new{}: pasamos los parametros
            datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("ObtenerRootV1", new { }), descripcion: "self", metodo: "GET"));

            //en los links genereales se crear los endpoint que no tienen dependecias, ni parametros y los POST y GET
            datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("ObtenerAutoresV1", new { }), descripcion: "autores-obtener", metodo: "GET"));

            
            //Acciones que solo los admin pueden hacer
                        
            //asi valido si el usuario es admin
            if (esAdmin.Succeeded)
            {
                datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("crearAutor", new { }), descripcion: "crear-autor", metodo: "POST"));
                datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("crearLibro", new { }), descripcion: "crear-libro", metodo: "POST"));
            }

            //si el usuario esta loggeado
            if (User.Identity.IsAuthenticated)
            {
                datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("renovarTokenV1", new { }), descripcion: "renovar-Token-V1", metodo: "GET"));
            }


            return datosHeateoas;
        }
    }
}
