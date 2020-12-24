# Configure the Azure Provider
provider "azurerm" {
  # whilst the `version` attribute is optional, we recommend pinning to a given version of the Provider
  version = "=2.40.0"
  features {}
}

terraform{
  backend "azurerm" {
    // storage_account_name = "${var.state_storage_account_name}"
    // container_name       = "${var.state_storage_account_container_name}"
    // key                  = "${var.state_storage_key_file}"

    // # rather than defining this inline, the Access Key can also be sourced
    // # from an Environment Variable - more information is available below.
    // access_key = "${var.state_storage_access_key}"
  }
}