resource "azurerm_storage_account" "hn-platform-storage" {
  name                     = "hnstorage${var.environment_prefix}"
  resource_group_name      = azurerm_resource_group.hn-platform-data-persistent.name
  location                 = azurerm_resource_group.hn-platform-data-persistent.location
  account_replication_type = "LRS"
  account_tier             = "Standard"
  account_kind             = "StorageV2"
  min_tls_version          = "TLS1_2"
  large_file_share_enabled = true

  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags
}