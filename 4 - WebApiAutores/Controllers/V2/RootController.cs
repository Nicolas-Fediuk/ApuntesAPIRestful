using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    [Route("api/v2")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RootController : ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRootV2")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DatosHEATEOAS>>> Get()
        {
            var datosHeateoas = new List<DatosHEATEOAS>();

            var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");

            datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("ObtenerRoot", new { }), descripcion: "self", metodo: "GET"));

            //en los links genereales se crear los endpoint que no tienen dependecias y los POST y GET
            datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("obtenerAutores", new { }), descripcion: "autores", metodo: "GET"));

            //asi valido si el usuario es admin
            if (esAdmin.Succeeded)
            {
                datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("crearAutor", new { }), descripcion: "crear-autor", metodo: "POST"));
                datosHeateoas.Add(new DatosHEATEOAS(enlace: Url.Link("crearLibro", new { }), descripcion: "crear-libro", metodo: "POST"));
            }


            return datosHeateoas;
        }
    }
}
