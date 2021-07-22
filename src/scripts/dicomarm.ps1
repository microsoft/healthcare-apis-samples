## PowerShell
##variables
$resourcegroupname="rg-bx222"
$location="South Central US"
$workspacename="ws2"
$dicomservicename="dicom2"

##login to azure
Connect-AzAccount 
#Connect-AzAccount SubscriptionId cc148bf2-42fb-4913-a3fb-2f284a69eb89
#Set-AzContext -Subscription a1766500-6fd5-4f5c-8515-607798271014
#Connect-AzAccount -Tenant 'xxxx-xxxx-xxxx-xxxx' -SubscriptionId 'yyyy-yyyy-yyyy-yyyy'
#Get-AzContext -ListAvailable
#Get-AzContext 


##create resource group
New-AzResourceGroup -Name $resourcegroupname -Location $location

##deploy the resource
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile "src/templates/dicomtemplate.json" -region $location -workspaceName $workspacename -dicomServiceName $dicomservicename
