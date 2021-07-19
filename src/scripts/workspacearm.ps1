## PowerShell
##variables
$resourcegroupname="rg-csstest1"
$location="South Central US"
$workspacename="bxwstest111"
$tenantid="72f988bf-86f1-41af-91ab-2d7cd011db47"
#$subscriptionid="cc148bf2-42fb-4913-a3fb-2f284a69eb89"
$subscriptionid="a1766500-6fd5-4f5c-8515-607798271014"


##login to azure
Connect-AzAccount 
#Connect-AzAccount SubscriptionId cc148bf2-42fb-4913-a3fb-2f284a69eb89
#Set-AzContext -Subscription cc148bf2-42fb-4913-a3fb-2f284a69eb89
#Connect-AzAccount -Tenant 'xxxx-xxxx-xxxx-xxxx' -SubscriptionId 'yyyy-yyyy-yyyy-yyyy'
#Get-AzContext -ListAvailable
#Get-AzContext 


##create resource group
New-AzResourceGroup -Name $resourcegroupname -Location $location

##deploy the resource
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile "src/templates/workspacetemplate.json" -region $location -workspaceName $workspacename 

