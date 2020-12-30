resource "azurerm_key_vault" "hnkeyvault" {
  name                = "hn-key-vault-${var.environment_prefix}"
  resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
  location            = azurerm_resource_group.hn-platform-data-persistent.location
  sku_name            = "standard"
  tenant_id           = var.tenant_id
  soft_delete_enabled = true
  soft_delete_retention_days  = 7
  tags = local.common_tags
}

resource "azurerm_key_vault_access_policy" "hn-contributor-access-policy" {
  key_vault_id = azurerm_key_vault.hnkeyvault.id
  tenant_id    = var.tenant_id
  object_id    = var.active_directory_contributor_object_id

  certificate_permissions = [
    "get",
    "list"
  ]

  key_permissions = [
    "get",
    "list"
  ]

  secret_permissions = [
    "get",
    "list"
  ]
}