using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bili.EntityFrameworkCore.Cosmos.Extensions.Model
{
    class CosmosDBDatabaseAccount
    {
        public string id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string type { get; set; }
        public string kind { get; set; }
        public Tags tags { get; set; }
        public Properties properties { get; set; }
        public Identity identity { get; set; }
    }

    public class Tags
    {
    }

    public class Properties
    {
        public string provisioningState { get; set; }
        public string documentEndpoint { get; set; }
        public object[] ipRules { get; set; }
        public bool isVirtualNetworkFilterEnabled { get; set; }
        public object[] virtualNetworkRules { get; set; }
        public string databaseAccountOfferType { get; set; }
        public bool disableKeyBasedMetadataWriteAccess { get; set; }
        public string defaultIdentity { get; set; }
        public Consistencypolicy consistencyPolicy { get; set; }
        public Writelocation[] writeLocations { get; set; }
        public Readlocation[] readLocations { get; set; }
        public Location[] locations { get; set; }
        public Failoverpolicy[] failoverPolicies { get; set; }
        public Privateendpointconnection[] privateEndpointConnections { get; set; }
        public object[] cors { get; set; }
        public bool enableFreeTier { get; set; }
        public Apiproperties apiProperties { get; set; }
        public bool enableAnalyticalStorage { get; set; }
        public Backuppolicy backupPolicy { get; set; }
        public string networkAclBypass { get; set; }
        public object[] networkAclBypassResourceIds { get; set; }
    }

    public class Consistencypolicy
    {
        public string defaultConsistencyLevel { get; set; }
        public int maxIntervalInSeconds { get; set; }
        public int maxStalenessPrefix { get; set; }
    }

    public class Apiproperties
    {
    }

    public class Backuppolicy
    {
        public string type { get; set; }
        public Periodicmodeproperties periodicModeProperties { get; set; }
    }

    public class Periodicmodeproperties
    {
        public int backupIntervalInMinutes { get; set; }
        public int backupRetentionIntervalInHours { get; set; }
    }

    public class Writelocation
    {
        public string id { get; set; }
        public string locationName { get; set; }
        public string documentEndpoint { get; set; }
        public string provisioningState { get; set; }
        public int failoverPriority { get; set; }
    }

    public class Readlocation
    {
        public string id { get; set; }
        public string locationName { get; set; }
        public string documentEndpoint { get; set; }
        public string provisioningState { get; set; }
        public int failoverPriority { get; set; }
    }

    public class Location
    {
        public string id { get; set; }
        public string locationName { get; set; }
        public string documentEndpoint { get; set; }
        public string provisioningState { get; set; }
        public int failoverPriority { get; set; }
    }

    public class Failoverpolicy
    {
        public string id { get; set; }
        public string locationName { get; set; }
        public int failoverPriority { get; set; }
    }

    public class Privateendpointconnection
    {
        public string id { get; set; }
        public Properties1 properties { get; set; }
    }

    public class Properties1
    {
        public Privateendpoint privateEndpoint { get; set; }
        public Privatelinkserviceconnectionstate privateLinkServiceConnectionState { get; set; }
    }

    public class Privateendpoint
    {
        public string id { get; set; }
    }

    public class Privatelinkserviceconnectionstate
    {
        public string status { get; set; }
        public string actionsRequired { get; set; }
    }

    public class Identity
    {
        public string type { get; set; }
        public string principalId { get; set; }
        public string tenantId { get; set; }
    }
}
