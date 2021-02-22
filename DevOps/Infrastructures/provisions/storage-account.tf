resource "azurerm_storage_account" "hn-platform-storage" {
  name                     = "hnstorage${var.environment_prefix}"
  resource_group_name      = azurerm_resource_group.hn-platform-data-persistent.name
  location                 = azurerm_resource_group.hn-platform-data-persistent.location
  account_replication_type = "LRS"
  account_tier             = "Standard"
  account_kind             = "StorageV2"
  min_tls_version          = "TLS1_2"
  large_file_share_enabled = true
  allow_blob_public_access = true

  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags
}

resource "azurerm_key_vault_secret" "storage-account-da-connectionstring-blu" {
  name         = "Storage-Account-Da-Connection-String"
  value        = azurerm_storage_account.hn-platform-storage.primary_connection_string
  key_vault_id = azurerm_key_vault.hnkeyvault.id

  tags = local.common_tags
}