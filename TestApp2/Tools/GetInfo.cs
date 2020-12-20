using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

using Microsoft.VisualStudio.Services.Client;

using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace TestApp2.Tools
{
    public class GetInfo
    {
        public static void SampleREST(VssConnection _connection, string teamProjectName)
        {
            // Connection может быть создан один раз для каждого приложения, и мы будем использовать его для получения объектов httpclient.
            // Httpclients были повторно использованы между вызывающими абонентами и потоками.
            // Их срок службы управляется подключением (нам не нужно их утилизировать).
            // Это более надежно, чем прямое создание объектов httpclient.  

            // Обязательно отправьте полный full collection uri, i.e. http://myserver:8080/tfs/defaultcollection
            // We are using default VssCredentials which uses NTLM against an Azure DevOps Server.  См. дополнительные сведения
            // примеры создания учетных данных для других типов аутентификации.
            VssConnection connection = _connection;

            // Создайте экземпляр WorkItemTrackingHttpClient с помощью VssConnection
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            //Получить 2 уровня элементов иерархии запросов
            List<QueryHierarchyItem> queryHierarchyItems = witClient.GetQueriesAsync(teamProjectName, depth: 2).Result;

            // Найдите папку 'My Queries' folder
            QueryHierarchyItem myQueriesFolder = queryHierarchyItems.FirstOrDefault(qhi => qhi.Name.Equals("My Queries"));
            if (myQueriesFolder != null)
            {
                string queryName = "MyQueri";

                // See if our 'REST Sample' query already exists under 'My Queries' folder.
                QueryHierarchyItem newBugsQuery = null;
                if (myQueriesFolder.Children != null)
                {
                    newBugsQuery = myQueriesFolder.Children.FirstOrDefault(qhi => qhi.Name.Equals(queryName));
                }
                if (newBugsQuery == null)
                {
                    // if the 'REST Sample' query does not exist, create it.
                    newBugsQuery = new QueryHierarchyItem()
                    {
                        Name = queryName,
                        Wiql = "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.WorkItemType] = 'Task' AND [System.State] = 'Done'",
                        IsFolder = false
                    };
                    newBugsQuery = witClient.CreateQueryAsync(newBugsQuery, teamProjectName, myQueriesFolder.Name).Result;
                }

                // run the 'REST Sample' query
                WorkItemQueryResult result = witClient.QueryByIdAsync(newBugsQuery.Id).Result;

                if (result.WorkItems.Any())
                {
                    int skip = 0;
                    const int batchSize = 100;
                    IEnumerable<WorkItemReference> workItemRefs;
                    do
                    {
                        workItemRefs = result.WorkItems.Skip(skip).Take(batchSize);
                        if (workItemRefs.Any())
                        {
                            // get details for each work item in the batch
                            List<WorkItem> workItems = witClient.GetWorkItemsAsync(workItemRefs.Select(wir => wir.Id)).Result;
                            foreach (WorkItem workItem in workItems)
                            {
                                // write work item to console
                                Console.WriteLine("{0} {1}", workItem.Id, workItem.Fields["System.Title"]);
                            }
                        }
                        skip += batchSize;
                    }
                    while (workItemRefs.Count() == batchSize);
                }
                else
                {
                    Console.WriteLine("No work items were returned from query.");
                }
            }
        }
    }
}