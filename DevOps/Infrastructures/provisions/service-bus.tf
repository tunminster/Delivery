resource "azurerm_servicebus_namespace" "hn-platform-service-bus"{
    name                    = "hn-platform-service-bus-${var.environment_prefix}"
    resource_group_name     = azurerm_resource_group.hn-platform-data-persistent.name
    location                = azurerm_resource_group.hn-platform-data-persistent.location
    sku                     = "Standard"
    tags                    = var.environment_prefix 
}

resource "azurerm_servicebus_namespace_authorization_rule" "hn-platform-service-bus-auth-rule"{
    name                    = "hn-platform-service-bus-auth-rules-${var.environment_prefix}"
    resource_group_name     = azurerm_servicebus_namespace.hn-platform-service-bus.resource_group_name
    namespace_name          = azurerm_servicebus_namespace.hn-platform-service-bus.name

    listen = true
    send   = true
    manage = true
}

# Order topic
resource "azurerm_servicebus_topic" "platform-orders"{
    name                    = "orders"
    resource_group_name     = azurerm_resource_group.hn-platform-data-persistent.name
    namespace_name          = azurerm_servicebus_namespace.hn-platform-service-bus.name
}

resource "azurerm_servicebus_topic_authorization_rule" "platform-orders" {
  name                = "service-bus-topic-orders-rule-${var.environment_prefix}"

  resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
  namespace_name      = azurerm_servicebus_namespace.hn-platform-service-bus.name
  topic_name          = azurerm_servicebus_topic.platform-orders.name
  listen              = true
  send                = true
  manage              = true
}

resource "azurerm_key_vault_secret" "service-bus-topic-orders-connection-string" {
  name         = "ServiceBus-Topic-Orders-ConnectionString"
  value        = azurerm_servicebus_topic_authorization_rule.platform-orders.primary_connection_string
  #key_vault_id = azurerm_key_vault.pfpersistentblu.id

  tags = local.common_tags
}