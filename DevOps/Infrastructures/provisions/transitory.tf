resource "azurerm_resource_group" "hn-platform-transitory" {
  name     = "hn-platform-transitory-${var.environment_prefix}"
  location = "East us 2"
}