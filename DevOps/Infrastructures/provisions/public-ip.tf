resource "azurerm_public_ip" "hn-public-ip" {
  name                = "hnPlatformIp"
  resource_group_name = azurerm_resource_group.hn-platform-transitory.name
  location            = azurerm_resource_group.hn-platform-transitory.location
  allocation_method   = "Static"

  tags = local.common_tags
}