using './AzureNamingTool-Main.bicep'

param parEnvironment = 'sbx'
param parCompanyName = 'snpd'
param parLocation = deployment().location
param parFileShareName = 'aznamingtooldata'
param parResourceGroupName = ''
param parStorageAccountName = 'st${parCompanyName}${uniqueString(parResourceGroupName)}'

