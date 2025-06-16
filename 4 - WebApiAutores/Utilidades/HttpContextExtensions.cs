using Microsoft.EntityFrameworkCore;

namespace WebApiAutores.Utilidades
{

    //una extension de HttpContext
    public static class HttpContextExtensions
    {
        public async static Task InsertarParametroPaginacionCabecera<T>(this HttpContext httpContext, 
            IQueryable<T> queryable)
        {
            if(httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));   
            }

            double cantidad = await queryable.CountAsync();

            //para agregar a la cabecera la respuesta la cantidad de registro
            httpContext.Response.Headers.Append("cantidad-total-registros",cantidad.ToString());
        }
    }
}
