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