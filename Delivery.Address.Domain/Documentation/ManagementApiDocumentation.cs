using Delivery.Domain.Documentation;

namespace Delivery.Address.Domain.Documentation
{
    public static class ManagementApiDocumentation
    {
        public static string GetApiGeneralDocumentationMarkdown(string environment, string category) => @$"
{GenericApiDocumentation.GetApiGeneralAboutDocumentationMarkdown(category)}

{GenericApiDocumentation.GetBoilerplateDocumentationMarkdown(environment, category)}
		";
    }
}