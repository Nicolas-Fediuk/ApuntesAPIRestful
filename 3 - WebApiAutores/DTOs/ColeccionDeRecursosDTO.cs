namespace WebApiAutores.DTOs
{
    public class ColeccionDeRecursosDTO<T> : Recurso where T: Recurso
    {
        public IEnumerable<T> Valores { get; set; } 
    }
}
