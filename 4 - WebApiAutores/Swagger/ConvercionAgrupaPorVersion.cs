using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebApiAutores.Swagger
{
    public class ConvercionAgrupaPorVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            // Ejemplo: "Controllers.V1"
            var namespaceDelControlador = controller.ControllerType.Namespace;
            var version = namespaceDelControlador.Split(".").Last().ToLower();
            controller.ApiExplorer.GroupName = version;
        }
    }
}
