using './solution.bicep'

param environment
param companyName = 'snpd'
param location = deployment().location
param FileShareName = 'aznamingtooldata'
param ResourceGroupName = 'AzureNamingTool-${environment}-rg'
param storageAccountName = 'st${companyName}${uniqueString(ResourceGroupName)}'

