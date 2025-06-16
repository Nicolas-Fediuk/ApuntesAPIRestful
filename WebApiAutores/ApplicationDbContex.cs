using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores
{
    public class ApplicationDbContex : DbContext
    {
        public ApplicationDbContex(DbContextOptions options) : base(options)
        {

        }

        //se van a crear campos con los atributos de Autores
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
    }
}
