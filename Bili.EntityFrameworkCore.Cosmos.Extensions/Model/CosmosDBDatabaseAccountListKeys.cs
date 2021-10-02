using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bili.EntityFrameworkCore.Cosmos.Extensions.Model
{
    public class CosmosDBDatabaseAccountListKeys
    {
        public string PrimaryMasterKey { get; set; }
        public string SecondaryMasterKey { get; set; }
        public string PrimaryReadonlyMasterKey { get; set; }
        public string SecondaryReadonlyMasterKey { get; set; }
    }

}
