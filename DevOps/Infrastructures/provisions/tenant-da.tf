resource "azurerm_mssql_database" "da" {
  name           = "sql-database-da"
  server_id      = azurerm_mssql_server.hn-platform-sql-server.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  server_name                      = azurerm_mssql_server.hn-platform-sql-server.name
  edition                          = "Standard"
  requested_service_objective_name = "S0"


  extended_auditing_policy {
    storage_endpoint                        = azurerm_storage_account.hn-platform-storage.primary_blob_endpoint
    storage_account_access_key              = azurerm_storage_account.hn-platform-storage.primary_access_key
    storage_account_access_key_is_secondary = true
    retention_in_days                       = 6
  }

  resource "azurerm_sql_firewall_rule" "da" {
    name                = "AlllowAzureServices"
    resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
    server_name         = azurerm_sql_server.hn-platform-sql-server.name
    start_ip_address    = "0.0.0.0"
    end_ip_address      = "0.0.0.0"
 }

  resource "azurerm_sql_firewall_rule" "da" {
    name                = "FirewallRule1"
    resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
    server_name         = azurerm_sql_server.hn-platform-sql-server.name
    start_ip_address    = "81.109.71.197"
    end_ip_address      = "81.109.71.197"
 }

  tags = local.common_tags

}

resource "azurerm_key_vault_secret" "sql-database-da-connection-string" {
  name         = "Sql-Database-Da-Connection-String"
  value        = "Server=tcp:${azurerm_mssql_server.hn-platform-sql-server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.da.name};Persist Security Info=False;User ID=${var.sql_database_runtimeuser_name_da};Password=\"${var.tenant_da_password}\";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.pfpersistentblu.id

  tags = local.common_tags
}