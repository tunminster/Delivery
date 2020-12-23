# Configure the Azure Provider
provider "azurerm" {
  # whilst the `version` attribute is optional, we recommend pinning to a given version of the Provider
  version = "=2.40.0"
  features {}
}

# Create a resource group
resource "azurerm_resource_group" "hn-platform-rg" {
  name     = "hn-platform-cosmos-rg-dev"
  location = "West US 2"
}