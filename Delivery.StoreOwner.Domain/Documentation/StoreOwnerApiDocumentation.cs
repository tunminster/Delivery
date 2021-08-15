using Delivery.Domain.Documentation;

namespace Delivery.StoreOwner.Domain.Documentation
{
    public static class StoreOwnerApiDocumentation
    {
        public static string GetApiGeneralDocumentationMarkdown(string environment, string category) => @$"
{GenericApiDocumentation.GetApiGeneralAboutDocumentationMarkdown(category)}

{GenericApiDocumentation.GetBoilerplateDocumentationMarkdown(environment, category)}
		";
    }
}