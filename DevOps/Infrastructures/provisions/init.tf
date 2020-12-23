terraform {
  backend "azurerm" {
    version = "=2.40.0"
    storage_account_name = "${var.state_storage_account_name}"
    container_name       = "${var.state_storage_account_container_name}"
    key                  = "${var.state_storage_key_file}"
    access_key = "${var.state_storage_access_key}"
  }
}