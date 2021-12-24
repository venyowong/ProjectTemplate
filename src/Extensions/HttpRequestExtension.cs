using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTemplate.Extensions
{
    public static class HttpRequestExtension
    {
        public static async Task<string> ReadBody(this HttpRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            try
            {
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, false, -1, true))
                {
                    var result = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                    return result;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetAbsoluteUrl(this HttpRequest request) => $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}";
    }
}
