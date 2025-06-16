using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores
{
    public class ApplicationDbContex : IdentityDbContext
    {
        public ApplicationDbContex(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //siempre tiene que estar
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AutorLibro>().HasKey(x => new { x.AutorId, x.LibroId });

            //filtro global para que no traiga los comentarios borrados logicamente
            modelBuilder.Entity<Comentario>().HasQueryFilter(b => !b.EstaBorrado);
        }

        //se van a crear campos con los atributos de Autores
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }

        public DbSet<AutorLibro> AutorLibros { get; set; }  
        public DbSet<Error> Errores { get; set; }
    }
}
