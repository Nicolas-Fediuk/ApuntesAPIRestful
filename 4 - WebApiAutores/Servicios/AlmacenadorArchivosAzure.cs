using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Management.Storage.Models;

namespace WebApiAutores.Servicios
{
    public class AlmacenadorArchivosAzure : IAlmacenadorArchivos
    {
        private readonly string connectionString;

        public AlmacenadorArchivosAzure(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("AzureStorageConnection");
        }

        public async Task<string> Almacenar(string contenedor, IFormFile archivo)
        {
            //cliente para conectarme al Azure
            //contenedor: carpeta donde se guarda las imagenes
            var cliente = new BlobContainerClient(connectionString, contenedor);
            // para crear el contenedor
            await cliente.CreateIfNotExistsAsync();
            //politica para acceder a los archivos
            cliente.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            //para crear el archivo
            var blob = cliente.GetBlobClient(nombreArchivo);

            //configuraciones
            var blobHttpHeaders = new BlobHttpHeaders();
            blobHttpHeaders.ContentType = archivo.ContentType;

            await blob.UploadAsync(archivo.OpenReadStream(), blobHttpHeaders);
            return blob.Uri.ToString(); 
        }

        public async Task Borrar(string ruta, string contenedor)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return;
            }

            var cliente = new BlobContainerClient(connectionString, contenedor);
            await cliente.CreateIfNotExistsAsync();
            var nombreArchivo = Path.GetFileName(ruta);
            var blob = cliente.GetBlobClient(nombreArchivo);
            await blob.DeleteIfExistsAsync();
        }
    }
}
