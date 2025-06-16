namespace WebApiAutores.Middawares
{

    public static class LoggearRespuestaHTTPMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggearRespuestaHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoggearRespuestaHTTPMiddleware>();
        }
    }

    public class LoggearRespuestaHTTPMiddleware
    {
        private readonly RequestDelegate siguiente;
        private readonly ILogger<LoggearRespuestaHTTPMiddleware> logger;

        //queremos invocar los siguientes middleware de la tuberia 
        public LoggearRespuestaHTTPMiddleware(RequestDelegate siguiente, ILogger<LoggearRespuestaHTTPMiddleware> logger)
        {
            this.siguiente = siguiente;
            this.logger = logger;
        }

        //invoke o invokeAsync

        public async Task InvokeAsync(HttpContext contexto)
        {
            using (var ms = new MemoryStream())
            {
                var cueroOriginalRespuesta = contexto.Response.Body;
                contexto.Response.Body = ms;

                await siguiente(contexto);

                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cueroOriginalRespuesta);
                contexto.Response.Body = cueroOriginalRespuesta;

                logger.LogInformation(respuesta);
            }
        }

    }
}
