using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Polly;
using StackExchange.Redis;

namespace ProjectTemplate.Extensions
{
    public static class RedisDbExtension
    {
        private static ConcurrentDictionary<string, object> _dictionary = new ConcurrentDictionary<string, object>();

        public static async Task<bool> StringSetWithPolly(this IDatabase db, RedisKey redisKey, RedisValue value,
            TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}StringSet";
            IAsyncPolicy<bool> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<bool>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<bool>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.StringSetAsync(redisKey, value, expiry, when, flags);
            });
        }

        public static async Task<RedisValue> StringGetWithPolly(this IDatabase db, RedisKey redisKey, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}StringGet.RedisValue";
            IAsyncPolicy<RedisValue> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<RedisValue>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<RedisValue>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.StringGetAsync(redisKey, flags);
            });
        }

        public static async Task<bool> HashSetWithPolly(this IDatabase db, RedisKey redisKey, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}HashSet";
            IAsyncPolicy<bool> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<bool>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<bool>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.HashSetAsync(redisKey, hashField, value, when, flags);
            });
        }

        public static async Task<bool> HashSetWithPolly(this IDatabase db, RedisKey redisKey, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}HashSet.HashEntry[]";
            IAsyncPolicy<bool> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<bool>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<bool>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                await db.HashSetAsync(redisKey, hashFields, flags);
                return true;
            });
        }

        public static async Task<HashEntry[]> HashGetAllWithPolly(this IDatabase db, RedisKey redisKey, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}HashGetAll";
            IAsyncPolicy<HashEntry[]> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<HashEntry[]>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<HashEntry[]>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.HashGetAllAsync(redisKey, flags);
            });
        }

        public static async Task<RedisValue> HashGetWithPolly(this IDatabase db, RedisKey redisKey, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}HashGet.RedisValue";
            IAsyncPolicy<RedisValue> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<RedisValue>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<RedisValue>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.HashGetAsync(redisKey, hashField, flags);
            });
        }

        public static async Task<long> HashDeleteWithPolly(this IDatabase db, RedisKey redisKey, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}HashDelete.RedisValue[]";
            IAsyncPolicy<long> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<long>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<long>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.HashDeleteAsync(redisKey, hashFields, flags);
            });
        }

        public static async Task<bool> HashDeleteWithPolly(this IDatabase db, RedisKey redisKey, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}HashDelete.RedisValue";
            IAsyncPolicy<bool> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<bool>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<bool>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.HashDeleteAsync(redisKey, hashField, flags);
            });
        }

        public static async Task<RedisValue> StreamAddWithPolly(this IDatabase db, RedisKey redisKey, RedisValue streamField, RedisValue streamValue,
            RedisValue? messageId = null, int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}StreamAdd.RedisValue";
            IAsyncPolicy<RedisValue> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<RedisValue>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<RedisValue>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.StreamAddAsync(redisKey, streamField, streamValue, messageId, maxLength, useApproximateMaxLength, flags);
            });
        }

        public static async Task<StreamEntry[]> StreamReadWithPolly(this IDatabase db, RedisKey redisKey, RedisValue position,
            int? count = null, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}StreamRead.RedisValue";
            IAsyncPolicy<StreamEntry[]> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<StreamEntry[]>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<StreamEntry[]>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.StreamReadAsync(redisKey, position, count, flags);
            });
        }

        public static async Task<RedisValue[]> SortedSetRangeByRankWithPolly(this IDatabase db, RedisKey redisKey, long start = 0, long stop = -1,
            Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}SortedSetRangeByRank.RedisValue[]";
            IAsyncPolicy<RedisValue[]> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<RedisValue[]>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<RedisValue[]>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.SortedSetRangeByRankAsync(redisKey, start, stop, order, flags);
            });
        }

        public static async Task<bool> KeyDeleteWithPolly(this IDatabase db, RedisKey redisKey, CommandFlags flags = CommandFlags.None)
        {
            var key = $"{db.Database}KeyDelete.RedisKey";
            IAsyncPolicy<bool> policy = null;
            if (_dictionary.ContainsKey(key))
            {
                policy = _dictionary[key] as IAsyncPolicy<bool>;
            }
            else
            {
                policy = PollyPolicies.GetRedisCommandPolicy<bool>();
                _dictionary.TryAdd(key, policy);
            }

            return await policy.ExecuteAsync(async () =>
            {
                return await db.KeyDeleteAsync(redisKey, flags);
            });
        }
    }
}