using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using TestApp2.Models;
using TestApp2.Tools;
using static TestApp2.Models.TokenMolel;

namespace TestApp2.Controllers
{
    public class ReadyForReleaseController : Controller
    {
        //GET: ReadyForRelease
        public async Task<ActionResult> TestsInfo()
        {
            // обновить токен
            var token = (TokenModel)Session["token"];
            Session["token"] = await GetInfo.RefreshToken(token.RefreshToken);
            // обновить соединение
            var connect = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential(((TokenModel)Session["token"]).AccessToken));
            Session["connect"] = connect;
            ViewBag.Name = connect.AuthorizedIdentity.DisplayName;

            var list_tags = await GetListOfTagsAsync(token.AccessToken);
            list_tags = list_tags.Select(i => i.Replace(" ", "_")).ToList();
            ViewBag.Tags = list_tags;
            Session["taglist"] = list_tags;

            ViewBag.Table = new Dictionary<int, Dictionary<string, TesterModel>>();
            ViewBag.Fail = " ";

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> TestsInfo(string tasktags)
        {
            tasktags = tasktags.Replace("_", " ");
            // обновить токен
            var token = (TokenModel)Session["token"];
            //Session["token"] = await GetInfo.RefreshToken(token.RefreshToken);
            // обновить соединение
            var connect = new VssConnection(new Uri("https://dev.azure.com/LATeamInc/"), new VssOAuthAccessTokenCredential(((TokenModel)Session["token"]).AccessToken));
            Session["connect"] = connect;
            ViewBag.Name = connect.AuthorizedIdentity.DisplayName;
            var t = Session["taglist"];
            ViewBag.Tags = Session["taglist"];
            var queriResult = ExecuteItemsSelectionWQery(connect,tasktags);
            if (queriResult != null)
            {
                var result_tuple = reportGenerate(queriResult, connect);
                ViewBag.Table = result_tuple.Item1;
                ViewBag.Fail = result_tuple.Item2;
            }

            return View();
        }

        public async Task<List<string>> GetListOfTagsAsync(string token)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage responseMessage = await client.GetAsync(new Uri("https://dev.azure.com/LATeamInc/WorkPractice/_apis/wit/tags?api-version=6.0-preview.1"));

            if (responseMessage.IsSuccessStatusCode)
            {
                // Обрабатывать успешный запрос
                String body = await responseMessage.Content.ReadAsStringAsync();
                return JObject.Parse(body)["value"].Select(i => i["name"].ToString()).ToList(); ;
            }
            return null;
        }

        public static WorkItemQueryResult ExecuteItemsSelectionWQery(VssConnection _connection, string tag)
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
                string queryName = tag+"SomeQueri";

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
                        Wiql = "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.WorkItemType] = 'Task' AND [System.State] = 'Done' AND [System.Tags] Contains '"+tag+"' ",
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

                            // первый тестер должен иметь 2 парамета, второй либо имеет оба парамета, либо ни одного, при этом сложность и ветка должны быть указаны
                            if (IsCorrect(workItem))
                            { fail += " <"+ workItem.Fields["System.Title"] + "> "; continue; }

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
                    }
                    skip += batchSize;
                }
                while (workItemRefs.Count() == batchSize);
            }

            fail += "-Incorrect field card, task is not counted";
            return (resultDictionary, fail);
        }

        // первый тестер должен иметь 2 парамета, второй либо имеет оба парамета, либо ни одного, при этом сложность и ветка должны быть указаны
        private static bool IsCorrect(WorkItem workItem) =>
            (!(workItem.Fields.ContainsKey("Custom.ReleaseTester1") && workItem.Fields.ContainsKey("Custom.ReleaseTestingTime1")) ||
            (workItem.Fields.ContainsKey("Custom.ReleaseTester2") ^ workItem.Fields.ContainsKey("Custom.ReleaseTestingTime2")) ||
            (!workItem.Fields.ContainsKey("Custom.TestingComplexity")) || (!workItem.Fields.ContainsKey("Custom.ReleaseBranchBuildNumber")));

    }
}