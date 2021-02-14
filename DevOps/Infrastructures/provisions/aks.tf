resource "azurerm_kubernetes_cluster" "hn-platform-aks" {
  name                = "hn-platform-aks"
  location            = azurerm_resource_group.hn-platform-transitory.location
  resource_group_name = azurerm_resource_group.hn-platform-transitory.name
  dns_prefix          = "hnplatformaks"

  default_node_pool {
    name                  = "default"
    type                  = "virtualMachineScaleSets"
    enable_auto_scaling   = true
    min_count             = 3
    max_count             = 3
    vm_size               = var.aks_node_vm_size
    os_disk_size_gb       = var.aks_node_os_disk_size
  }

  service_principal {
    client_id     = var.aks_sp_app_id
    client_secret = var.aks_sp_app_secret
  }

  role_based_access_control {
    enabled = true
  }

  network_profile {
        network_plugin = "azure"
        dns_service_ip = "10.0.0.10"
        docker_bridge_cidr = "172.17.0.1/16"
        service_cidr = "10.0.0.0/16"
    }

  tags = local.common_tags
}