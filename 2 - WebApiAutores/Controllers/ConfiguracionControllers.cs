using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/configuraciones")]
    public class ConfiguracionControllers : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly PagosProcesamiento pagosProcesamiento;
        private readonly PersonaOpciones _opcionesPersona;
        private readonly IConfigurationSection seccion1;
        private readonly IConfigurationSection seccion2;

                                                                     //IOptionsSnapshot: como cambian los valores esta la acctualiza
                                                                     //ya que Ioptions guarda en cache lo valores
                                                                     //es un servicio singleton
        public ConfiguracionControllers(IConfiguration configuration, IOptions/*Snapshot*/<PersonaOpciones> opcionesPersona,
            PagosProcesamiento pagosProcesamiento)
        {
            seccion1 = configuration.GetSection("seccion1");
            seccion2 = configuration.GetSection("seccion2");
            this.configuration = configuration;
            this.pagosProcesamiento = pagosProcesamiento;
            this._opcionesPersona = opcionesPersona.Value;
        }

        [HttpGet("options-monitor")]
        public ActionResult GetTarifas()
        {
            return Ok(pagosProcesamiento.ObtenerTarifas());
        }

        [HttpGet("seccion1_opciones")]
        public ActionResult GetSeccion1Opciones()
        {
            return Ok(_opcionesPersona);
        }

        //obtengo todos los valores
        [HttpGet("obtenerTodos")]
        public ActionResult GetObtenerTodos()
        {
            var hijos = seccion2.GetChildren().Select(x => $"{x.Key}: {x.Value}");

            return Ok(new { hijos });
        }

        [HttpGet("seccion1")]
        public ActionResult GetSeccion1()
        {
            var nombre = seccion1.GetValue<string>("nombre");
            var edad = seccion1.GetValue<int>("edad");

            return Ok(new { nombre, edad });
        }

        [HttpGet("seccion2")]
        public ActionResult GetSeccion2()
        {
            var nombre = seccion2.GetValue<string>("nombre");
            var edad = seccion2.GetValue<int>("edad");

            return Ok(new { nombre, edad });
        }

        [HttpGet("proveedores")]
        public ActionResult GetProveedor()
        {
            var valor = configuration.GetValue<string>("quienSoy");
            return Ok(new {valor });
        }
    }
}
