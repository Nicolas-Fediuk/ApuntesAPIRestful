namespace WebApiAutores.DTOs
{
    //que pagine de 10 en 10
    //public record PaginacionDTO(int Pagina = 1, int RecodsPorPagina = 10)
    //{
    //    //para que muestre un maximo de 50 datos por pagina
    //    private const int CantidadMaximaRecodsPorPagina = 50;

    //    //Para que el usuario no coloque 0 o -1
    //    private int Pagina { get; init; } = Math.Max(1, Pagina);

    //    //para que el numero de paginacion sea mayor a 1 pero menor a CantidadMaximaRecodsPorPagina
    //    public int RecodsPorPagina { get; init; } = Math.Clamp(RecodsPorPagina, 1, CantidadMaximaRecodsPorPagina);


    //}

    public class PaginacionDTO
    {
        private const int CantidadMaximaRecodsPorPagina = 50;

        private int _recodsPorPagina = 10;
        public int Pagina { get; set; } = 1;

        public int RecodsPorPagina
        {
            get => _recodsPorPagina;
            set => _recodsPorPagina = Math.Clamp(value, 1, CantidadMaximaRecodsPorPagina);
        }
    }
}
