using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios.V1;

namespace WebApiAutores.Utilidades.V1
{
    public class HATEOASAutoresAttribute : HATEOASFilterAttribute
    {
        private readonly IGeneradorEnlaces generadorEnlaces;
        private readonly HATEOASFilterAttribute filterAttribute;

        public HATEOASAutoresAttribute(IGeneradorEnlaces generadorEnlaces, HATEOASFilterAttribute filterAttribute)
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
            var modelo = result.Value as List<AutorDTO> ?? throw new ArgumentException("Se esperaba una instacion de List<AutorDTO>");
            context.Result = new ObjectResult(await generadorEnlaces.GenerarEnlaces(modelo);
            await next();
        }
    }
}
