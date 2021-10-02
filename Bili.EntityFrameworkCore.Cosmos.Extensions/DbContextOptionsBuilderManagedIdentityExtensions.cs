using Azure.Core;
using Azure.Identity;
using Bili.EntityFrameworkCore.Cosmos.Extensions.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Bili.EntityFrameworkCore.Cosmos.Extensions
{
    public static class DbContextOptionsBuilderManagedIdentityExtensions
    {
        private static readonly string[] AzureManagementScope = new string[] { "https://management.azure.com/.default" };
        private static DefaultAzureCredential azureCredentials = new DefaultAzureCredential();
        public static DbContextOptionsBuilder UseCosmos(this DbContextOptionsBuilder builder, string subscriptionId, string resourceGroupName, string accountName, string database)
        {
            string resourceId = GetResourceId(subscriptionId, resourceGroupName, accountName);
            return builder.UseCosmos(resourceId, database);
        }

        public static DbContextOptionsBuilder UseCosmos(this DbContextOptionsBuilder builder, string resourceId, string database)
        {
            CosmosDBDatabaseAccountListKeys keys = RetrieveCosmosKeys(resourceId);
            string endpoint = RetrieveEndpoint(resourceId);
            return builder.UseCosmos(endpoint, keys.PrimaryMasterKey, database);
        }


        private static string GetResourceId(string subscriptionId, string resourceGroupName, string accountName)
        {
            return $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}";
        }

        private static string RetrieveEndpoint(string resourceId)
        {
            string url = $"https://management.azure.com{resourceId}?api-version=2021-04-15";
            var token = azureCredentials.GetToken(new TokenRequestContext(AzureManagementScope));
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


        private static CosmosDBDatabaseAccountListKeys RetrieveCosmosKeys(string resourceId)
        {
            var token = azureCredentials.GetToken(new TokenRequestContext(AzureManagementScope));
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
