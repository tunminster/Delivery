resource "azurerm_notification_hub_namespace" "hn-notification-hub-namespace" {
  name                = "hn-platform-notification-hub-namespace-${var.environment_prefix}"
  resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
  location            = azurerm_resource_group.hn-platform-data-persistent.location
  namespace_type      = "NotificationHub"

  sku_name = "Basic"
}

resource "azurerm_notification_hub" "hn-notification-hub" {
  name                = "hn-platform-notification-hub-${var.environment_prefix}"
  namespace_name      = azurerm_notification_hub_namespace.hn-notification-hub-namespace.name
  resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
  location            = azurerm_resource_group.hn-platform-data-persistent.location
}