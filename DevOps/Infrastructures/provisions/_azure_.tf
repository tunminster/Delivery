# Configure the Azure Provider
provider "azurerm" {
  # whilst the `version` attribute is optional, we recommend pinning to a given version of the Provider
  version = "=2.40.0"
  features {}
}

terraform{
  backend "azurerm" {
    storage_account_name = "pngosa"
    container_name       = "platform-tf-state-container"
    key                  = "platform-tf-state.tfstate"

    // # rather than defining this inline, the Access Key can also be sourced
    // # from an Environment Variable - more information is available below.
    // access_key = "${var.state_storage_access_key}"
  }
}