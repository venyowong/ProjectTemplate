using Dapper;
using Polly;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ProjectTemplate.Extensions
{
    public static class DbConnectionExtension
    {
        private static ConcurrentDictionary<string, object> _dictionary = new ConcurrentDictionary<string, object>();
        
        public static async Task<IEnumerable<T>> QueryWithPolly<T>(this IDbConnection cnn, string policyKey, string sql, 
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var key = cnn.ConnectionString + policyKey;
            IAsyncPolicy<IEnumerable<T>> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<IEnumerable<T>>;
            }
            else
            {
                policy = PollyPolicies.GetDbCommandPolicy<IEnumerable<T>>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await cnn.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
            });
        }
    }
}