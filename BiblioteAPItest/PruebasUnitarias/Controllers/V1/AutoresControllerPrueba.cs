using BiblioteAPItest.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiAutores.Controllers.V1;
using WebApiAutores.Entidades;
using WebApiAutores.Servicios;
using WebApiAutores.Servicios.V1;

namespace BiblioteAPItest.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPrueba : BasePruebas
    {
        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorIdNoExiste()
        {
            //Prepacion, paso las instancias del controlador
            var nombreDB = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreDB);
            var mapper = ConfigurarAutoMapper();

            IAlmacenadorArchivos almacenadorArchivos = null!;
            IConfiguration configuration = null!;
            ILogger<AutoresController> logger = null!;
            IServicioAutores servicioAutores = null!;

            var controller = new AutoresController(context, mapper, configuration, almacenadorArchivos, logger, servicioAutores);

            //Prueba
            var respuesta = await controller.Get(1);

            //Verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado.StatusCode);
        }

        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorIdExiste()
        {
            //Prepacion, paso las instancias del controlador
            var nombreDB = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreDB);
            var mapper = ConfigurarAutoMapper();

            IAlmacenadorArchivos almacenadorArchivos = null!;
            IConfiguration configuration = null!;
            ILogger<AutoresController> logger = null!;
            IServicioAutores servicioAutores = null!;

            //Creamos 2 autores nuevos en la tabla
            context.Autores.Add(new Autor {Nombre="Nicolas" });
            context.Autores.Add(new Autor {Nombre="Felipe" });

            await context.SaveChangesAsync();

            var context2 = ConstruirContext(nombreDB);

            var controller = new AutoresController(context2, mapper, configuration, almacenadorArchivos, logger, servicioAutores);

            //Prueba
            var respuesta = await controller.Get(1);

            //Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado.Id);
        }
    }
}
