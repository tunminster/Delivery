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
    [string] $additionalEnvironmentSettingsJson,
    [string] $baseUrl
)

function InvokeWithRetry {
    param
    (
        [string] $url = $(throw "The url is required"),
        [string] $method = $(throw "method is required"),
        [string] $body = $(throw "body is required")
    )

    ## Add api keys to a list
    $apiKeys = $postmanApiKeys.Split(",")

    ##  Get random API key
    $apiKey = $apiKeys | Get-Random
    
    $defaultHeaders = @{
        'Accept' = 'application/json'
        'Content-Type' = 'application/json'
    }

    $defaultHeaders.add('X-Api-Key', $apiKey)
    
    $retryCount = 0
    $completed = $false
    $response = $null
    while (-not $completed) {
        try {
            if($method -eq 'GET' -or $method -eq 'DELETE'){
                Write-Host "Calling $method Invoke-RestMethod $url -Method $method -Headers $defaultHeaders"
                $response = Invoke-RestMethod $url -Method $method -Headers $defaultHeaders
            }
            else{
                Write-Host "Calling $method Invoke-RestMethod $url with method $method, headers $defaultHeaders and body $body"
                $response = Invoke-RestMethod $url -Method $method -Headers $defaultHeaders -Body $body
            }
            $completed = $true
        } catch {
            $errorDescription = $_
            Write-Host "Error: $errorDescription"
            if ($retrycount -ge 5) {
                Write-Host "Request to $url failed the maximum number of $retryCount times. Continuing On Error: $errorDescription" 
                Write-Host "Continuing On Error"   
                $completed = $true
            } else {
                Write-Warning "Request to $url failed. Retrying in 2 seconds."
                Start-Sleep 2
                $retrycount++
            }
        }
    }
    return $response
}

# ## Add api keys to a list
# $apiKeys = $postmanApiKeys.Split(",")

# ##  Get random API key
# $apiKey = $apiKeys | Get-Random

# $defaultHeaders = @{
#     'Accept' = 'application/json'
#     'Content-Type' = 'application/json'
#     'X-Api-Key' = $apiKey
# }

Write-Host "Converting to additional parameters list: $additionalEnvironmentSettingsJson"
$additionalEnvironmentSettingsTable = $additionalEnvironmentSettingsJson | ConvertFrom-Json -AsHashtable

## Get existing environments
$environmentsUrl = "https://api.getpostman.com/environments"
Write-Host "Getting existing environments from $environmentsUrl"

$targetEnvironmentName = "$environment - $domain"
if($authorizationClientSecret){
    $targetEnvironmentName = "$targetEnvironmentName (internal use only)"
    ## Unable in postman to lockdown this environment so special casing this and not publishing key
    if($targetEnvironmentName -eq "Prd - Management (internal use only)"){ 
        $authorizationClientSecret = 'YOUR-CLIENT-SECRET'
    }
} else {
    $authorizationClientId = 'YOUR-CLIENT-ID'
    $authorizationClientSecret = 'YOUR-CLIENT-SECRET'
}

$url = $environmentsUrl + '?workspace=' + $workspaceId
$response = InvokeWithRetry $url 'GET' $null | ConvertTo-Json | ConvertFrom-Json
$environments = $response.environments | Where-Object { $_.name -eq $targetEnvironmentName }

if($environments.Length -gt 1) {
    throw "Expected one or less environments to match '$targetEnvironmentName'. Found: $environments"
}


## Create environment values
$environmentValues = @{
    'authorizationClientId' = $authorizationClientId
    'authorizationClientSecret' = $authorizationClientSecret
    'authorizationTokenEndpoint' = $authorizationTokenEndpoint
    'authorizationScopes' = $authorizationScopes
    'maxRetryCount' = $maxRetryCount
    'baseUrl' = $baseUrl
}

$environmentItems = New-Object Collections.Generic.List[string]

$environmentValues.Keys | ForEach-Object{
    $value = $environmentValues[$_]
    $item = "{""key"": ""$_"", ""value"": ""$value""}"
    $environmentItems.Add($item)
}

$additionalEnvironmentSettingsTable.Keys | ForEach-Object{
    $value = $additionalEnvironmentSettingsTable[$_]
    $item = "{""key"": ""$_"", ""value"": ""$value""}"
    $environmentItems.Add($item)
}

$environmentItemsFormatted = [String]::Join(",", $environmentItems.ToArray())

$environmentPayload = @"
{
    "environment": {
        "name": "$targetEnvironmentName",
        "values": [$environmentItemsFormatted]
    }
}
"@

Write-Host $environmentPayload

## Create or update environment
$environmentCount = $environments.Length
if($environmentCount -eq 1) {
    
    #Check env actually exists (postman flakiness)
    $httpMethod = 'GET'
    $environmentId = $environments[0].id
    $environmentsGetUrl = "$environmentsUrl/$environmentId"
    $getResponse = InvokeWithRetry $environmentsGetUrl $httpMethod $null
    Write-Host "reesp" + $getResponse.environment
    if($null-ne $getResponse.environment){
        $httpMethod = 'DELETE'
        $response = InvokeWithRetry $environmentsGetUrl $httpMethod $null
    }
}

$httpMethod = 'POST'
$url = $environmentsUrl + '?workspace=' + $workspaceId
Write-Host "Creating environment with url $url"

$response = InvokeWithRetry $url $httpMethod $environmentPayload

Write-Host "Updated environment with url $url and workspace $workspaceId"
Write-Host "-------------------------------------------------------------------------"
Write-Host $response.environment