﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

using Microsoft.VisualStudio.Services.Client;

using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json.Linq;
using TestApp2.Models;
using static TestApp2.Models.TokenMolel;

namespace TestApp2.Tools
{
    public class GetInfo
    {

        /// <summary>
        /// Gets a new access
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public static async Task<TokenModel> RefreshToken(string refreshToken)
        {
            TokenModel token = null;
            String error = null;
            if (!String.IsNullOrEmpty(refreshToken))
            {
                // Сформировать запрос на обмен кода аутентификации на токен доступа и токен обновления
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, ConfigurationManager.AppSettings["TokenUrl"]);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Dictionary<String, String> form = new Dictionary<String, String>()
                {
                    { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                    { "client_assertion", ConfigurationManager.AppSettings["ClientAppSecret"] },
                    { "grant_type", "refresh_token" },
                    { "assertion", refreshToken },
                    { "redirect_uri", ConfigurationManager.AppSettings["CallbackUrl"] }
                };
                requestMessage.Content = new FormUrlEncodedContent(form);

                // Сделайте запрос на обмен кода аутентификации на токен доступа (и токен обновления)
                HttpResponseMessage responseMessage = await new HttpClient().SendAsync(requestMessage);

                if (responseMessage.IsSuccessStatusCode)
                {
                    // Обрабатывать успешный запрос
                    String body = await responseMessage.Content.ReadAsStringAsync();
                    token = JObject.Parse(body).ToObject<TokenModel>();
                }
                else
                {
                    error = responseMessage.ReasonPhrase;
                }
            }
            else
            {
                error = "Invalid refresh token";
            }

            return token;
        }
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
                            var resultDictionary = new Dictionary<int, Dictionary<string,TesterModel>>();
                            // get details for each work item in the batch
                            witClient = connection.GetClient<WorkItemTrackingHttpClient>();
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
                                    if (!resultDictionary.ContainsKey(rbnId)) {
                                        resultDictionary.Add(rbnId, new Dictionary<string, TesterModel>());
                                        resultDictionary[rbnId].Add("0", new TesterModel());//итоговая сумма для каждого
                                    }
                                    var i = 1; // показывает сколько у нас тестеров
                                    while  (workItem.Fields.ContainsKey("Custom.ReleaseTester" + i))
                                    {
                                        // id группы, если такой еще не создали, то создаем
                                        var id = ((IdentityRef)workItem.Fields["Custom.ReleaseTester"+i]).Id;
                                        if (!resultDictionary[rbnId].ContainsKey(id))
                                            resultDictionary[rbnId].Add(id, new TesterModel());
                                        // получить имя
                                        var name  = ((IdentityRef)workItem.Fields["Custom.ReleaseTester" + i]).DisplayName;
                                        // Определяем данные о такске(сложность и время)
                                        var compl = int.Parse(workItem.Fields["Custom.TestingComplexity"].ToString());
                                        var time = int.Parse(workItem.Fields["Custom.ReleaseTestingTime" + i].ToString());

                                        resultDictionary[rbnId][id].lastName = name;
                                        resultDictionary[rbnId][id].AddTaskData(compl, time);
                                        //добавлять новую сумму
                                        resultDictionary[rbnId]["0"].AddTaskData(compl, time);

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