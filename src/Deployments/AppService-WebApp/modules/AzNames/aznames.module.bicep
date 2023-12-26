/*
 * Azure Bicep Module:
 * Azure Naming Convention - Azure Resoruce refName Generator
 *
 * Authors: Francesco Sodano, Dominique Broeglin
 * Github: https://github.com/francesco-sodano/AZNames-bicep
 * 
 */

 @description('suffixes for naming - if not specified, the names will refer to an APPDEMO application in a DEV environment')
 param suffixes array = [
   'appdemo'
   'dev'
 ]
 
 // Parameters that are part of the goverance and should be fixed in the module
 // *****************************************************************************
 @description('Custom ending value for naming - if not specified, part of resourceGroup.Id will be used')
 param uniquifier string = resourceGroup().id
  
 @description('Max length of the uniqueness suffix to be added from 0 to 5 - if not specified, the length will be 3')
 @minValue(0)
 @maxValue(5)
 param uniquifierLength int = 3
  
 @description('Dashes will be used when possible')
 param useDashes bool = true
 
 // *****************************************************************************
 
 var separator = useDashes ? '-' : ''
 var useUniquifier = uniquifierLength == 0
 var azureDummyResource = '????'
 var definedSuffix = toLower('${separator}${replace(replace(replace(string(suffixes), '["', ''), '"]', ''), '","', separator)}')
 var uniquifierEnd = useUniquifier ? '' : toLower('${separator}${substring(uniqueString(uniquifier), 0, uniquifierLength)}')
 
 var resourceNameTemplate = '${azureDummyResource}${definedSuffix}'
 var resourceNameTemplateNoDashes = replace(resourceNameTemplate, separator, '')
 var uniqueResourceNameTemplate = '${azureDummyResource}${definedSuffix}${uniquifierEnd}'
 var uniqueResourceNameTemplateNoDashes = replace(uniqueResourceNameTemplate, separator, '')
 
 output names object = {
   analysisServicesServer: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'as'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'as')), 63))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'as'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'as')), 63))
     prefix: 'as'
     maxLength: 63
     scope: 'resourceGroup'
     dashes: false
   }
   apiManagement: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'apim'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'apim')), 50))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'apim'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'apim')), 50))
     prefix: 'apim'
     maxLength: 50
     scope: 'global'
     dashes: false
   }
   appConfiguration: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'appcg'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'appcg')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'appcg'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'appcg')), 50))
     prefix: 'appcg'
     maxLength: 50
     scope: 'resourceGroup'
     dashes: true
   }
   appServicePlan: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'plan'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'plan')), 40))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'plan'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'plan')), 40))
     prefix: 'plan'
     maxLength: 40
     scope: 'resourceGroup'
     dashes: true
   }
   appService: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'app'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'app')), 60))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'app'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'app')), 60))
     prefix: 'app'
     maxLength: 60
     scope: 'global'
     dashes: true
   }
   applicationGateway: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'agw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'agw')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'agw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'agw')), 80))
     prefix: 'agw'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   applicationInsights: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'appi'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'appi')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'appi'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'appi')), 260))
     prefix: 'appi'
     maxLength: 260
     scope: 'resourceGroup'
     dashes: true
   }
   applicationSecurityGroup: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'asg'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'asg')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'asg'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'asg')), 80))
     prefix: 'asg'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   automationAccount: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'aa'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'aa')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'aa'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'aa')), 50))
     prefix: 'aa'
     maxLength: 50
     scope: 'resourceGroup'
     dashes: true
   }
   automationCertificate: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'aacert'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'aacert')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'aacert'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'aacert')), 128))
     prefix: 'aacert'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   automationCredential: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'aacred'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'aacred')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'aacred'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'aacred')), 128))
     prefix: 'aacred'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   automationRunbook: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'aacred'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'aacred')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'aacred'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'aacred')), 63))
     prefix: 'aacred'
     maxLength: 63
     scope: 'parent'
     dashes: true
   }
   automationSchedule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'aasched'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'aasched')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'aasched'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'aasched')), 128))
     prefix: 'aasched'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   automationVariable: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'aavar'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'aavar')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'aavar'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'aavar')), 128))
     prefix: 'aavar'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   availabilitySet: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'avail'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'avail')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'avail'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'avail')), 80))
     prefix: 'avail'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   bastionHost: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'bas'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'bas')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'bas'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'bas')), 80))
     prefix: 'bas'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   batchAccount: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'ba'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'ba')), 24))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'ba'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'ba')), 24))
     prefix: 'ba'
     maxLength: 24
     scope: 'region'
     dashes: false
   }
   batchApplication: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'baapp'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'baapp')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'baapp'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'baapp')), 64))
     prefix: 'baapp'
     maxLength: 64
     scope: 'parent'
     dashes: true
   }
   batchCertificate: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'bacert'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'bacert')), 45))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'bacert'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'bacert')), 45))
     prefix: 'bacert'
     maxLength: 45
     scope: 'parent'
     dashes: true
   }
   batchPool: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'bapool'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'bapool')), 24))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'bapool'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'bapool')), 24))
     prefix: 'bapool'
     maxLength: 24
     scope: 'parent'
     dashes: true
   }
   cdnEndpoint: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'cdn'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'cdn')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'cdn'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'cdn')), 50))
     prefix: 'cdn'
     maxLength: 50
     scope: 'global'
     dashes: true
   }
   cdnProfile: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'cdnprof'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'cdnprof')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'cdnprof'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'cdnprof')), 260))
     prefix: 'cdnprof'
     maxLength: 260
     scope: 'resourceGroup'
     dashes: true
   }
   cognitiveService: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'cog'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'cog')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'cog'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'cog')), 64))
     prefix: 'cog'
     maxLength: 64
     scope: 'resourceGroup'
     dashes: true
   }
   containerGroup: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'ci'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'ci')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'ci'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'ci')), 63))
     prefix: 'ci'
     maxLength: 63
     scope: 'resourceGroup'
     dashes: true
   }
   containerRegistry: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'cr'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'cr')), 63))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'cr'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'cr')), 63))
     prefix: 'cr'
     maxLength: 63
     scope: 'resourceGroup'
     dashes: false
   }
   containerRegistryWebhook: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'crwh'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'crwh')), 50))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'crwh'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'crwh')), 50))
     prefix: 'crwh'
     maxLength: 50
     scope: 'resourceGroup'
     dashes: false
   }
   cosmosdbService: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'cosmos'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'cosmos')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'cosmos'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'cosmos')), 63))
     prefix: 'cosmos'
     maxLength: 63
     scope: 'resourceGroup'
     dashes: true
   }
   customProvider: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'prov'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'prov')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'prov'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'prov')), 64))
     prefix: 'prov'
     maxLength: 64
     scope: 'resourceGroup'
     dashes: true
   }
   dashboard: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'dsb'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'dsb')), 160))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'dsb'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'dsb')), 160))
     prefix: 'dsb'
     maxLength: 160
     scope: 'parent'
     dashes: true
   }
   dataFactory: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adf'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adf')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adf'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adf')), 63))
     prefix: 'adf'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   dataFactoryDatasetMysql: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfmysql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfmysql')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfmysql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfmysql')), 260))
     prefix: 'adfmysql'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryDatasetPostgresql: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfpsql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfpsql')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfpsql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfpsql')), 260))
     prefix: 'adfpsql'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryDatasetSqlServerTable: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfmssql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfmssql')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfmssql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfmssql')), 260))
     prefix: 'adfmssql'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryIntegrationRuntimeManaged: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfir'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfir')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfir'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfir')), 63))
     prefix: 'adfir'
     maxLength: 63
     scope: 'parent'
     dashes: true
   }
   dataFactoryLinkedServiceDataLakeStorageGen2: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfsvst'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfsvst')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvst'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvst')), 260))
     prefix: 'adfsvst'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryLinkedServiceKeyVault: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfsvkv'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfsvkv')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvkv'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvkv')), 260))
     prefix: 'adfsvkv'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryLinkedServiceMysql: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfsvmysql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfsvmysql')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvmysql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvmysql')), 260))
     prefix: 'adfsvmysql'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryLinkedServicePostgresql: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfsvpsql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfsvpsql')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvpsql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvpsql')), 260))
     prefix: 'adfsvpsql'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryLinkedServiceSqlServer: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfsvmssql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfsvmssql')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvmssql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfsvmssql')), 260))
     prefix: 'adfsvmssql'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryPipeline: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adfpl'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adfpl')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfpl'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adfpl')), 260))
     prefix: 'adfpl'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataFactoryTriggerSchedule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'adftg'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'adftg')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'adftg'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'adftg')), 260))
     prefix: 'adftg'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   dataLakeAnalyticsAccount: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'dla'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'dla')), 24))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'dla'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'dla')), 24))
     prefix: 'dla'
     maxLength: 24
     scope: 'global'
     dashes: false
   }
   dataLakeAnalyticsFirewallRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'dlfw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'dlfw')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'dlfw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'dlfw')), 50))
     prefix: 'dlfw'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   dataLakeStorage: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'dls'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'dls')), 24))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'dls'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'dls')), 24))
     prefix: 'dls'
     maxLength: 24
     scope: 'parent'
     dashes: false
   }
   dataLakeStorageFirewallRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'dlsfw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'dlsfw')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'dlsfw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'dlsfw')), 50))
     prefix: 'dlsfw'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   databricksWorkspace: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'dbw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'dbw')), 30))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'dbw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'dbw')), 30))
     prefix: 'dbw'
     maxLength: 30
     scope: 'resourceGroup'
     dashes: true
   }
   diskEncryptionSet: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'des'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'des')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'des'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'des')), 80))
     prefix: 'des'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   dnsZone: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'dns'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'dns')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'dns'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'dns')), 63))
     prefix: 'dns'
     maxLength: 63
     scope: 'resourceGroup'
     dashes: true
   }
   eventgridDomain: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'egd'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'egd')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'egd'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'egd')), 50))
     prefix: 'egd'
     maxLength: 50
     scope: 'resourceGroup'
     dashes: true
   }
   eventgridDomainTopic: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'egdt'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'egdt')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'egdt'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'egdt')), 50))
     prefix: 'egdt'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   eventgridEventSubscription: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'egs'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'egs')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'egs'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'egs')), 64))
     prefix: 'egs'
     maxLength: 64
     scope: 'resourceGroup'
     dashes: true
   }
   eventgridTopic: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'egt'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'egt')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'egt'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'egt')), 50))
     prefix: 'egt'
     maxLength: 50
     scope: 'resourceGroup'
     dashes: true
   }
   eventhub: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'evh'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'evh')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'evh'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'evh')), 50))
     prefix: 'evh'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   eventhubAuthorizationRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'evhar'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'evhar')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhar'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhar')), 50))
     prefix: 'evhar'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   eventhubConsumerGroup: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'evhcg'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'evhcg')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhcg'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhcg')), 50))
     prefix: 'evhcg'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   eventhubNamespace: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'evhn'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'evhn')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhn'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhn')), 50))
     prefix: 'evhn'
     maxLength: 50
     scope: 'global'
     dashes: true
   }
   eventhubNamespaceAuthorizationRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'evhnar'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'evhnar')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhnar'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhnar')), 50))
     prefix: 'evhnar'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   eventhubNamespaceDisasterRecoveryConfig: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'evhdr'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'evhdr')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhdr'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'evhdr')), 50))
     prefix: 'evhdr'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   expressRouteCircuit: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'erc'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'erc')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'erc'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'erc')), 80))
     prefix: 'erc'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   expressRouteGateway: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'ergw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'ergw')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'ergw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'ergw')), 80))
     prefix: 'ergw'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   firewall: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'afw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'afw')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'afw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'afw')), 80))
     prefix: 'afw'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   firewallPolicy: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'afwp'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'afwp')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'afwp'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'afwp')), 80))
     prefix: 'afwp'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   frontdoor: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'fd'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'fd')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'fd'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'fd')), 64))
     prefix: 'fd'
     maxLength: 64
     scope: 'global'
     dashes: true
   }
   frontdoorFirewallPolicy: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'fdfp'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'fdfp')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'fdfp'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'fdfp')), 80))
     prefix: 'fdfp'
     maxLength: 80
     scope: 'global'
     dashes: true
   }
   functionApp: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'func'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'func')), 60))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'func'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'func')), 60))
     prefix: 'func'
     maxLength: 60
     scope: 'global'
     dashes: true
   }
   hdinsightHadoopCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'hadoop'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'hadoop')), 59))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'hadoop'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'hadoop')), 59))
     prefix: 'hadoop'
     maxLength: 59
     scope: 'global'
     dashes: true
   }
   hdinsightHbaseCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'hbase'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'hbase')), 59))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'hbase'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'hbase')), 59))
     prefix: 'hbase'
     maxLength: 59
     scope: 'global'
     dashes: true
   }
   hdinsightInteractiveQueryCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'iqr'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'iqr')), 59))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'iqr'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'iqr')), 59))
     prefix: 'iqr'
     maxLength: 59
     scope: 'global'
     dashes: true
   }
   hdinsightKafkaCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'kafka'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'kafka')), 59))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'kafka'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'kafka')), 59))
     prefix: 'kafka'
     maxLength: 59
     scope: 'global'
     dashes: true
   }
   hdinsightMlServicesCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mls'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mls')), 59))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mls'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mls')), 59))
     prefix: 'mls'
     maxLength: 59
     scope: 'global'
     dashes: true
   }
   hdinsightRserverCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'rsv'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'rsv')), 59))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'rsv'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'rsv')), 59))
     prefix: 'rsv'
     maxLength: 59
     scope: 'global'
     dashes: true
   }
   hdinsightSparkCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'spark'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'spark')), 59))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'spark'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'spark')), 59))
     prefix: 'spark'
     maxLength: 59
     scope: 'global'
     dashes: true
   }
   hdinsightStormCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'storm'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'storm')), 59))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'storm'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'storm')), 59))
     prefix: 'storm'
     maxLength: 59
     scope: 'global'
     dashes: true
   }
   image: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'img'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'img')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'img'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'img')), 80))
     prefix: 'img'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   iotcentralApplication: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'iotapp'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'iotapp')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'iotapp'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'iotapp')), 63))
     prefix: 'iotapp'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   iothub: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'iot'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'iot')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'iot'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'iot')), 50))
     prefix: 'iot'
     maxLength: 50
     scope: 'global'
     dashes: true
   }
   iothubConsumerGroup: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'iotcg'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'iotcg')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'iotcg'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'iotcg')), 50))
     prefix: 'iotcg'
     maxLength: 50
     scope: 'parent'
     dashes: true
   }
   iothubProvisioningService: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'provs'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'provs')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'provs'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'provs')), 64))
     prefix: 'provs'
     maxLength: 64
     scope: 'resourceGroup'
     dashes: true
   }
   iothubProvisioningServiceCertificate: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'pcert'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'pcert')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'pcert'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'pcert')), 64))
     prefix: 'pcert'
     maxLength: 64
     scope: 'parent'
     dashes: true
   }
   keyVault: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'kv'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'kv')), 24))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'kv'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'kv')), 24))
     prefix: 'kv'
     maxLength: 24
     scope: 'global'
     dashes: true
   }
   keyVaultCertificate: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'kvc'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'kvc')), 127))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'kvc'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'kvc')), 127))
     prefix: 'kvc'
     maxLength: 127
     scope: 'parent'
     dashes: true
   }
   keyVaultKey: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'kvk'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'kvk')), 127))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'kvk'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'kvk')), 127))
     prefix: 'kvk'
     maxLength: 127
     scope: 'parent'
     dashes: true
   }
   keyVaultSecret: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'kvs'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'kvs')), 127))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'kvs'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'kvs')), 127))
     prefix: 'kvs'
     maxLength: 127
     scope: 'parent'
     dashes: true
   }
   kubernetesCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'aks'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'aks')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'aks'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'aks')), 63))
     prefix: 'aks'
     maxLength: 63
     scope: 'resourceGroup'
     dashes: true
   }
   kustoCluster: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'kc'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'kc')), 22))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'kc'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'kc')), 22))
     prefix: 'kc'
     maxLength: 22
     scope: 'global'
     dashes: false
   }
   kustoDatabase: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'kdb'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'kdb')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'kdb'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'kdb')), 260))
     prefix: 'kdb'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   kustoEventhubDataConnection: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'kehc'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'kehc')), 40))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'kehc'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'kehc')), 40))
     prefix: 'kehc'
     maxLength: 40
     scope: 'parent'
     dashes: true
   }
   lb: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'lb'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'lb')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'lb'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'lb')), 80))
     prefix: 'lb'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   lbNatRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'lbnatrl'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'lbnatrl')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'lbnatrl'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'lbnatrl')), 80))
     prefix: 'lbnatrl'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   localNetworkGateway: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'lgw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'lgw')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'lgw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'lgw')), 80))
     prefix: 'lgw'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   logAnalyticsWorkspace: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'log'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'log')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'log'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'log')), 63))
     prefix: 'log'
     maxLength: 63
     scope: 'parent'
     dashes: true
   }
   logicApp: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'logic'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'logic')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'logic'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'logic')), 80))
     prefix: 'logic'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   machineLearningWorkspace: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mlw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mlw')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mlw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mlw')), 260))
     prefix: 'mlw'
     maxLength: 260
     scope: 'resourceGroup'
     dashes: true
   }
   managedDisk: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'dsk'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'dsk')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'dsk'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'dsk')), 80))
     prefix: 'dsk'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   managedIdentity: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'id'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'id')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'id'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'id')), 128))
     prefix: 'id'
     maxLength: 128
     scope: 'resourceGroup'
     dashes: true
   }
   mapsAccount: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'map'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'map')), 98))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'map'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'map')), 98))
     prefix: 'map'
     maxLength: 98
     scope: 'resourceGroup'
     dashes: true
   }
   mariadbDatabase: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mariadb'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mariadb')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mariadb'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mariadb')), 63))
     prefix: 'mariadb'
     maxLength: 63
     scope: 'parent'
     dashes: true
   }
   mariadbFirewallRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mariafw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mariafw')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mariafw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mariafw')), 128))
     prefix: 'mariafw'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   mariadbServer: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'maria'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'maria')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'maria'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'maria')), 63))
     prefix: 'maria'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   mariadbVirtualNetworkRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mariavn'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mariavn')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mariavn'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mariavn')), 128))
     prefix: 'mariavn'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   mysqlDatabase: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mysqldb'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mysqldb')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mysqldb'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mysqldb')), 63))
     prefix: 'mysqldb'
     maxLength: 63
     scope: 'parent'
     dashes: true
   }
   mysqlFirewallRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mysqlfw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mysqlfw')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mysqlfw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mysqlfw')), 128))
     prefix: 'mysqlfw'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   mysqlServer: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mysql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mysql')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mysql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mysql')), 63))
     prefix: 'mysql'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   mysqlVirtualNetworkRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'mysqlvn'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'mysqlvn')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'mysqlvn'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'mysqlvn')), 128))
     prefix: 'mysqlvn'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   networkInterface: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'nic'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'nic')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'nic'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'nic')), 80))
     prefix: 'nic'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   networkSecurityGroup: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'nsg'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'nsg')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'nsg'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'nsg')), 80))
     prefix: 'nsg'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   networkSecurityGroupRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'nsgr'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'nsgr')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'nsgr'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'nsgr')), 80))
     prefix: 'nsgr'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   networkSecurityRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'nsgr'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'nsgr')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'nsgr'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'nsgr')), 80))
     prefix: 'nsgr'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   networkWatcher: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'nw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'nw')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'nw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'nw')), 80))
     prefix: 'nw'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   notificationHub: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'ntf'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'ntf')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'ntf'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'ntf')), 260))
     prefix: 'ntf'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   notificationHubAuthorizationRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'ntfr'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'ntfr')), 256))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'ntfr'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'ntfr')), 256))
     prefix: 'ntfr'
     maxLength: 256
     scope: 'parent'
     dashes: true
   }
   notificationHubNamespace: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'ntfns'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'ntfns')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'ntfns'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'ntfns')), 50))
     prefix: 'ntfns'
     maxLength: 50
     scope: 'global'
     dashes: true
   }
   postgresqlDatabase: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'psqldb'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'psqldb')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'psqldb'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'psqldb')), 63))
     prefix: 'psqldb'
     maxLength: 63
     scope: 'parent'
     dashes: true
   }
   postgresqlFirewallRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'psqlfw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'psqlfw')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'psqlfw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'psqlfw')), 128))
     prefix: 'psqlfw'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   postgresqlServer: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'psql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'psql')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'psql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'psql')), 63))
     prefix: 'psql'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   postgresqlVirtualNetworkRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'psqlvn'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'psqlvn')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'psqlvn'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'psqlvn')), 128))
     prefix: 'psqlvn'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   privateDnsZone: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'pdnsz'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'pdnsz')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'pdnsz'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'pdnsz')), 63))
     prefix: 'pdnsz'
     maxLength: 63
     scope: 'resourceGroup'
     dashes: true
   }
   publicIp: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'pip'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'pip')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'pip'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'pip')), 80))
     prefix: 'pip'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   publicIpPrefix: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'ippre'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'ippre')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'ippre'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'ippre')), 80))
     prefix: 'ippre'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   purviewService: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'pview'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'pview')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'pview'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'pview')), 63))
     prefix: 'pview'
     maxLength: 63
     scope: 'resourceGroup'
     dashes: true
   }
   redisCache: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'redis'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'redis')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'redis'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'redis')), 63))
     prefix: 'redis'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   redisFirewallRule: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'redisfw'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'redisfw')), 256))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'redisfw'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'redisfw')), 256))
     prefix: 'redisfw'
     maxLength: 256
     scope: 'parent'
     dashes: false
   }
   resourceGroup: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'rg'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'rg')), 90))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'rg'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'rg')), 90))
     prefix: 'rg'
     maxLength: 90
     scope: 'subscription'
     dashes: true
   }
   route: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'rt'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'rt')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'rt'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'rt')), 80))
     prefix: 'rt'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   routeTable: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'route'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'route')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'route'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'route')), 80))
     prefix: 'route'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   serviceFabricCluster: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sf'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sf')), 23))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sf'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sf')), 23))
     prefix: 'sf'
     maxLength: 23
     scope: 'region'
     dashes: true
   }
   servicebusNamespace: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sb'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sb')), 50))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sb'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sb')), 50))
     prefix: 'sb'
     maxLength: 50
     scope: 'global'
     dashes: true
   }
   servicebusQueue: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sbq'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sbq')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sbq'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sbq')), 260))
     prefix: 'sbq'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   servicebusTopic: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sbt'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sbt')), 260))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sbt'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sbt')), 260))
     prefix: 'sbt'
     maxLength: 260
     scope: 'parent'
     dashes: true
   }
   signalrService: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sgnlr'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sgnlr')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sgnlr'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sgnlr')), 63))
     prefix: 'sgnlr'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   snapshots: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'snap'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'snap')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'snap'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'snap')), 80))
     prefix: 'snap'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   sqlDatabase: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sqldb'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sqldb')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sqldb'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sqldb')), 128))
     prefix: 'sqldb'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   sqlElasticpool: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sqlep'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sqlep')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sqlep'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sqlep')), 128))
     prefix: 'sqlep'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   sqlFirewallRule: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sqlfw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sqlfw')), 128))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sqlfw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sqlfw')), 128))
     prefix: 'sqlfw'
     maxLength: 128
     scope: 'parent'
     dashes: true
   }
   sqlServer: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sql'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sql')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sql'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sql')), 63))
     prefix: 'sql'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   sqlServerManagedInstance: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'sqlmi'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'sqlmi')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'sqlmi'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'sqlmi')), 63))
     prefix: 'sqlmi'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   storageAccount: {
     refName: substring(replace(resourceNameTemplateNoDashes, azureDummyResource, 'st'), 0, min(length(replace(resourceNameTemplateNoDashes, azureDummyResource, 'st')), 24))
     uniName: substring(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'st'), 0, min(length(replace(uniqueResourceNameTemplateNoDashes, azureDummyResource, 'st')), 24))
     prefix: 'st'
     maxLength: 24
     scope: 'global'
     dashes: false
   }
   subnet: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'snet'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'snet')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'snet'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'snet')), 80))
     prefix: 'snet'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   timeSeriesInsightsEnvironment: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'deploy'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'deploy')), 90))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'deploy'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'deploy')), 90))
     prefix: 'deploy'
     maxLength: 90
     scope: 'resourceGroup'
     dashes: true
   }
   trafficManagerProfile: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'traf'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'traf')), 63))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'traf'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'traf')), 63))
     prefix: 'traf'
     maxLength: 63
     scope: 'global'
     dashes: true
   }
   virtualMachineLinux: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'vm'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'vm')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'vm'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'vm')), 64))
     prefix: 'vm'
     maxLength: 64
     scope: 'resourceGroup'
     dashes: true
   }
   virtualMachineWindows: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'vm'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'vm')), 15))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'vm'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'vm')), 15))
     prefix: 'vm'
     maxLength: 15
     scope: 'resourceGroup'
     dashes: true
   }
   virtualMachineScaleSetLinux: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'vmss'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'vmss')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'vmss'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'vmss')), 64))
     prefix: 'vmss'
     maxLength: 64
     scope: 'resourceGroup'
     dashes: true
   }
   virtualMachineScaleSetWindows: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'vmss'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'vmss')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'vmss'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'vmss')), 64))
     prefix: 'vmss'
     maxLength: 64
     scope: 'resourceGroup'
     dashes: true
   }
   virtualNetwork: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'vnet'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'vnet')), 64))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'vnet'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'vnet')), 64))
     prefix: 'vnet'
     maxLength: 64
     scope: 'resourceGroup'
     dashes: true
   }
   virtualNetworkGateway: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'vgw'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'vgw')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'vgw'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'vgw')), 80))
     prefix: 'vgw'
     maxLength: 80
     scope: 'resourceGroup'
     dashes: true
   }
   virtualNetworkPeering: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'peer'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'peer')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'peer'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'peer')), 80))
     prefix: 'peer'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
   virtualWan: {
     refName: substring(replace(resourceNameTemplate, azureDummyResource, 'vwan'), 0, min(length(replace(resourceNameTemplate, azureDummyResource, 'vwan')), 80))
     uniName: substring(replace(uniqueResourceNameTemplate, azureDummyResource, 'vwan'), 0, min(length(replace(uniqueResourceNameTemplate, azureDummyResource, 'vwan')), 80))
     prefix: 'vwan'
     maxLength: 80
     scope: 'parent'
     dashes: true
   }
 }
 