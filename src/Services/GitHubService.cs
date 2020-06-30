using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectTemplate.Services
{
    public class GitHubService
    {
        public HttpClient Client { get; }

        public GitHubService(HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            // GitHub API versioning
            client.DefaultRequestHeaders.Add("Accept", 
                "application/vnd.github.v3+json");
            // GitHub requires a user-agent
            client.DefaultRequestHeaders.Add("User-Agent", 
                "HttpClientFactory-Sample");
            client.Timeout = new TimeSpan(0, 1, 0);
            Client = client;
        }

        public async Task<Dictionary<string, string>> GetEmojis()
        {
            var response = await Client.GetAsync("/emojis");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content
                .ReadAsAsync<Dictionary<string, string>>();
            return result;
        }
    }
}