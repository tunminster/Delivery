data "azurerm_api_management" "hn-platform" {
  name                = "hn-apim-dev"
  resource_group_name = "hn-platform-data-persistent-dev"
}