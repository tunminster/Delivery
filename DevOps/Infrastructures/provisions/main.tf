
# Create a resource group
resource "azurerm_resource_group" "hn-platform-data-persistent" {
  name     = "hn-platform-data-persistent-${var.environment_prefix}"
  location = "East us 2"
}