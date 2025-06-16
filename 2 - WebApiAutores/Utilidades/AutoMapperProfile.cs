using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Migrations;

namespace WebApiAutores.Utilidades
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //mapea de AutorCreacionDTO a Autor
            CreateMap<AutorCreacionDTO, Autor>();

            //mapea de Autor a AutorDTO
            CreateMap<Autor, AutorDTO>();


            CreateMap<Autor, AutorDTOConLibros>()
                .ForMember(autorDTO => autorDTO.Libros, opciones => opciones.MapFrom(MapAutorDTOLibros));
            
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(libros => libros.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros));

            CreateMap<Libro, LibroDTO>();

            CreateMap<Libro, LibroDTOConAutores>()
                .ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(MapLibroDtoAutores));

            CreateMap<LibroCreacionDTO, AutorLibro>()
                .ForMember(ent => ent.Libro, config => config.MapFrom(dto => new Libro { Titulo = dto.Titulo }));

            /*CreateMap<Libro, LibroDTOConAutores>()
                .ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(libro => MapearIdNombreAutor(libro)));*/

            //para que se mapee de LibroPachDTO a LibroDTO y viceversa
            CreateMap<LibroPachDTO, Libro>().ReverseMap();

            CreateMap<ComentarioCreacionDTO, Comentario>();

            CreateMap<Comentario, ComentarioDto>();
        }

        private List<LibroDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();

            if (autor.AutoresLibros == null) { return resultado; }

            foreach(var autorLibro in autor.AutoresLibros)
            {
                resultado.Add(new LibroDTO()
                {
                    Id = autorLibro.LibroId,
                    Titulo = autorLibro.Libro.Titulo
                });
            }
            return resultado;   
        }

        private List<AutorDTO> MapLibroDtoAutores(Libro libro, LibroDTO libroDTO)
        {
            var resultado = new List<AutorDTO>();

            if (libro.AutoresLibros == null) {return resultado;} 

            foreach(var autorLibro in libro.AutoresLibros)
            {
                resultado.Add(new AutorDTO()
                {
                    Id = autorLibro.AutorId,
                    Nombre = autorLibro.Autor.Nombre
                });
            }

            return resultado;
        }

        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            var resultado = new List<AutorLibro>();

            if(libroCreacionDTO.AutoresIds == null) { return resultado; }

            foreach(var autorId in libroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro() { AutorId = autorId });
            }

            return resultado;
        }

        //private string MapearIdNombreAutor(Libro libroDTO) => $"{libroDTO.Id},{libroDTO.Titulo}";
    }
}
