using Microsoft.AspNetCore.Authorization;
using System;
using WebApiAutores.DTOs;

namespace WebApiAutores.Servicios.V1
{
    public class GeneradorEnlaces : IGeneradorEnlaces
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public GeneradorEnlaces(LinkGenerator linkGenerator, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            this.linkGenerator = linkGenerator;
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task GenerarEnlaces(AutorDTO autorDTO)
        {
            var usuario = httpContextAccessor.HttpContext.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "esAdmin");
            GenerarEnlaces(autorDTO, esAdmin.Succeeded);
        }

        public async Task<ColeccionDeRecursosDTO<AutorDTO>> GenerarEnlaces(List<AutorDTO> autores)
        {
            var resultado = new ColeccionDeRecursosDTO<AutorDTO> { Valores = autores };

            var usuario = httpContextAccessor.HttpContext.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "esAdmin");

            foreach (var dto in autores)
            {
                GenerarEnlaces(dto,esAdmin.Succeeded);
            }

            resultado.Enlaces.Add(new DatosHEATEOAS(
                enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext, "obtenerAutorV1", new { }),
                descripcion: "self",
                metodo: "GET"));

            if (esAdmin.Succeeded)
            {
                resultado.Enlaces.Add(new DatosHEATEOAS(
                enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext, "crearAutorV1", new { }),
                descripcion: "crear-autor",
                metodo: "POST"));

                resultado.Enlaces.Add(new DatosHEATEOAS(
                    enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext, "CrearAutorConFotoV1", new { }),
                    descripcion: "crear-autorConFoto",
                    metodo: "POST"));
            }

            return resultado;
        }

        private void GenerarEnlaces(AutorDTO autorDTO, bool esAdmin)
        {
            autorDTO.Enlaces.Add(new DatosHEATEOAS(
                enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext, "obtenerAutorV1", new { id = autorDTO.Id }),
                descripcion: "self",
                metodo: "GET"));

            if (esAdmin)
            {
                autorDTO.Enlaces.Add(new DatosHEATEOAS(
                enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext, "actualizarAutorV1", new { id = autorDTO.Id }),
                descripcion: "autor-actualizar",
                metodo: "PUT"));

                autorDTO.Enlaces.Add(new DatosHEATEOAS(
                    enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext, "eliminarAutorV1", new { id = autorDTO.Id }),
                    descripcion: "autor-eliminar",
                    metodo: "DELETE"));
            }


        }
    }
}
