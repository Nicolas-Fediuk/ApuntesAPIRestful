using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApiAutores.Utilidades
{
    public static class ModelStateDictionaryExtensions
    {
        public static BadRequestObjectResult ConstruirDetalleDeProblema(this ModelStateDictionary modelState)
        {
            var DetalleDeProblema = new ValidationProblemDetails(modelState) 
            { 
                Title="Ocurrieron uno o mas errores de validacion",
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(DetalleDeProblema);


        }
    }
}
