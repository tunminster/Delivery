data "azurerm_api_management" "hn-platform" {
  name                = "hn-apim-${var.environment_prefix}"
  resource_group_name = "hn-platform-data-persistent-${var.environment_prefix}"
}