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
            services.AddControllers(x => x.Filters.Add(typeof(FiltroDeExcepcion))).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();

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
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

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

            //Configuracion de CORS
            //https://apirequest.io/ para hacer el test
            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder => {

                    //cuales URL entran acceso de nuestras apis
                    //que Metodos (httpGet, httpPost, etc)
                    //Permitir cualquier cabecera
                    //.WithExposedHeaders() podemos agregarlo para exponer nuestras cabeceas
                    builder.WithOrigins("https://apirequest.io").AllowAnyMethod().AllowAnyHeader();

                });
            });

            //declaramos el servicio de hash que creamos 
            services.AddTransient<HashService>();

            //toma los valores del seccion1 del appsetting y mapea los valores a PersonasOpciones
            services.AddOptions<PersonaOpciones>().Bind(/*builder.*/Configuration.GetSection(PersonaOpciones.Seccion))
                //para permitir las validaciones por el modelo
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<TarifasOpciones>().Bind(/*builder.*/Configuration.GetSection(TarifasOpciones.Seccion))
                //para permitir las validaciones por el modelo
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<PagosProcesamiento>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //para usar Swagger desde produccion
            //app.UseSwagger();
            //app.UseSwaggerUI();

            app.UseHttpsRedirection();

            //activamos el CORS en nuestro middlaware
            app.UseCors();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
