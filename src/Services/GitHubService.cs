using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
#if (Polly)
using ProjectTemplate.Extensions;
#endif

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
            #if (Polly)
            var response = await this.Client.GetWithPolly("/emojiss");
            #else
            var response = await this.Client.GetAsync("/emojiss");
            #endif
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