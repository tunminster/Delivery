variable "state_storage_access_key" {
}

variable "state_storage_key_file" {
}

variable "state_storage_account_name" {
}

variable "state_storage_account_container_name" {
}

variable "environment_prefix" {
}

variable "tenant_id"{

}

variable "active_directory_contributor_object_id"{

}

locals {
    common_tags = {
        environment = var.environment_prefix
    }
}