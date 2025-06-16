namespace WebApiAutores.DTOs
{
    public class AutorFiltroDTO
    {
        public int Pagina { get; set; } = 1;
        public int RecordsPorPagina { get; set; } = 10;
        public PaginacionDTO PaginacionDTO
        {
            get
            {
                return new PaginacionDTO { Pagina = Pagina, RecodsPorPagina = RecordsPorPagina };
            }
        }
        public string? Nombre {get; set;} 
        public bool? TieneFoto { get; set; }
        public bool? TieneLibros { get; set; }
        public string? TituloLibro { get; set; }
        public bool IncluirLibros { get; set; }
        public string? CampoOrdenar { get; set; }
        public bool OrdernarAscendente { get; set; } = true;

    }
}
