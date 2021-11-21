variable "api_backend_url" {
  description = "The backend api for the gateway"
}

variable "platform_public_management_api_link"{
    description = "The platform public management api link"
}

# Api Gateway
variable "protocol" {
  default = "https"
}

variable "release_name" {
  default     = "1"
  description = "The release_name number to link which release_name the infratructure was created from."
}

variable "frontend_url_for_cors"{
  default = "localhost:5001"
}