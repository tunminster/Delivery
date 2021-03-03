resource "azurerm_mssql_database" "raus" {
  name           = "sql-database-raus"
  server_id      = azurerm_mssql_server.hn-platform-sql-server.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  sku_name       = "S0"


  extended_auditing_policy {
    storage_endpoint                        = azurerm_storage_account.hn-platform-storage.primary_blob_endpoint
    storage_account_access_key              = azurerm_storage_account.hn-platform-storage.primary_access_key
    storage_account_access_key_is_secondary = true
    retention_in_days                       = 6
  }


  tags = local.common_tags

}

resource "azurerm_key_vault_secret" "sql-database-raus-connection-string" {
  name         = "Sql-Database-Raus-Connection-String"
  value        = "Server=tcp:${azurerm_mssql_server.hn-platform-sql-server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.raus.name};Persist Security Info=False;User ID=${var.sql_database_runtimeuser_name_raus};Password=\"${var.tenant_raus_password}\";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.hnkeyvault.id

  tags = local.common_tags
}