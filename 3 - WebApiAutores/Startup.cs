using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApiAutores.Middawares;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using WebApiAutores.Servicios;
using WebApiAutores.Swagger;
using WebApiAutores.Utilidades;
using Microsoft.AspNetCore.Diagnostics;
using WebApiAutores.Entidades;
using WebApiAutores.Servicios.V1;
using WebApiAutores.Utilidades.V1;

namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configuration) 
        {
            //para limpiar los mapeos automaticos de los claims de aspNet
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllers(x => x.Filters.Add(typeof(FiltroDeExcepcion))).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();
                        
            //Para agrgar filtros globales, no hay que usar si se usa cache
            services.AddControllers(x =>
            {
                x.Filters.Add<FiltroTiempoEjecucion>();
                //para agrpar por version el swagger
                x.Conventions.Add(new ConvercionAgrupaPorVersion());

            }).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();
            /*A partir de.Net6 se puede utilizar el OutputCache
            services.AddOutputCache(opciones =>
            {
                //Tiempo de vida del cache
                opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(15);
            });*/

            //Usar con Redis, comentar el de arriba
            //services.AddStackExchangeRedisOutputCache(opciones =>
            //{
            //    opciones.Configuration = builder.Configuration.GetConnectionString("redis");
            //});

            // servicio de autenticacion  con Bearer, mas la configuracion del token
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(x => x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew = TimeSpan.Zero
                });

            services.AddDbContext<ApplicationDbContex>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            services.AddAutoMapper(typeof(Startup));

            //para crear el servicio de identity, IdentityRole si queremos usar roles
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContex>()
                .AddDefaultTokenProviders();


            services.AddEndpointsApiExplorer();

            //configuramos el Swagger para utizar token
            services.AddSwaggerGen(c =>
            {
                //informacion extra
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "v1",
                    Title = "Biblioteca API",
                    Description = "API de libros",
                    Contact = new OpenApiContact{
                        Email="nf@gmail.com",
                        Name = "Nicolas Fediuk",
                        Url = new Uri("https://google.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name  = "MIT",
                        Url = new Uri("https://opensource.org/license/mit/")
                        
                    }
                    
                });

                c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "v2",
                    Title = "Biblioteca API",
                    Description = "API de libros",
                    Contact = new OpenApiContact
                    {
                        Email = "nf@gmail.com",
                        Name = "Nicolas Fediuk",
                        Url = new Uri("https://google.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/license/mit/")

                    }

                });

                //configuracion para usar JWT con swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                //asi solo le pongo el cadado a cada endpint en swagger
                //c.OperationFilter<FiltroAutorizacion>();

                //asi si no le quiero poner el filtro de swagger a cada endpoint
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            //para ahcer autorizaciones basadas en Claims. y un usuario tiene un claims "EsAdmin" tiene permisos
            services.AddAuthorization(x =>
            {
                x.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
               // x.AddPolicy("EsVendedor", politica => politica.RequireClaim("EsVendedor"));
            });

            //servicios de proteccion de datos
            services.AddDataProtection();

            var origenesPermitidos = Configuration.GetSection("origenesPermitidos").Get<string[]>();

            //Configuracion de CORS
            //https://apirequest.io/ para hacer el test
            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder => {

                    //Para que culaquier origen pueda acceder a los endpoint y a las cabeceras
                    //builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();

                    //cuales URL entran acceso de nuestras apis
                    //que Metodos (httpGet, httpPost, etc)
                    //Permitir cualquier cabecera
                    //.WithExposedHeaders() podemos agregarlo para exponer nuestras cabeceas
                    //builder.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader();

                    //para exponer la cabecera
                    builder.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("cantidad-total-registros");

                });
            });

            //declaramos el servicio de hash que creamos 
            services.AddTransient<HashService>();

            //toma los valores del seccion1 del appsetting y mapea los valores a PersonasOpciones
            services.AddOptions<PersonaOpciones>().Bind(/*builder.*/Configuration.GetSection(PersonaOpciones.Seccion))
                //para permitir las validaciones por el modelo
                .ValidateDataAnnotations();
                //.ValidateOnStart();

            services.AddOptions<TarifasOpciones>().Bind(/*builder.*/Configuration.GetSection(TarifasOpciones.Seccion))
                //para permitir las validaciones por el modelo
                .ValidateDataAnnotations();
                //.ValidateOnStart();

            services.AddSingleton<PagosProcesamiento>();

            //servicio para almacenar archivos en azure
            //services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosAzure>();

            //servicio para almacenar archivos local
            services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivoLocal>();

            //para usar el IHttpContextAccessor
            services.AddHttpContextAccessor();

            //Servicio de Filtros
            services.AddScoped<MiFiltroDeAccion>();

            //Filtro generico usando LibroCreacionDTO 
            services.AddScoped<FiltroValidacionLibro>();

            //Servicio para reducir codigo entre las versiones
            services.AddScoped<WebApiAutores.Servicios.V1.IServicioAutores, WebApiAutores.Servicios.V1.ServicioAutores>();

            services.AddScoped<WebApiAutores.Servicios.V1.IGeneradorEnlaces, WebApiAutores.Servicios.V1.GeneradorEnlaces>();

            services.AddScoped<HATEOASFilterAttribute>();

            services.AddScoped<HATEOASAutorAttribute>();

            services.AddScoped<HATEOASAutoresAttribute>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(opciones => {
                    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API Autores V1");
                    opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "Web API Autores V2");
                });
            }

            //Para guardar errores de la app en la base de datos
            /*app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>{
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var excepcion = exceptionHandlerFeature?.Error!;

                var error = new Error()
                {
                    MensajeError = excepcion.Message,
                    StrackTrace = excepcion.StackTrace,
                    Fecha = DateTime.UtcNow
                };

                var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContex>();
                dbContext.Add(error);
                await dbContext.SaveChangesAsync();
                await Results.InternalServerError(new
                {
                    tipo = "error",
                    mensaje = "Ha ocurrido un error inesperado",
                    estatus = 500
                }).ExecuteAsync(context);
            }));*/

            //para usar Swagger desde produccion
            //app.UseSwagger();
            //app.UseSwaggerUI();

            app.UseHttpsRedirection();
            
            //para servir los archivos del wwwroot
            app.UseStaticFiles();

            //activamos el CORS en nuestro middlaware
            app.UseCors();

            //app.UseOutputCache();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
