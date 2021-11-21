data "azurerm_api_management" "hn-platform" {
  name                = "hn-apim-${var.environment}"
  resource_group_name = "hn-platform-data-persistent-${var.environment}"
}