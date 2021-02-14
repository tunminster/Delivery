resource "azurerm_public_ip" "hn-public-ip" {
  name                = "hnPlatformIp"
  resource_group_name = "MC_hn-platform-transitory-dev_hn-platform-aks_eastus2"
  location            = azurerm_resource_group.hn-platform-transitory.location
  allocation_method   = "Static"

  tags = local.common_tags
}