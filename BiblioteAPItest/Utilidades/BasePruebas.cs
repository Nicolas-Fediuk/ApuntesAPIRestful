using WebApiAutores;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using WebApiAutores.Utilidades;

namespace BiblioteAPItest.Utilidades
{
    //clase auxiliar para trabsjar con EF core
    public class BasePruebas
    {
        //Metodo para crear el DbContext
        protected ApplicationDbContex ConstruirContext(string nombreBD)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContex>()
                .UseInMemoryDatabase(nombreBD).Options;

            var dbContext = new ApplicationDbContex(opciones);

            return dbContext;
        }

        //para usar Automapper
        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(opciones =>
            {
                opciones.AddProfile(new AutoMapperProfile());
            });

            return config.CreateMapper();
        }
    }
}
