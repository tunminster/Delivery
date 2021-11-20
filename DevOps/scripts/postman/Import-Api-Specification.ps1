param
(
    [string] $postmanApiKeys = $(throw "The postman api key is required"),
    [string] $workspaceId = $(throw "Workspace id is required"),
    [string] $environment = $(throw "Environment is required"),
    [string] $openApiDownloadLink = $(throw "The link to download the open api document is required"),
    [string] $domain = $(throw "Domain is required")
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
            if($method -eq 'GET'){
                Write-Host "Calling $url with method $method and headers $defaultHeaders"
                $response = Invoke-RestMethod $url -Method $method -Headers $defaultHeaders | ConvertTo-Json -Depth 100
            }
            else{
                Write-Host "Calling $url with method $method, headers $defaultHeaders and body $body"
                $response = Invoke-RestMethod $url -Method $method -Headers $defaultHeaders -Body $body | ConvertTo-Json -Depth 100
            }
            $completed = $true
        } catch {
            $errorDescription = $_.Exception.Response.StatusDescription
            Write-Host "Error: $errorDescription"
            if ($retrycount -ge 5) {
                throw "Request to $url failed the maximum number of $retryCount times."
            } else {
                Write-Warning "Request to $url failed. Retrying in 2 seconds."
                Start-Sleep 2
                $retrycount++
            }
        }
    }
    return $response
}

## Add api keys to a list
$apiKeys = $postmanApiKeys.Split(",")

##  Get random API key
$apiKey = $apiKeys | Get-Random

$defaultHeaders = @{
    'Accept' = 'application/json'
    'Content-Type' = 'application/json'
}

## Download api specification and update document title
Write-Host "Downloading open api document from $openApiDownloadLink"
$openApiSpecificationOriginalText = Invoke-RestMethod $openApiDownloadLink -Method 'GET' -Headers $defaultHeaders | ConvertTo-Json -Depth 100
$openApiSpecification = $openApiSpecificationOriginalText | ConvertFrom-Json
$documentTitle = $openApiSpecification.info.title + " (" + $environment + ")"
Write-Host "Downloded open api document from $openApiDownloadLink"

$openApiSpecificationText = $openApiSpecificationOriginalText -replace $openApiSpecification.info.title, $documentTitle
$openApiSpecification = $openApiSpecificationText | ConvertFrom-Json
$documentTitle = $openApiSpecification.info.title
Write-Host "Using open api document title '$documentTitle'"

## Check if api already exists
$defaultHeaders.add('X-Api-Key', $apiKey)

$postmanBaseUrl = "https://api.getpostman.com"
$apisBaseUrl = "$postmanBaseUrl/apis"
$getApiUrl = $apisBaseUrl + "?workspace=$workspaceId"
Write-Host "Querying current apis from $getApiUrl with api Key $apiKey"

$response = InvokeWithRetry $getApiUrl 'GET' $null | ConvertFrom-Json
$apis = $response.apis | Where-Object { $_.name -eq $documentTitle }

Write-Host "Response - '$response'"

if($apis.Length -gt 1) {
    throw "Expected one or less apis to match '$documentTitle' since it's managed by automation. Found: $apis"
}

$updateApiUrl = $apisBaseUrl + "?workspace=$workspaceId"
$apiHttpMethod = "POST"
if($apis.Length -eq 1) {
    $apiHttpMethod = "PUT"
    $apiId = $apis[0].id
    $updateApiUrl = $apisBaseUrl + "/" + $apiId + "?workspace=$workspaceId"
    Write-Host "Found an existing api; using url $updateApiUrl"
}

## Create or update api
$apiPayload = @"
{
    "api": {
        "name": "$documentTitle",
        "summary": "Contains api operations to manage the domain in $documentTitle",
        "description": "Contact support (contact@ragibull.com) in case of issues"
    }
}
"@

##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey
Write-Host "Updating api using url $updateApiUrl with api key $apiKey"
$response = Invoke-RestMethod $updateApiUrl -Method $apiHttpMethod -Headers $defaultHeaders -Body $apiPayload | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$apiId = $response.api.id
Write-Host "Updated api with name $documentTitle and id $apiUid"

## Create updated api version
##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey

$apiVersionUrl = $apisBaseUrl + "/" + $apiId + "/" + "versions?workspace=" + $workspaceId
Write-Host "Calling $apiVersionUrl with API key $apiKey"
$response = Invoke-RestMethod $apiVersionUrl -Method 'GET' -Headers $defaultHeaders | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$apiVersions = $response.versions
$latestName = $apiVersions[0].name
if($latestName -eq "Draft"){
    $targetApiVersion = "1"
} else {
    $targetApiVersion = [int]$apiVersions[0].name + 1
}

# Create new api version
$apiVersionPayload = @"
{
"version": {
    "name": "$targetApiVersion"
}
}
"@

##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey
Write-Host "Creating api version using url $apiVersionUrl with API Key $apiKey"
Write-Host $apiVersionPayload
$response = Invoke-RestMethod $apiVersionUrl -Method "POST" -Headers $defaultHeaders -Body $apiVersionPayload | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$apiVersionId = $response.version.id

## Attach schema
$schemaUrl = $apisBaseUrl + "/" + $apiId + "/" + "versions/" + $apiVersionId + "/schemas?workspace=$workspaceId"
$openApiSpecificationTextEscaped = $openApiSpecificationText | ConvertTo-Json -Depth 100
$schemaPayload = @"
{
    "schema": {
        "language": "json",
        "type": "openapi3",
        "schema": $openApiSpecificationTextEscaped
    }
}
"@

##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey
Write-Host "Importing schema to url $schemaUrl with api key $apiKey"

$response = Invoke-RestMethod $schemaUrl -Method "POST" -Headers $defaultHeaders -Body $schemaPayload | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$schemaId = $response.schema.id
Write-Host "Updated schema with id $schemaId"

## Link environments
$environmentIds = New-Object Collections.Generic.List[string]
$environmentsUrl = "$postmanBaseUrl/environments?workspace=$workspaceId"
$targetEnvironmentPrefix = "$environment - $domain"

##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey
Write-Host "Calling $environmentsUrl with api key $apiKey"
$response = Invoke-RestMethod $environmentsUrl -Method 'GET' -Headers $defaultHeaders | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$response.environments | Where-Object { $_.name -match $targetEnvironmentPrefix } | ForEach-Object{
    $environmentIds.Add("""" + $_.uid + """")
}

## Delete previous versions 
$versionsUrl = $apisBaseUrl + "/" + $apiId + "/" + "versions"
$response = Invoke-RestMethod $versionsUrl -Method 'GET' -Headers $defaultHeaders | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$response.versions | Where-Object { $_.id -ne $apiVersionId } | ForEach-Object{
    Write-Host "Deleting previous version $_.id for API $apiId"
    $deleteVersionUrl = $versionsUrl + "/" + $_.id
    Invoke-RestMethod $deleteVersionUrl -Method 'DELETE' -Headers $defaultHeaders | ConvertTo-Json -Depth 100 | ConvertFrom-Json
}

$environmentsFormatted = [String]::Join(",", $environmentIds.ToArray())
$relationsUrl = $versionsUrl + "/" + $apiVersionId + "/relations"
$relationsPayload = @"
{
    "environment": [$environmentsFormatted] 
}
"@

##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey

Write-Host "Linking environments to $relationsUrl with api key $apiKey with payload:"
Write-Host $relationsPayload
$response = Invoke-RestMethod $relationsUrl -Method "POST" -Headers $defaultHeaders -Body $relationsPayload | ConvertTo-Json -Depth 100 | ConvertFrom-Json

##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey

## Drop previous collection
$getCollectionsUrl = "https://api.getpostman.com/collections"
Write-Host "Calling $getCollectionsUrl with $apiKey"
$response = Invoke-RestMethod $getCollectionsUrl -Method 'GET' -Headers $defaultHeaders | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$collections = $response.collections | Where-Object { $_.name -eq $documentTitle }

if($collections.Length -gt 1) {
    throw "Expected one or less collections to match '$documentTitle'. Found: $collections"
}

if($collections.Length -eq 0) {
    ## Create collection
    $collectionUrl = $apisBaseUrl + "/" + $apiId + "/" + "versions/" + $apiVersionId + "/schemas/" + $schemaId + "/collections?workspace=$workspaceId"
    $collectionPayload = @"
{
    "name": "$documentTitle",
    "relations": [
        {
            "type": "documentation"
        }
    ]
}
"@

    ##  Get random API key
    $apiKey = $apiKeys | Get-Random
    $defaultHeaders['X-Api-Key'] = $apiKey

    Start-Sleep -s 20
    Write-Host "Matching collection to api: $collectionUrl with api key $apiKey"
    $response = Invoke-RestMethod $collectionUrl -Method "POST" -Headers $defaultHeaders -Body $collectionPayload | ConvertTo-Json -Depth 100 | ConvertFrom-Json
    $collection = $response.collection
} else {
    $collection = $collections[0]
}

$collectionId = $collection.id
$collectionUid = $collection.uid

Write-Host "Matched schema to collection id $collectionId and uid $collectionUid"

## Generate api docs using cli
$sourceFilePath = "$PSScriptRoot/$documentTitle.json"
$targetFilePath = "$PSScriptRoot/$documentTitle - postman.json"

## Fix date times
$cleanedOpenApiSpecificationText = $openApiSpecificationText -replace """format"": ""date-time""", ""

Set-Content -Path $sourceFilePath -Value $cleanedOpenApiSpecificationText
Write-Host "Preparing to generate file $targetFilePath from $sourceFilePath"
openapi2postmanv2 -s $sourceFilePath -o $targetFilePath -p --options "collapseFolders=true,requestParametersResolution=Example,exampleParametersResolution=Example,folderStrategy=Tags"

## Substitute workaround for circular references
$circularReferenceRegex = '(\\"value.*?detected>\\")'
$rawCollectionDefinition = Get-Content -Path $targetFilePath
$rawCollectionDefinition = $rawCollectionDefinition -replace "$circularReferenceRegex", ""

## Increment any lines matching {{i}}
$incrementLetters = New-Object Collections.Generic.List[string]
$incrementLetters.Add("i")
$incrementLetters.Add("j")

$parameterizedCollectionDefinition = ""
ForEach ($line in $($rawCollectionDefinition -split "`r`n"))
{
    ForEach ($incrementLetter in $incrementLetters)
    {
        # The openapi2postmanv2 tool fakes collections always with 2 items, and for matching post/put requests unique incrementing ids need to be passed to the apis
        # e.g Machine-1, Machine-2 for post and put
        $global:i = 1
        $line = [regex]::replace($line, "\{\{$incrementLetter\}\}", {"$($global:i)"; If ($global:i -eq 2) {$global:i = 1} Else {$global:i++} })
    }

    $parameterizedCollectionDefinition = $parameterizedCollectionDefinition + $line
}

$collectionDefinition = $parameterizedCollectionDefinition | ConvertFrom-Json

## Add test script to each event node
Write-Host "Preparing to add pre and post request scripts"
$PreRequest = Get-Content ("$PSScriptRoot/PreRequestScript.json") | ConvertFrom-Json
$postRequest = Get-Content ("$PSScriptRoot/PostRequestScript.json") | ConvertFrom-Json

ForEach ($operation in $collectionDefinition.item){
    $operation.update | % {$operation.event += $preRequest}
    $operation.update | % {$operation.event += $postRequest}
    
    ForEach ($subOperation in $operation.item){
        $subOperation.update | % {$subOperation.event += $preRequest}
        $subOperation.update | % {$subOperation.event += $postRequest}
    }
}

Write-Host "Added pre and post request scripts"

Write-Host "Finalized pre-request substitution"

##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey

## Reload collection with details
$collectionUrl = $postmanBaseUrl + "/collections/$collectionUid"
Write-Host "Loading collection details: $collectionUrl with $apiKey"
$collectionPayload = Invoke-RestMethod $collectionUrl -Method "GET" -Headers $defaultHeaders | ConvertTo-Json -Depth 100 | ConvertFrom-Json
$collectionPayload.collection.item = $collectionDefinition.item

##  Get random API key
$apiKey = $apiKeys | Get-Random
$defaultHeaders['X-Api-Key'] = $apiKey

## Update collection with postman format document
Write-Host "Updating collection: $collectionUrl with $apiKey"
$collectionPayloadBody = $collectionPayload | ConvertTo-Json -Depth 100
$response = Invoke-RestMethod $collectionUrl -Method "PUT" -Headers $defaultHeaders -Body $collectionPayloadBody
Write-Host "Finalized $collectionUid"