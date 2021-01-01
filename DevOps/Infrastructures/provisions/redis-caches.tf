resource "azurerm_redis_cache" "hn-redis-cache" {
  name                = "hn-platform-redis-cache-${var.environment_prefix}"
  resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
  location            = azurerm_resource_group.hn-platform-data-persistent.location
  capacity            = 0
  family              = "C"
  sku_name            = "Basic"
  enable_non_ssl_port = false
}

// resource "azurerm_key_vault_secret" "redis-cache-connection-string" {
//   name         = "RedisCache-ConnectionString"
//   value        = "${azurerm_redis_cache.hn-redis-cache}:${azurerm_redis_cache.hn-redis-cache.ssl_port},password=${azurerm_redis_cache.hn-redis-cache.primary_access_key},ssl=True,abortConnect=False"
//   key_vault_id = azurerm_key_vault.hnkeyvault.id

//   tags = local.common_tags

//   lifecycle {
//     ignore_changes = [
//       tags
//     ]
//   }
// }