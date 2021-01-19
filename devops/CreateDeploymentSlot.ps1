Write-Host "Powershell starting"

$resourceGroupName = $OctopusParameters["ResourceGroupName"]
Write-Host "Resource group name $resourceGroupName"

$appName = $OctopusParameters["AppName"]

Write-Host "Application name $appName"

Try 
{
	Get-AzureRmWebAppSlot -ResourceGroupName $resourceGroupName -Name $appName -Slot batonbot
	Write-Host "Environment exists." -ForegroundColor blue
}
Catch 
{
	Write-Host "Environment does not exist. Create new environment" -ForegroundColor red
	Write-Host "Create new environment" -ForegroundColor green
	New-AzureRmWebAppSlot -ResourceGroupName $resourceGroupName -Name $appName -Slot batonbot
}