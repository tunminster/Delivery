terraform {
      required_providers {
        azurerm = {
          source  = "hashicorp/azurerm"
          version = "=2.48.0"
        }
      }
    }
    
# Configure the Azure Provider
provider "azurerm" {
  # whilst the `version` attribute is optional, we recommend pinning to a given version of the Provider
  #version = "=2.40.0"
  features {}
}

terraform{
  backend "azurerm" {
    storage_account_name = "pngosa"
    container_name       = "platform-tf-state-container"
    key                  = "platform-tf-state.tfstate"

  }
}