$aadDashboardClientId="4edf9a98-e287-4e91-9715-d1ba7403b88f"
$aadDashboardClientSecret="d55NcXY~73.290.7b1vjcIH2Hd~1.1K-2k"
$fhirsubid='2c39b90b-3d90-419a-8d8b-8c6c8f08ed7c'
$fhirrgname="rg-fhirserver-msdn"
$fhirserviceurl="https://bxfhirserver123.azurewebsites.net"
$environmentname ="bx111"
$rgname=$fhirrgname+"new"
$location="westus2"

$sandboxappTemplate="https://raw.githubusercontent.com/zxue/fhir-server-samples/master/deploy/templates/azuredeploy-sandbox-1.json" 

Set-AzContext -SubscriptionId $fhirsubid
$rg = New-AzResourceGroup -Name $rgname  -Location $location -Force 
$rgsof = New-AzResourceGroup -Name ${rgname}-sof -Location $location -Force 


New-AzResourceGroupDeployment -TemplateUri $sandboxappTemplate -ResourceGroupName $rgname -EnvironmentName $environmentname  -fhirServiceUrl $fhirserviceurl -aadDashboardClientId  $aadDashboardClientId -aadDashboardClientSecret $aadDashboardClientSecret

