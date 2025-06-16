using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace WebApiAutores.Utilidades
{
    //Filtro global
    public class FiltroTiempoEjecucion : IAsyncActionFilter
    {
        private readonly ILogger<FiltroTiempoEjecucion> logger;

        public FiltroTiempoEjecucion(ILogger<FiltroTiempoEjecucion> logger)
        {
            this.logger = logger;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Antes de la ejecucion
            var stopWatch = Stopwatch.StartNew();
            //para saber el nombre de la accion
            logger.LogInformation($"INICION accion: {context.ActionDescriptor.DisplayName}");

            await next();

            //Despues de la ejecucion
            stopWatch.Stop();
            logger.LogInformation($"FIN accion: {context.ActionDescriptor.DisplayName} - Tiempo: {stopWatch.ElapsedMilliseconds} ms");
        }
    }
}
