using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NulahCore.Controllers.GitHub {
    public class GitHubApi {
        public static async Task<string> Get(string ApiUrl, string AccessToken) {

            var http = new HttpClient();
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            http.DefaultRequestHeaders.Add("User-Agent", "Nulah-Core GitHub API Client");
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", AccessToken);

            var stringTask = http.GetStringAsync(ApiUrl);
            var msg = await stringTask;

            return msg;
        }

        public static async Task<T> Get<T>(string ApiUrl, string AccessToken) {

            var ApiResponse = await Get(ApiUrl, AccessToken);

            var ConvertedResponse = JsonConvert.DeserializeObject<T>(ApiResponse, new JsonSerializerSettings {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });

            return ConvertedResponse;
        }

    }
}
