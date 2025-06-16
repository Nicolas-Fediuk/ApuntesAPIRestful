using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Utilidades
{
    //Para agregar respuestas personalizadas en la cabecera a las peticiones http
    public class FiltroAgregarCabeceraAttribute : ActionFilterAttribute
    {
        private readonly string nombre;
        private readonly string valor;

        public FiltroAgregarCabeceraAttribute(string nombre, string valor)
        {
            this.nombre = nombre;
            this.valor = valor;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //Antes de la ejecucion de la accion
            context.HttpContext.Response.Headers.Append(valor, nombre); 
            base.OnResultExecuting(context);
            //Despues de la ejecucion de la accion
        }
    }
}
