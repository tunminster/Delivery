resource "azurerm_cosmosdb_account" "hn-platform-cosmos-db" {
  name                = "hn-platform-cosmos-db"
  location            = resource.azurerm_resource_group.hn-platform-rg.location
  resource_group_name = resource.azurerm_resource_group.hn-platform-rg.name
  offer_type          = "Standard"
  kind                    = "GlobalDocumentDB"

  enable_automatic_failover = false
  capabilities        = [{
    name = "EnableServerless"
  }]
}