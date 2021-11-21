resource "azurerm_api_management_api_policy" "hn-platform-public-management-api-policy" {
  resource_group_name = data.azurerm_api_management.hn-platform.resource_group_name
  api_management_name = data.azurerm_api_management.hn-platform.name
  api_name            = azurerm_api_management_api.hn-platform-public-management-api.name
  xml_content = templatefile("/DevOps/Infrastructures/api-management/policies/global-policies.xml",
    {
      frontend_url           = var.frontend_url_for_cors
  })

}