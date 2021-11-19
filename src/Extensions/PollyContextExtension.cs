using Polly;

namespace ProjectTemplate.Extensions
{
    public static class PollyContextExtension
    {
        public static string GetDbParameters(this Context context)
        {
            if (context == null)
            {
                return string.Empty;
            }

            var callerMemberName = string.Empty;
            if (context.ContainsKey("callerMemberName"))
            {
                callerMemberName = context["callerMemberName"].ToString();
            }
            var sql = string.Empty;
            if (context.ContainsKey("sql"))
            {
                sql = context["sql"].ToString();
            }
            var param = string.Empty;
            if (context.ContainsKey("param"))
            {
                param = context["param"].ToString();
            }
            return $"callerMemberName: {callerMemberName}, sql: {sql}, param: {param}";
        }
    }
}
