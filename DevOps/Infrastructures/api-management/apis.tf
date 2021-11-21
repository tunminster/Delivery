resource "azurerm_api_management_api" "hn-platform-public-management-api" {
  name                = "hn-platform-public-management-api-dev"
  resource_group_name = data.azurerm_api_management.hn-platform.resource_group_name
  api_management_name = data.azurerm_api_management.hn-platform.name
  revision            = replace(var.release_name, ".", "")
  display_name        = "Ragibull - Management Apis"
  description         = "Contains all of the managemenet endpoints."
  path                = ""
  protocols           = [var.protocol]
  service_url         = "${var.api_backend_url}"

  subscription_required = false

  import {
    content_format = "openapi-link"
    content_value = "https://delivery-api.harveynetwork.com/swagger/api/management/v1/swagger.json"
    #content_value  = var.platform_public_management_api_link
  }
}