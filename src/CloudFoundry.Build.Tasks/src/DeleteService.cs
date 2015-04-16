using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.CloudController.V2.Client.Data;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.Build.Tasks
{
    public class DeleteService : BaseTask
    {
        [Required]
        public string CFServiceName { get; set; }

        [Required]
        public string CFSpace { get; set; }

        public override bool Execute()
        {

            logger = new Microsoft.Build.Utilities.TaskLoggingHelper(this);

            CloudFoundryClient client = InitClient();
            
            logger.LogMessage("Deleting service {0} from space {1}", CFServiceName, CFSpace);

            Guid? spaceGuid = null;

            if (CFSpace.Length > 0)
            {
                PagedResponseCollection<ListAllSpacesResponse> spaceList = client.Spaces.ListAllSpaces(new RequestOptions() { Query = "name:" + CFSpace }).Result;

                spaceGuid = new Guid(spaceList.FirstOrDefault().EntityMetadata.Guid);
            
                if (spaceGuid == null)
                {
                    logger.LogError("Space {0} not found", CFSpace);
                    return false;
                }
            }

            var servicesList = client.Spaces.ListAllServiceInstancesForSpace(spaceGuid, new RequestOptions() { Query = "name:" + CFServiceName }).Result;

            if (servicesList.Count() > 1)
            {
                logger.LogError("There are more services named {0} in space {1}", CFServiceName, CFSpace);
                return false;
            }

            client.ServiceInstances.DeleteServiceInstance(new Guid(servicesList.First().EntityMetadata.Guid)).Wait();
            return true;
        }
    }
}
