using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Delivery.Azure.Library.WebApi.Swagger.Filters
{
    public class ControllerDocumentationConvention : IControllerModelConvention
    {
        void IControllerModelConvention.Apply(ControllerModel controller)
        {
            foreach (var attribute in controller.Attributes)
            {
                if (attribute.GetType() != typeof(RouteAttribute))
                {
                    continue;
                }

                var routeAttribute = (RouteAttribute)attribute;
                if (!string.IsNullOrWhiteSpace(routeAttribute.Name))
                {
                    controller.ControllerName = routeAttribute.Name;
                }
            }
        }
    }
}