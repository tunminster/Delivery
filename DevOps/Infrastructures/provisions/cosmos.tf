resource "azurerm_cosmosdb_account" "hn-platform-cosmos-db" {
  name                = "hn-platform-cosmos-db-${var.environment_prefix}"
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
    max_staleness_prefix    = 20
  }

  geo_location {
    location          = azurerm_resource_group.hn-platform-data-persistent.location
    failover_priority = 0
  }

}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "hn-platform"
  resource_group_name = azurerm_cosmosdb_account.hn-platform-cosmos-db.resource_group_name
  account_name        = azurerm_cosmosdb_account.hn-platform-cosmos-db.name
}

resource "azurerm_cosmosdb_sql_container" "connect-accounts" {
  name                = "connect-accounts"
  resource_group_name = azurerm_cosmosdb_account.hn-platform-cosmos-db.resource_group_name
  account_name        = azurerm_cosmosdb_account.hn-platform-cosmos-db.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path    = "/partitionKey"
  partition_key_version = 1
}

resource "azurerm_cosmosdb_sql_container" "country-taxes" {
  name                = "country-taxes"
  resource_group_name = azurerm_cosmosdb_account.hn-platform-cosmos-db.resource_group_name
  account_name        = azurerm_cosmosdb_account.hn-platform-cosmos-db.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path    = "/partitionKey"
  partition_key_version = 1
}

resource "azurerm_key_vault_secret" "hn-platform-cosmos-db" {
  name         = "CosmosDatabase-Hn-Platform-ConnectionString"
  value        = "AccountEndpoint=https://${azurerm_cosmosdb_account.hn-platform-cosmos-db.name}.documents.azure.com:443/;AccountKey=${azurerm_cosmosdb_account.hn-platform-cosmos-db.primary_master_key};"
  key_vault_id = azurerm_key_vault.hnkeyvault.id

  tags = local.common_tags
}