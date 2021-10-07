resource "azurerm_api_management" "hnapim"{
    name                = "hn-apim-${var.environment_prefix}"
    resource_group_name = azurerm_resource_group.hn-platform-data-persistent.name
    location            = azurerm_resource_group.hn-platform-data-persistent.location
    publisher_name      = var.gateway_publisher_name
    publisher_email     = var.gateway_publisher_email

    sku_name = var.gateway_sku_name

    identity {
    type = "SystemAssigned"
  }

  protocols {
    enable_http2 = true
  }

  tags = local.common_tags

  lifecycle {
    prevent_destroy = true
    ignore_changes = [
      tags
    ]
  }
}