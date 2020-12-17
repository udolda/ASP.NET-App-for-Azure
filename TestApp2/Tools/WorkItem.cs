using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestApp2.Tools
{
    public class WorkItem
    {
        private TfsTeamProjectCollection _teamProjectCollection;

        public WorkItem(TfsTeamProjectCollection teamProjectCollection)
        {
            _teamProjectCollection = teamProjectCollection;
        }

        public void getInfo()
        {
            // get the WorkItemStore service
            WorkItemStore workItemStore = _teamProjectCollection.GetService<WorkItemStore>();

            // get the project context for the work item store
            Project workItemProject = workItemStore.Projects["WorkPractice"];

            // search for the 'My Queries' folder
            QueryFolder myQueriesFolder = workItemProject.QueryHierarchy.FirstOrDefault(qh => qh is QueryFolder && qh.IsPersonal) as QueryFolder;
            if (myQueriesFolder != null)
            {
                // search for the 'SOAP Sample' query
                string queryName = "SOAP Sample";
                QueryDefinition newBugsQuery = myQueriesFolder.FirstOrDefault(qi => qi is QueryDefinition && qi.Name.Equals(queryName)) as QueryDefinition;
                if (newBugsQuery == null)
                {
                    // if the 'SOAP Sample' query does not exist, create it.
                    newBugsQuery = new QueryDefinition(queryName, "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags] FROM WorkItems WHERE [System.WorkItemType] = 'Bug' AND [System.State] = 'New'");
                    myQueriesFolder.Add(newBugsQuery);
                    workItemProject.QueryHierarchy.Save();
                }

                // run the 'SOAP Sample' query
                WorkItemCollection workItems = workItemStore.Query(newBugsQuery.QueryText);
                foreach (WorkItem workItem in workItems)
                {
                    // write work item to console
                   // Console.WriteLine("{0} {1}", workItem.Id, workItem.Fields["System.Title"].Value);
                }
            }
        }
    }
}