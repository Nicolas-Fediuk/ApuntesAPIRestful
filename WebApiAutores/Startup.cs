using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middawares;
using WebApiAutores.Servicios;

namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configuration) 
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // los servicios es la resoliucion de una dependencia configurada en el sistema de inyeccion de dependencia
            // creamos los filtros globales
            services.AddControllers(x => x.Filters.Add(typeof(FiltroDeExcepcion))).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            //cuando alguna clase requiera un IServicio que intancia la clase ServicioA,
            // se usa para no instaciar las dependencias de las dependencias de las clases
            //AddTransient: Un aneva instacia de ServicioA, simple funciones, cambia de ruta http y muere la instacia
            //AddScope: el timepo de vida aumenta, dentro del mismo contexto http, si se cambia de ruta, esta permanece
            //AddSingleton: Siempre la misma instacia, para chache, 
            services.AddTransient<IServicio, ServicioA>();

            services.AddResponseCaching();

            services.AddHostedService<EscribirEnArchivo>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

            //una clase como servicio
            //services.AddTransient<ServicioA>();

            services.AddDbContext<ApplicationDbContex>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            services.AddTransient<MiFiltroDeAccion>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            
        }

        //Middleware: tuberia de peticiones http, es una cadena de procesos conectados de tal forma que la salida de cada 
        //elemento de la cadena es la entrada del proximo 
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            //asi podemos hacer un middleware en un clase y usarla
            //app.UseMiddleware<LoggearRespuestaHTTPMiddleware>();
            //app.UseLoggearRespuestaHTTP();

            //log de respuesta http
            //Use: es como el map, pero no interrumpe los otros procesos, solo es para agregar mis propios middleware
            //app.Use(async (contexto, siguinte) =>
            //{
            //    using (var ms = new MemoryStream())
            //    {
            //        var cueroOriginalRespuesta = contexto.Response.Body;
            //        contexto.Response.Body = ms;

            //        await siguinte.Invoke();

            //        ms.Seek(0, SeekOrigin.Begin);
            //        string respuesta = new StreamReader(ms).ReadToEnd();
            //        ms.Seek(0, SeekOrigin.Begin);

            //        await ms.CopyToAsync(cueroOriginalRespuesta);
            //        contexto.Response.Body = cueroOriginalRespuesta;

            //        logger.LogInformation(respuesta);
            //    }
            //});

            //Map: bifurcacion en la tuberia, si el usuario usa la ruta "/ruta1" solo se va a ejecutar los middleware que estandentro de el
            app.Map("/ruta1", app =>
            {
                //tomamos todas las peticiones http y retornamos un string
                // Run cancela la ejecucion de los siguientes middleware, solo se ejecuta el y ya
                app.Run(async contexto => await contexto.Response.WriteAsync("estoy interceptando la tuberia"));
            });
            

            //si estamos en desarrollo agregamos los middleware de swagger
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseResponseCaching();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //aca mapeo los controladores
                endpoints.MapControllers();
            });
        }
    }
}
