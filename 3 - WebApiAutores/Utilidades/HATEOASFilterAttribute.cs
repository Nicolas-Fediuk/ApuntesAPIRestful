using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Utilidades
{
    public class HATEOASFilterAttribute : ResultFilterAttribute
    {
        public bool IncluirHATEOAS(ResultExecutingContext context)
        {
            if (context.Result is not ObjectResult result || !EsRspuestaExistosa(result))
            {
                return false;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("IncluirHATEOAS", out var cabecera))
            {
                return false;
            }

            return string.Equals(cabecera, "Y", StringComparison.OrdinalIgnoreCase);
        }

        private bool EsRspuestaExistosa(ObjectResult result)
        {
            if (result.Value == null)
            {
                return false;
            }

            //Si el StatusCode comienza con 2 retorna false
            if (result.StatusCode.HasValue && !result.StatusCode.Value.ToString().StartsWith("2"))
            {
                return false;
            }

            return true;
        }
    }
}
