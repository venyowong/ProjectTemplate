using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace ProjectTemplate.Services
{
    public class GitHubService
    {
        private static ConcurrentDictionary<string, IAsyncPolicy<HttpResponseMessage>> _dictionary = 
            new ConcurrentDictionary<string, IAsyncPolicy<HttpResponseMessage>>();
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
            var response = await this.GetWithPolly("/emojiss");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content
                .ReadAsAsync<Dictionary<string, string>>();
            return result;
        }

        private Task<HttpResponseMessage> GetWithPolly(string requestUri)
        {
            IAsyncPolicy<HttpResponseMessage> policy = null;
            if (_dictionary.ContainsKey(requestUri))
            {
                policy = _dictionary[requestUri];
            }
            else
            {
                policy = PollyPolicies.GetHttpPolicy(new TimeSpan(0, 1, 0));
                _dictionary.TryAdd(requestUri, policy);
            }

            return policy.ExecuteAsync(async () => await this.Client.GetAsync(requestUri));
        }
    }
}