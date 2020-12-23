
# Create a resource group
resource "azurerm_resource_group" "hn-platform-data-persistent" {
  name     = "hn-platform-data-persistent-dev"
  location = "West US 2"
}