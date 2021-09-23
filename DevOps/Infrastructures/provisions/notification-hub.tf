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

resource "azurerm_notification_hub" "hn-notification-driver-hub" {
  name                = "hn-platform-notification-driver-hub-${var.environment_prefix}"
  namespace_name      = azurerm_notification_hub_namespace.hn-notification-hub-namespace.name
  resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
  location            = azurerm_resource_group.hn-platform-data-persistent.location
  lifecycle {
    ignore_changes = [
      parameters,
      tags,
      gcm_credential
    ]
  }
}

resource "azurerm_notification_hub" "hn-notification-shop-hub" {
  name                = "hn-platform-notification-shop-hub-${var.environment_prefix}"
  namespace_name      = azurerm_notification_hub_namespace.hn-notification-hub-namespace.name
  resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
  location            = azurerm_resource_group.hn-platform-data-persistent.location
}