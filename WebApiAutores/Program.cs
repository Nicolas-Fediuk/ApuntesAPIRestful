using WebApiAutores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//servicios
//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

//middleware

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//configuaramos el servicio ILogger en la clase Startup
var servicioLoggre = (ILogger < Startup >) app.Services.GetService(typeof(ILogger<Startup>));

startup.Configure(app, app.Environment,servicioLoggre);

app.Run();
