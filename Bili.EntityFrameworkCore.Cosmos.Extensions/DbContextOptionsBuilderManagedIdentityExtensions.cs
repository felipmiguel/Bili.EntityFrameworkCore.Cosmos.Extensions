using Azure.Core;
using Azure.Identity;
using Bili.EntityFrameworkCore.Cosmos.Extensions.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bili.EntityFrameworkCore.Cosmos.Extensions
{
    public static class DbContextOptionsBuilderManagedIdentityExtensions
    {
        private static readonly string[] AzureManagementScope = new string[] { "https://management.azure.com/.default" };
        private static DefaultAzureCredential azureCredentials = new DefaultAzureCredential();
        

        /// <summary>
        /// Configures the context to connect to Azure Cosmos Database. It retrieves the connection information from the management API using DefaultAzureCredentials. 
        /// Cosmos DB account is identified using the subscription, resource group and account name
        /// </summary>
        /// <param name="builder">DbContextOptionsBuilder to be configured</param>
        /// <param name="subscriptionId">Subscription ID that hosts the Cosmos Database</param>
        /// <param name="resourceGroupName">Resource group name where the Cosmos Database is hosted</param>
        /// <param name="accountName">Cosmos DB account name</param>
        /// <param name="database">Cosmos DB database name</param>
        /// <returns>DbContextOptionsBuilder configured</returns>
        public static DbContextOptionsBuilder UseCosmos(this DbContextOptionsBuilder builder, string subscriptionId, string resourceGroupName, string accountName, string database)
        {
            return builder.UseCosmosWithCredentials(subscriptionId, resourceGroupName, accountName, database, azureCredentials);
        }

        /// <summary>
        /// Configures the context to connect to Azure Cosmos Database. It retrieves the connection information from the management API using DefaultAzureCredentials. 
        /// Cosmos DB account is identified using the Resource ID in this format /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}
        /// </summary>
        /// <param name="builder">DbContextOptionsBuilder to be configured</param>
        /// <param name="resourceId">Cosmos DB account resource ID using standard format /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}</param>
        /// <param name="database">Cosmos Database name</param>
        /// <returns>DbContextOptionsBuilder configured</returns>
        public static DbContextOptionsBuilder UseCosmos(this DbContextOptionsBuilder builder, string resourceId, string database)
        {
            return builder.UseCosmosWithCredentials(resourceId, database, azureCredentials);
        }

        /// <summary>
        /// Configures the context to connect to Azure Cosmos Database. It retrieves the connection information from the management API using the provided TokenCredentials.
        /// Cosmos DB account is identified using the subscription, resource group and account name
        /// This method is useful for developers if the logged account has access to more than one tenant as it allows to control the tenant to be used. For instance:
        /// <code>
        /// DefaultAzureCredential azureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        /// {
        ///    InteractiveBrowserTenantId = dbConnectionInfo.TenantId,
        ///    SharedTokenCacheTenantId = dbConnectionInfo.TenantId,
        ///    VisualStudioCodeTenantId = dbConnectionInfo.TenantId,
        ///    VisualStudioTenantId = dbConnectionInfo.TenantId
        /// });
        /// services.AddDbContextFactory<TheContext>(options => options.UseCosmosWithCredentials(subscriptionId, resourceGroupName, accountName, databaseName, azureCredential));
        /// </code>
        /// </summary>
        /// <param name="builder">DbContextOptionsBuilder to be configured</param>
        /// <param name="subscriptionId">Subscription ID that hosts the Cosmos Database</param>
        /// <param name="resourceGroupName">Resource group name where the Cosmos Database is hosted</param>
        /// <param name="accountName">Cosmos DB account name</param>
        /// <param name="database">Cosmos DB database name</param>
        /// <param name="providedCredentials">Credentials to get access to CosmosDB information (endpoint and access keys)</param>
        /// <returns>DbContextOptionsBuilder configured</returns>
        public static DbContextOptionsBuilder UseCosmosWithCredentials(this DbContextOptionsBuilder builder, string subscriptionId, string resourceGroupName, string accountName, string database, TokenCredential providedCredentials)
        {
            
            string resourceId = GetResourceId(subscriptionId, resourceGroupName, accountName);
            return builder.UseCosmosWithCredentials(resourceId, database, providedCredentials);
        }

        /// <summary>
        /// Configures the context to connect to Azure Cosmos Database. It retrieves the connection information from the management API using DefaultAzureCredentials. 
        /// Cosmos DB account is identified using the Resource ID in this format /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}
        /// This method is useful for developers if the logged account has access to more than one tenant as it allows to control the tenant to be used. For instance:
        /// <code>
        /// DefaultAzureCredential azureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        /// {
        ///    InteractiveBrowserTenantId = dbConnectionInfo.TenantId,
        ///    SharedTokenCacheTenantId = dbConnectionInfo.TenantId,
        ///    VisualStudioCodeTenantId = dbConnectionInfo.TenantId,
        ///    VisualStudioTenantId = dbConnectionInfo.TenantId
        /// });
        /// services.AddDbContextFactory<TheContext>(options => options.UseCosmosWithCredentials(resourceID, databaseName, azureCredential));
        /// </code>
        /// </summary>
        /// <param name="builder">DbContextOptionsBuilder to be configured</param>
        /// <param name="resourceId">Cosmos DB account resource ID using standard format /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}</param>
        /// <param name="database">Cosmos Database name</param>
        /// <param name="providedCredentials">Credentials to get access to CosmosDB information (endpoint and access keys)</param>
        /// <returns>DbContextOptionsBuilder configured</returns>
        public static DbContextOptionsBuilder UseCosmosWithCredentials(this DbContextOptionsBuilder builder, string resourceId, string database, TokenCredential providedCredentials)
        {
            var token = providedCredentials.GetToken(new TokenRequestContext(AzureManagementScope), default(CancellationToken));
            CosmosDBDatabaseAccountListKeys keys = RetrieveCosmosKeys(resourceId, token);
            string endpoint = RetrieveEndpoint(resourceId, token);
            return builder.UseCosmos(endpoint, keys.PrimaryMasterKey, database);
        }

        private static string GetResourceId(string subscriptionId, string resourceGroupName, string accountName)
        {
            return $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}";
        }

        private static string RetrieveEndpoint(string resourceId, AccessToken token)
        {
            string url = $"https://management.azure.com{resourceId}?api-version=2021-04-15";
            //var token = GetAccessToken(tenantId);
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", $"Bearer {token.Token}");
            var response = request.GetResponse();
            using (var responseReader = new StreamReader(response.GetResponseStream()))
            {
                var accountInfo = JsonConvert.DeserializeObject<CosmosDBDatabaseAccount>(responseReader.ReadToEnd());
                return accountInfo.properties.documentEndpoint;
            }
        }


        private static CosmosDBDatabaseAccountListKeys RetrieveCosmosKeys(string resourceId, AccessToken token)
        {
            var request = WebRequest.Create($"https://management.azure.com{resourceId}/listKeys?api-version=2021-04-15");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", $"Bearer {token.Token}");
            var response = request.GetResponse();
            using (var responseReader = new StreamReader(response.GetResponseStream()))
            {
                var cosmosKeys = JsonConvert.DeserializeObject<CosmosDBDatabaseAccountListKeys>(responseReader.ReadToEnd());
                return cosmosKeys;
            }
        }
    }
}
