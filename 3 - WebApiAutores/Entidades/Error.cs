namespace WebApiAutores.Entidades
{
    public class Error
    {
        public Guid Id { get; set; }
        public string MensajeError { get; set; }
        //codigo que llevo al error
        public string StrackTrace { get; set; }
        public DateTime Fecha { get; set; }
    }
}
