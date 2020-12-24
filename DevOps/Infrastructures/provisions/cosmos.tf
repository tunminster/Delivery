resource "azurerm_cosmosdb_account" "hn-platform-cosmos-db" {
  name                = "platform-cosmos-db-${var.environment_prefix}"
  location            = azurerm_resource_group.hn-platform-data-persistent.location
  resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
  offer_type          = "Standard"
  kind                    = "GlobalDocumentDB"

  enable_automatic_failover = false
  
  capabilities {
    name = "EnableServerless"
  }

  consistency_policy {
    consistency_level       = "BoundedStaleness"
    max_interval_in_seconds = 10
    max_staleness_prefix    = 200
  }

  geo_location {
    location          = azurerm_resource_group.hn-platform-data-persistent.location
    failover_priority = 0
  }

}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "platform-cosmossql-${var.environment_prefix}"
  resource_group_name = azurerm_cosmosdb_account.hn-platform-cosmos-db.resource_group_name
  account_name        = azurerm_cosmosdb_account.hn-platform-cosmos-db.name
}