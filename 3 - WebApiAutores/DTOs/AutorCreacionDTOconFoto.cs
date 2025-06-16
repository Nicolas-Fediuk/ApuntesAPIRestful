namespace WebApiAutores.DTOs
{
    public class AutorCreacionDTOconFoto : AutorCreacionDTO
    {
        //representa archivos
        public IFormFile Foto { get; set; }
    }
}
