resource "azurerm_mssql_server" "hn-platform-sql-server" {
  name                         = "hn-platform-sql-server-${var.environment_prefix}"
  resource_group_name          = azurerm_resource_group.hn-platform-data-persistent.name
  location                     = azurerm_resource_group.hn-platform-data-persistent.location
  version                      = "12.0"
  administrator_login          = var.sql_server_admin_name
  administrator_login_password = var.sql_server_admin_password
  minimum_tls_version          = "1.2"

  azuread_administrator {
    login_username = "AzureAD Admin"
    object_id      = "0321b94f-e554-45ce-94ae-6d82c3368c37"
  }

  azurerm_mssql_server_extended_auditing_policy {
    storage_endpoint                        = azurerm_storage_account.hn-platform-storage.primary_blob_endpoint
    storage_account_access_key              = azurerm_storage_account.hn-platform-storage.primary_access_key
    storage_account_access_key_is_secondary = true
    retention_in_days                       = 6
  }

  tags = local.common_tags
}