/*
 * AzNames Module - Starter Pack
 * This Bicep module helps to automate resource name generation following the recommended 
 * naming convention and abbreviations for Azure resource types.
 *
 * Authors: Francesco Sodano, Dominique Broeglin
 * Github: https://github.com/francesco-sodano/AZNames-bicep
 * 
 * Starter Pack Kit - Storage Account Module
 */

 param location string
 param name string
 param tags object = {}
 
 @allowed([
   'BlobStorage'
   'BlockBlobStorage'
   'FileStorage'
   'Storage'
   'StorageV2'
 ])
 param kind string = 'StorageV2'
 
 @allowed([
   'Premium_LRS'
   'Premium_ZRS'
   'Standard_LRS'
   'Standard_GRS'
   'Standard_GZRS'
   'Standard_RAGRS'
   'Standard_RAGZRS'
   'Standard_ZRS'
 ])
 param skuName string = 'Standard_LRS'
 
 resource storage 'Microsoft.Storage/storageAccounts@2021-06-01' = {
   name: toLower(replace(name, '-', ''))
   location: location
   kind: kind
   sku: {
     name: skuName
   }
   tags: union(tags, {
     displayName: name
   })
   properties: {
     accessTier: 'Hot'
     supportsHttpsTrafficOnly: true
   }
 }
 
 output id string = storage.id
 output name string = storage.name
 // output primaryKey string = listKeys(storage.id, storage.apiVersion).keys[0].value
 output primaryEndpoints object = storage.properties.primaryEndpoints
 // output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
