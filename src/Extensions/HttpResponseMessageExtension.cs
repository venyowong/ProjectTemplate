using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectTemplate.Extensions
{
    public static class HttpResponseMessageExtension
    {
        public static async Task<T> ReadAsObj<T>(this HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return default;
            }
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();
            return json.ToObj<T>().Item1;
        }

        public static async Task<string> ReadAsString(this HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return default;
            }
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
