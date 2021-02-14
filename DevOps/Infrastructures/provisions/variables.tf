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

variable "sql_server_admin_name"{

}

variable "sql_server_admin_password"{
    
}

variable "aks_node_vm_size"{

}

variable "aks_node_os_disk_size"{

}

variable "aks_sp_app_id" {

}

variable "aks_sp_app_secret"{
    
}

locals {
    common_tags = {
        environment = var.environment_prefix
    }
}