using Microsoft.AspNetCore.Mvc.Filters;
using WebApiAutores.DTOs;

namespace WebApiAutores.Utilidades
{

    //Para hacer un filtro generico para validaciones que recibe como parametro un objeto de una entidad
    public class FiltroValidacionLibro : IAsyncActionFilter
    {
        private readonly ApplicationDbContex dbContext;

        public FiltroValidacionLibro(ApplicationDbContex dbContex) 
        {
            this.dbContext = dbContex;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Para validar que en parametro se envie un objeto del tipo LibroCreacionDTO
            if (!context.ActionArguments.TryGetValue("libroCreacionDTO", out var value) || value is not LibroCreacionDTO libroCreacionDTO)
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es valido");
                context.Result = context.ModelState.ConstruirDetalleDeProblema();
                return;
            }

            if (libroCreacionDTO.AutoresIds == null)
            {
                context.Result = context.ModelState.ConstruirDetalleDeProblema();
                return;
            }

            await next();

        }
    }
}
