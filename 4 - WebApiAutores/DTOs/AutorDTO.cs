namespace WebApiAutores.DTOs
{
    public class AutorDTO: Recurso
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public string Foto { get; set; }
    }
}
