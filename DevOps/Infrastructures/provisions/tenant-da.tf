resource "azurerm_mssql_database" "da" {
  name           = "sql-database-da"
  server_id      = azurerm_mssql_server.hn-platform-sql-server.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  sku_name       = "S0"

  tags = local.common_tags

}

resource "azurerm_key_vault_secret" "sql-database-da-connection-string" {
  name         = "Sql-Database-Da-Connection-String"
  value        = "Server=tcp:${azurerm_mssql_server.hn-platform-sql-server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.da.name};Persist Security Info=False;User ID=${var.sql_database_runtimeuser_name_da};Password=\"${var.tenant_da_password}\";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.hnkeyvault.id

  tags = local.common_tags
}