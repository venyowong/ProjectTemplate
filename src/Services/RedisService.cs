using ProjectTemplate.Extensions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectTemplate.Services
{
    public class RedisService
    {
        private IDatabase db;

        public RedisService(IDatabase db)
        {
            this.db = db;
        }

        public async Task<(T, Exception)> GetAsync<T>(string key) where T : class
        {
            #if (Polly)
            var cache = await this.db.StringGetWithPolly(key);
            #else
            var cache = await this.db.StringGetAsync(key);
            #endif
            if (cache.IsNullOrEmpty)
            {
                return default;
            }
            else
            {
                return cache.ToString().ToObj<T>();
            }
        }

        public async Task<(List<T>, Exception)> GetCompressedList<T>(string key)
        {
            #if (Polly)
            var cache = await this.db.StringGetWithPolly(key);
            #else
            var cache = await this.db.StringGetAsync(key);
            #endif
            if (cache.IsNullOrEmpty)
            {
                return default;
            }
            else
            {
                var (json, e) = cache.ToString().GZipDecompress();
                if (e != null)
                {
                    return (default, e);
                }

                return json.ToObj<List<T>>();
            }
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> init, TimeSpan timeSpan = default) where T : class
        {
            #if (Polly)
            var cache = await this.db.StringGetWithPolly(key);
            #else
            var cache = await this.db.StringGetAsync(key);
            #endif
            T result;
            if (cache.IsNullOrEmpty)
            {
                result = await init();
                if (result == default)
                {
                    return result;
                }

                if (timeSpan == default)
                {
                    var (json, e) = result.ToJson();
                    if (e == null)
                    {
                        #if (Polly)
                        await this.db.StringSetWithPolly(key, json);
                        #else
                        await this.db.StringSetAsync(key, json);
                        #endif
                    }
                }
                else
                {
                    var (json, e) = result.ToJson();
                    if (e == null)
                    {
                        #if (Polly)
                        await this.db.StringSetWithPolly(key, json, timeSpan);
                        #else
                        this.db.StringSet(key, json, timeSpan);
                        #endif
                    }
                }
            }
            else
            {
                (result, _) = cache.ToString().ToObj<T>();
            }

            return result;
        }

        public async Task<bool> SetAsync(string key, object value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
            {
                return false;
            }

            var(json, e) = value.ToJson();
            if (e != null)
            {
                return false;
            }

            #if (Polly)
            return await this.db.StringSetWithPolly(key, json, expiry);
            #else
            return await this.db.StringSetAsync(key, json, expiry);
            #endif
        }

        public async Task<bool> SetCompressedList<T>(string key, List<T> list, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key) || list == null)
            {
                return false;
            }

            var (json, e) = list.ToJson();
            if (e != null)
            {
                return false;
            }

            string value;
            (value, e) = json.GZipCompress();
            if (e != null)
            {
                return false;
            }

            #if (Polly)
            return await this.db.StringSetWithPolly(key, json, expiry);
            #else
            return await this.db.StringSetAsync(key, json, expiry);
            #endif
        }

        public async Task<bool> HashSet(RedisKey redisKey, RedisValue hashField, object value)
        {
            var (json, e) = value.ToJson();
            if (e != null)
            {
                return false;
            }

            #if (Polly)
            return await this.db.HashSetWithPolly(redisKey, hashField, json);
            #else
            return await this.db.HashSetAsync(redisKey, hashField, json);
            #endif
        }

        public async Task<bool> HashSet<T>(RedisKey key, IEnumerable<T> list, Func<T, RedisValue> nameSelector)
        {
            #if (Polly)
            return await this.db.HashSetWithPolly(key, list.Select(x =>
            {
                var (json, _) = x.ToJson();
                return new HashEntry(nameSelector(x), json);
            }).Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToArray());
            #else
            await this.db.HashSetAsync(key, list.Select(x =>
            {
                var (json, _) = x.ToJson();
                return new HashEntry(nameSelector(x), json);
            }).Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToArray());
            return true;
            #endif
        }

        public async Task<bool> HashSet(RedisKey key, HashEntry[] hashEntries)
        {
            #if (Polly)
            return await this.db.HashSetWithPolly(key, hashEntries);
            #else
            await this.db.HashSetAsync(key, hashEntries);
            return true;
            #endif
        }

        public async Task<bool> HashSetCompressedList<T>(RedisKey redisKey, RedisValue hashField, List<T> list)
        {
            var (json, e) = list.ToJson();
            if (e != null)
            {
                return false;
            }

            string value;
            (value, e) = json.GZipCompress();
            if (e != null)
            {
                return false;
            }

            #if (Polly)
            return await this.db.HashSetWithPolly(redisKey, hashField, value);
            #else
            return await this.db.HashSetAsync(redisKey, hashField, value);
            #endif
        }

        public async Task<(List<T>, Exception)> HashGetCompressedList<T>(RedisKey redisKey, RedisValue hashField)
        {
            #if (Polly)
            var cache = await this.db.HashGetWithPolly(redisKey, hashField);
            #else
            var cache = await this.db.HashGetAsync(redisKey, hashField);
            #endif
            if (cache.IsNullOrEmpty)
            {
                return default;
            }
            else
            {
                var (json, e) = cache.ToString().GZipDecompress();
                if (e != null)
                {
                    return (default, e);
                }

                return json.ToObj<List<T>>();
            }
        }

        public async Task<(T, Exception)> HashGet<T>(RedisKey redisKey, RedisValue hashField)
        {
            #if (Polly)
            var cache = await this.db.HashGetWithPolly(redisKey, hashField);
            #else
            var cache = await this.db.HashGetAsync(redisKey, hashField);
            #endif
            if (cache.IsNullOrEmpty)
            {
                return default;
            }
            else
            {
                return cache.ToString().ToObj<T>();
            }
        }

        public async Task<Dictionary<string, List<T>>> HashGetAllCompressedList<T>(RedisKey redisKey)
        {
            #if (Polly)
            var values = await this.db.HashGetAllWithPolly(redisKey);
            #else
            var values = await this.db.HashGetAllAsync(redisKey);
            #endif
            if (values.IsNullOrEmpty())
            {
                return new Dictionary<string, List<T>>();
            }

            return values.ToDictionary(x => x.Name.ToString(), x =>
            {
                var (json, e) = x.Value.ToString().GZipDecompress();
                if (e != null)
                {
                    return default;
                }

                List<T> result;
                (result, e) = json.ToObj<List<T>>();
                if (e != null)
                {
                    return default;
                }

                return result;
            });
        }

        public async Task<Dictionary<string, T>> HashGetAll<T>(RedisKey redisKey)
        {
            #if (Polly)
            var values = await this.db.HashGetAllWithPolly(redisKey);
            #else
            var values = await this.db.HashGetAllAsync(redisKey);
            #endif
            if (values.IsNullOrEmpty())
            {
                return new Dictionary<string, T>();
            }

            return values.ToDictionary(x => x.Name.ToString(), x =>
            {
                var (result, e) = x.Value.ToString().ToObj<T>();
                if (e != null)
                {
                    return default;
                }

                return result;
            });
        }

        public async Task<long> HashDelete(RedisKey redisKey, RedisValue[] hashFields)
        {
            #if (Polly)
            return await this.db.HashDeleteWithPolly(redisKey, hashFields);
            #else
            return await this.db.HashDeleteAsync(redisKey, hashFields);
            #endif
        }

        public async Task<bool> HashDelete(RedisKey key, RedisValue hashField)
        {
            #if (Polly)
            return await this.db.HashDeleteWithPolly(key, hashField);
            #else
            return await this.db.HashDeleteAsync(key, hashField);
            #endif
        }

        public async Task<(RedisValue, Exception)> Publish(RedisKey redisKey, object message, int? maxLength = null)
        {
            var (msg, e) = message.ToJson();
            if (e != null)
            {
                return (default, e);
            }

            #if (Polly)
            return (await this.db.StreamAddWithPolly(redisKey, "message", msg, null, maxLength), default);
            #else
            return (await this.db.StreamAddAsync(redisKey, "message", msg, null, maxLength), default);
            #endif
        }

        public async Task<Dictionary<string, T>> Consume<T>(RedisKey redisKey, RedisValue position, int? count = null)
        {
            #if (Polly)
            var messages = await this.db.StreamReadWithPolly(redisKey, position, count);
            #else
            var messages = await this.db.StreamReadAsync(redisKey, position, count);
            #endif
            if (messages.IsNullOrEmpty())
            {
                return new Dictionary<string, T>();
            }

            var result = new Dictionary<string, T>();
            foreach (var msg in messages)
            {
                if (msg.IsNull)
                {
                    continue;
                }

                var id = msg.Id.ToString();
                var value = msg.Values?.FirstOrDefault(x => x.Name == "message");
                if (value == null)
                {
                    continue;
                }

                var (obj, e) = value.Value.Value.ToString().ToObj<T>();
                if (e != null)
                {
                    continue;
                }

                result.Add(id, obj);
            }
            return result;
        }

        public async Task<RedisValue[]> ZSetGetAll(RedisKey redisKey, Order order = Order.Ascending)
        {
            #if (Polly)
            return await this.db.SortedSetRangeByRankWithPolly(redisKey, order: order);
            #else
            return await this.db.SortedSetRangeByRankAsync(redisKey, order: order);
            #endif
        }

        public async Task<RedisValue> ZSetGetLast(RedisKey redisKey)
        {
            #if (Polly)
            var values = await this.db.SortedSetRangeByRankWithPolly(redisKey, -1, -1);
            #else
            var values = await this.db.SortedSetRangeByRankAsync(redisKey, -1, -1);
            #endif
            if (values.IsNullOrEmpty())
            {
                return default;
            }

            return values.First();
        }

        public async Task<bool> KeyDelete(RedisKey key)
        {
            #if (Polly)
            return await this.db.KeyDeleteWithPolly(key);
            #else
            return await this.db.KeyDeleteAsync(key);
            #endif
        }
    }
}
