using System.Net;

namespace Delivery.Domain.Documentation
{
    public static class GenericApiDocumentation
    {
        public static string GetBoilerplateDocumentationMarkdown(string environment, string category) => $@"
{GetEnvironmentDocumentationMarkdown(environment, category)}

{GetApiAuthenticationDocumentationMarkdown}

{GetApiStatusCodesDocumentationMarkdown()}

{GetApiGeneralUsageDocumentationMarkdown}

{GetApiHelpDocumentationMarkdown}
		";
        
        public const string GetApiAuthenticationDocumentationMarkdown = @"
## Authentication
The apis use the client credentials flow for server-to-server communication. 
		";
        
        public static string GetApiGeneralAboutDocumentationMarkdown(string category) => @$"
The {category.ToLowerInvariant()} apis allow a distribution partner to create and manage their own food and delivery services.
		";
        public const string GetApiGeneralUsageDocumentationMarkdown = @"
## Updating data
The apis follow a desired-state principle to modify data. but note that there may be a delay due to the distributed and eventually-consistent nature of the platform.
		";
        
        public const string GetApiHelpDocumentationMarkdown = @"
## Support
Having issues with our apis? [Contact support](support@ragibull.com).
		";
        public static string GetEnvironmentDocumentationMarkdown(string environment, string category) => $@"
## Quick start
Select the environment `{environment} - {category}` above and click `Run in postman` to get started quickly.
		";
        
        public static string GetApiStatusCodesDocumentationMarkdown() => @$"
## Status codes
- `{(int) HttpStatusCode.OK} {HttpStatusCode.OK}`
    - Indicates a successful response
- `{(int) HttpStatusCode.Accepted} {HttpStatusCode.Accepted}`
    - Indicates an asynchronous operation has been started
- `{(int) HttpStatusCode.Unauthorized} {HttpStatusCode.Unauthorized}`
    - The credentials supplied do not have access to this resource
- `{(int) HttpStatusCode.BadRequest} {HttpStatusCode.BadRequest}`
    - The input supplied is not valid. All bad requests follow the same BadRequestContract structure documented in the open api document
		";
    }
}