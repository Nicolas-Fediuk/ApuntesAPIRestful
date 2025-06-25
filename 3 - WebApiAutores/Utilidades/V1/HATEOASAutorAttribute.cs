using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios.V1;

namespace WebApiAutores.Utilidades.V1
{
    public class HATEOASAutorAttribute
    {
        private readonly IGeneradorEnlaces generadorEnlaces;
        private readonly HATEOASFilterAttribute filterAttribute;

        public HATEOASAutorAttribute(IGeneradorEnlaces generadorEnlaces, HATEOASFilterAttribute filterAttribute) 
        {
            this.generadorEnlaces = generadorEnlaces;
            this.filterAttribute = filterAttribute;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var incluirHATEOAS = filterAttribute.IncluirHATEOAS(context);

            if (!incluirHATEOAS)
            {
                await next();
                return;
            }

            var result = context.Result as ObjectResult;
            var modelo = result.Value as AutorDTO ?? throw new ArgumentException("Se esperaba una instacion de AutorDTO");
            await generadorEnlaces.GenerarEnlaces(modelo);
            await next();
        }
    }
}
