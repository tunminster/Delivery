resource "azurerm_api_management_api" "hn-platform-public-management-api" {
  name                = "hn-platform-public-management-api-${var.environment_name}"
  resource_group_name = data.azurerm_api_management.hn-platform.resource_group_name
  api_management_name = data.azurerm_api_management.hn-platform.name
  revision            = replace(var.release_name, ".", "")
  display_name        = "Ragibull - Management Apis"
  description         = "Contains all of the managemenet endpoints."
  path                = "api"
  protocols           = [var.protocol]
  service_url         = "${var.api_backend_url}/api"

  subscription_required = false

  import {
    content_format = "openapi-link"
    content_value  = var.platform_public_management_api_link
  }
}