using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TestApp2.Models;
using TestApp2.Tools;
using static TestApp2.Models.TokenMolel;

namespace TestApp2.Controllers
{
    public class ReadyForReleaseController : Controller
    {
        // GET: ReadyForRelease
        public async Task<ActionResult> TestsInfo()
        {
            // обновить токен
            var token = (TokenModel)Session["token"];
            Session["token"] = await GetInfo.RefreshToken(token.RefreshToken);
            // обновить соединение 
            var connect = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential(((TokenModel)Session["token"]).AccessToken));
            Session["connect"] = connect;
            ViewBag.Name = connect.AuthorizedIdentity.DisplayName;
            //GetInfo.SampleREST(connect);
            var queriResult = ExecuteItemsSelectionWQery(connect);
            if (queriResult != null)
            {
                var result_tuple = reportGenerate(queriResult, connect);
                var result_dict = result_tuple.Item1;
                var fail = result_tuple.Item2;
                ViewBag.Fail = fail;
                ViewBag.Table = result_dict;
            }

            List<string> tags = new List<string>() { "test", "do it" };
            ViewBag.Tags = tags;

            return View();
        }

        //public List<string> GetListOfTags(VssConnection connection)
        //{
        //    Guid projectId = ClientSampleHelpers.FindAnyProject(this.Context).Id;

        //    TaggingHttpClient taggingClient = connection.GetClient<TaggingHttpClient>();

        //    WebApiTagDefinitionList listofTags = taggingClient.GetTagsAsync(projectId).Result;

        //    Console.WriteLine("List of tags:");

        //    foreach (var tag in listofTags)
        //    {
        //        Console.WriteLine("  ({0}) - {1}", tag.Id.ToString(), tag.Name);
        //    }

        //    return listofTags;
        //}

        public static WorkItemQueryResult ExecuteItemsSelectionWQery(VssConnection _connection)
        {
            var teamProjectName = "WorkPractice";
            VssConnection connection = _connection;
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

                // Выполняется запрос и получается егоо результат
                return witClient.QueryByIdAsync(newBugsQuery.Id).Result; ;
            }
            return null;
        }

        public static (Dictionary<int, Dictionary<string, TesterModel>>, string) reportGenerate(WorkItemQueryResult result, VssConnection connection)
        {
            string result_string = "";
            var resultDictionary = new Dictionary<int, Dictionary<string, TesterModel>>();
            string fail = "";
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
                        var witClient = connection.GetClient<WorkItemTrackingHttpClient>();
                        List<WorkItem> workItems = witClient.GetWorkItemsAsync(workItemRefs.Select(wir => wir.Id)).Result;
                        foreach (WorkItem workItem in workItems)
                        {
                            // выбор тега
                            //if (workItem.Fields["System.Tags"].ToString().Contains(""))
                            {
                                // первый тестер должен иметь 2 парамета, второй либо имеет оба парамета, либо ни одного, при этом сложность и ветка должны быть указаны
                                if (IsCorrect(workItem))
                                { fail += workItem.Fields["System.Title"] + "Incorrect field card, task is not counted\n"; continue; }

                                // номер для группировки
                                int rbnId = int.Parse(workItem.Fields["Custom.ReleaseBranchBuildNumber"].ToString());

                                //проверяем была ли уже создана такая группа если нет то добавим
                                if (!resultDictionary.ContainsKey(rbnId))
                                {
                                    resultDictionary.Add(rbnId, new Dictionary<string, TesterModel>());
                                    resultDictionary[rbnId].Add("0", new TesterModel());//итоговая сумма для каждого
                                }
                                var i = 1; // показывает сколько у нас тестеров
                                while (workItem.Fields.ContainsKey("Custom.ReleaseTester" + i))
                                {
                                    // id группы, если такой еще не создали, то создаем
                                    var id = ((IdentityRef)workItem.Fields["Custom.ReleaseTester" + i]).Id;
                                    if (!resultDictionary[rbnId].ContainsKey(id))
                                        resultDictionary[rbnId].Add(id, new TesterModel());
                                    // получить имя
                                    var name = ((IdentityRef)workItem.Fields["Custom.ReleaseTester" + i]).DisplayName;
                                    // Определяем данные о такске(сложность и время)
                                    var compl = int.Parse(workItem.Fields["Custom.TestingComplexity"].ToString());
                                    var time = int.Parse(workItem.Fields["Custom.ReleaseTestingTime" + i].ToString());
                                    //записать
                                    resultDictionary[rbnId][id].lastName = name;
                                    resultDictionary[rbnId][id].AddTaskData(compl, time);

                                    resultDictionary[rbnId]["0"].AddTaskData(compl, time);

                                    // next
                                    i++;
                                }
                            }
                            // write work item to console
                            //result_string += ("{0} {1}", workItem.Id, workItem.Fields["System.Title"]);
                            fail += workItem.Fields["System.Title"] + " - good\n";
                        }
                    }
                    skip += batchSize;
                }
                while (workItemRefs.Count() == batchSize);
            }

            return (resultDictionary, fail);
        }

        // первый тестер должен иметь 2 парамета, второй либо имеет оба парамета, либо ни одного, при этом сложность и ветка должны быть указаны
        private static bool IsCorrect(WorkItem workItem) =>
            (!(workItem.Fields.ContainsKey("Custom.ReleaseTester1") && workItem.Fields.ContainsKey("Custom.ReleaseTestingTime1")) ||
            (workItem.Fields.ContainsKey("Custom.ReleaseTester2") ^ workItem.Fields.ContainsKey("Custom.ReleaseTestingTime2")) ||
            (!workItem.Fields.ContainsKey("Custom.TestingComplexity")) || (!workItem.Fields.ContainsKey("Custom.ReleaseBranchBuildNumber")));


        public static string SampleREST(VssConnection _connection)
        {
            var teamProjectName = "WorkPractice";
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
                string result_string = "";
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
                            var resultDictionary = new Dictionary<int, Dictionary<string, TesterModel>>();
                            // get details for each work item in the batch
                            List<WorkItem> workItems = witClient.GetWorkItemsAsync(workItemRefs.Select(wir => wir.Id)).Result;
                            foreach (WorkItem workItem in workItems)
                            {
                                // выбор тега
                                //if (workItem.Fields["System.Tags"].ToString().Contains(""))
                                {
                                    // первый тестер должен иметь 2 парамета, второй либо имеет оба парамета, либо ни одного, при этом сложность и ветка должны быть указаны
                                    if (!(workItem.Fields.ContainsKey("Custom.ReleaseTester1") && workItem.Fields.ContainsKey("Custom.ReleaseTestingTime1")) ||
                                        (workItem.Fields.ContainsKey("Custom.ReleaseTester2") ^ workItem.Fields.ContainsKey("Custom.ReleaseTestingTime2")) ||
                                       (!workItem.Fields.ContainsKey("Custom.TestingComplexity")) || (!workItem.Fields.ContainsKey("Custom.ReleaseBranchBuildNumber")))
                                    { result_string += workItem.Fields["System.Title"] + "<- bad boy "; continue; }

                                    // номер для группировки
                                    int rbnId = int.Parse(workItem.Fields["Custom.ReleaseBranchBuildNumber"].ToString());
                                    //проверяем была ли уже создана такая группа если нет то добавим
                                    if (!resultDictionary.ContainsKey(rbnId))
                                        resultDictionary.Add(rbnId, new Dictionary<string, TesterModel>());
                                    var i = 1; // показывает сколько у нас тестеров
                                    while (workItem.Fields.ContainsKey("Custom.ReleaseTester" + i))
                                    {
                                        // id группы, если такой еще не создали, то создаем
                                        var id = ((IdentityRef)workItem.Fields["Custom.ReleaseTester" + i]).Id;
                                        if (!resultDictionary[rbnId].ContainsKey(id))
                                            resultDictionary[rbnId].Add(id, new TesterModel());
                                        // получить имя
                                        var name = ((IdentityRef)workItem.Fields["Custom.ReleaseTester" + i]).DisplayName;
                                        // Определяем данные о такске(сложность и время)
                                        var compl = int.Parse(workItem.Fields["Custom.TestingComplexity"].ToString());
                                        var time = int.Parse(workItem.Fields["Custom.ReleaseTestingTime" + i].ToString());

                                        resultDictionary[rbnId][id].lastName = name;
                                        resultDictionary[rbnId][id].AddTaskData(compl, time);
                                        // next
                                        i++;
                                    }
                                }
                                // write work item to console
                                result_string += ("{0} {1}", workItem.Id, workItem.Fields["System.Title"]);
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
                return result_string;
            }
            return " ";
        }
    }
}