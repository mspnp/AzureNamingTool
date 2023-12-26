// Web App - define connection to user GitHub which will create action to update when the user's fork is updated

targetScope = 'subscription'
// Need to figure out how to get image in Linux vs Windows format - dependant on the docker.exe architecture/build

metadata name = 'AZ Bicep - Deploy Azure Naming Tool'
metadata description = 'Orchestration module to deploy the Azure Naming Tool Web App. https://github.com/SNPD-Org-01/AzureNamingTool/tree/main'

@allowed([
  'prod'
  'qa'
  'dev'
  'sbx'
  'lbx'
])
@description('Environment to deploy to. [prod|qa|dev|sbx|lbx]')
param parEnvironment string

@minLength(3)
@maxLength(5)
@description('Company Name Identifier. (3-5 characters)')
param parCompanyName string = 'snpd'

@description('Location / Region for deployment.')
param parLocation string = deployment().location

@description('File Share Folder Name.')
param parFileShareName string = 'aznamingtooldata'

@description('Resource Group Name.')
param parResourceGroupName string    // Get this from the ADO Pipeline

@description('The name of the Storage Account')
param parStorageAccountName string = 'st${parCompanyName}${uniqueString(parResourceGroupName)}'

var varAppServicePlanName = 'aznamingtool-asp'
var varWebSiteName = '${parCompanyName}-aznamingtool-${parEnvironment}'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: parResourceGroupName
  location: parLocation
}

module rgManageIdStor './modules/rgManageIdStor.bicep' = {
  scope: resourceGroup
  name: 'mod_${resourceGroup.name}_manageIdStor'
  params: {
    FileShareName: parFileShareName
    location: parLocation
    storageAccountName: parStorageAccountName
  }
}

/*  REMOVED DUE TO Complexities in assigning AD Roles via deployment script
module dsCreateAppReg './modules/ds_createAppReg.bicep' = {
  scope: resourceGroup
  name: 'mod_ds_CreateAppRegistration'
  params: {
    location: location
    managedUserId: rgManageIdStor.outputs.managedIdentityPrincipalId
    managedIdName: rgManageIdStor.outputs.managedIdentityName
    websiteName: webSiteName
  }
}
*/

module rgWebApp './modules/rgWebApp.bicep' = {
  scope: resourceGroup
  name: 'mod_${resourceGroup.name}_WebApp'
  params: {
    appServicePlanName: varAppServicePlanName
    location: parLocation
    storageAccountName: parStorageAccountName
    storageAccountResId: rgManageIdStor.outputs.storageAccountResID
    storageAccountAPI: rgManageIdStor.outputs.storageAccountAPI
    // appRegClientId: dsCreateAppReg.outputs.clientId
    webSiteName: varWebSiteName
  }
}
