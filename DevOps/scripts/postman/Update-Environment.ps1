param
(
    [string] $postmanApiKeys = $(throw "The postman api key is required"),
    [string] $workspaceId = $(throw "Workspace id is required"),
    [string] $environment = $(throw "Environment is required"),
    [string] $domain = $(throw "Domain is required e.g. Trade, Industry"),
    [string] $authorizationTokenEndpoint= $(throw "The token endpoint is required"),
    [string] $authorizationScopes= $(throw "The authorization scopes are required"),
    [string] $authorizationClientId,
    [string] $authorizationClientSecret,
    [string] $maxRetryCount,
    [string] $additionalEnvironmentSettingsJson
)

## Add api keys to a list
$apiKeys = $postmanApiKeys.Split(",")

##  Get random API key
# $apiKey = $apiKeys | Get-Random

# $defaultHeaders = @{
#     'Accept' = 'application/json'
#     'Content-Type' = 'application/json'
#     'X-Api-Key' = $apiKey
# }

# Write-Host "Converting to additional parameters list: $additionalEnvironmentSettingsJson"
# $additionalEnvironmentSettingsTable = $additionalEnvironmentSettingsJson | ConvertFrom-Json -AsHashtable

# ## Get existing environments
# $environmentsUrl = "https://api.getpostman.com/environments"
# Write-Host "Getting existing environments from $environmentsUrl"

# $targetEnvironmentName = "$environment - $domain"
# if($authorizationClientSecret){
#     $targetEnvironmentName = "$targetEnvironmentName (internal use only)"
#     ## Unable in postman to lockdown this environment so special casing this and not publishing key
#     if($targetEnvironmentName -eq "Prd - Management (internal use only)"){ 
#         $authorizationClientSecret = 'YOUR-CLIENT-SECRET'
#     }
# } else {
#     $authorizationClientId = 'YOUR-CLIENT-ID'
#     $authorizationClientSecret = 'YOUR-CLIENT-SECRET'
# }

# "Calling $environmentsUrl with api key: $apiKey"
# $response = Invoke-RestMethod $environmentsUrl -Method 'GET' -Headers $defaultHeaders | ConvertTo-Json | ConvertFrom-Json
# $environments = $response.environments | Where-Object { $_.name -eq $targetEnvironmentName }

# if($environments.Length -gt 1) {
#     throw "Expected one or less environments to match '$targetEnvironmentName'. Found: $environments"
# }

# ## Create environment values
# $environmentValues = @{
#     'authorizationClientId' = $authorizationClientId
#     'authorizationClientSecret' = $authorizationClientSecret
#     'authorizationTokenEndpoint' = $authorizationTokenEndpoint
#     'authorizationScopes' = $authorizationScopes
#     'maxRetryCount' = $maxRetryCount
# }

# $environmentItems = New-Object Collections.Generic.List[string]

# $environmentValues.Keys | ForEach-Object{
#     $value = $environmentValues[$_]
#     $item = "{""key"": ""$_"", ""value"": ""$value""}"
#     $environmentItems.Add($item)
# }

# $additionalEnvironmentSettingsTable.Keys | ForEach-Object{
#     $value = $additionalEnvironmentSettingsTable[$_]
#     $item = "{""key"": ""$_"", ""value"": ""$value""}"
#     $environmentItems.Add($item)
# }

# $environmentItemsFormatted = [String]::Join(",", $environmentItems.ToArray())

# $environmentPayload = @"
# {
#     "environment": {
#         "name": "$targetEnvironmentName",
#         "values": [$environmentItemsFormatted]
#     }
# }
# "@

# Write-Host $environmentPayload
# Write-Host "WorkspaceId" + $workspaceId

# ## Create or update environment
# $httpMethod = 'POST'
# if($environments.Length -eq 1) {
#     $httpMethod = 'PUT'
#     $environmentUid = $environments[0].uid
#     $environmentsUrl = "$environmentsUrl/$environmentUid"
# }

# $environmentsUrl = $environmentsUrl + "?workspace=$workspaceId"
# Write-Host "Modifying environment with url $environmentsUrl"

# ##  Get random API key
# $apiKey = $apiKeys | Get-Random
# $defaultHeaders['X-Api-Key'] = $apiKey

# "Calling $environmentsUrl with api key: $apiKey"
# $response = Invoke-RestMethod $environmentsUrl -Method $httpMethod -Headers $defaultHeaders -Body $environmentPayload

# Write-Host "Updated environment with url $environmentsUrl and workspace $workspaceId"
# Write-Host "-------------------------------------------------------------------------"
# Write-Host $response.environment