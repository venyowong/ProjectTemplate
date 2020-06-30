using System;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;
using StackExchange.Redis;

namespace ProjectTemplate
{
    public static class PollyPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> HttpFallBackPolicy
        {
            get => Policy<HttpResponseMessage>.Handle<Exception>().FallbackAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError), d =>
            {
                Log.Warning($"Fallback: {d.Exception.GetType().Name} {d.Exception.Message}");
                return Task.CompletedTask;
            });
        }

        public static IAsyncPolicy<HttpResponseMessage> HttpCircuitBreakerPolicy
        {
            get => HttpPolicyExtensions.HandleTransientHttpError()// HttpRequestException, HTTP 5xx, HTTP 408
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(3, new TimeSpan(0, 0, 30), (d, ts) =>
                {
                    Log.Warning($"Open HTTP Circuit Breaker：{ts.TotalMilliseconds}");
                }, () =>
                {
                    Log.Warning("Closed HTTP Circuit Breaker");
                }, () =>
                {
                    Log.Warning("HalfOpen HTTP Circuit Breaker");
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy
        {
            get => HttpPolicyExtensions.HandleTransientHttpError()// HttpRequestException, HTTP 5xx, HTTP 408
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(new[] 
                {
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(300)
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetHttpTimeoutPolicy(TimeSpan timeSpan) => 
            Policy.TimeoutAsync<HttpResponseMessage>(timeSpan);

        public static IAsyncPolicy<HttpResponseMessage> GetHttpPolicy(TimeSpan timeout) =>
            Policy.WrapAsync(HttpFallBackPolicy, HttpRetryPolicy, HttpCircuitBreakerPolicy, GetHttpTimeoutPolicy(timeout));

        public static IAsyncPolicy<T> GetDbRetryPolicy<T>() => 
            Policy<T>.Handle<DbException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(100) });

        public static IAsyncPolicy<T> GetDbCircuitBreakerPolicy<T>() => 
            Policy<T>.Handle<DbException>()
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(3, new TimeSpan(0, 0, 30), (d, ts) =>
                {
                    Log.Warning($"Open DB Circuit Breaker：{ts.TotalMilliseconds}");
                }, () =>
                {
                    Log.Warning("Closed DB Circuit Breaker");
                }, () =>
                {
                    Log.Warning("HalfOpen DB Circuit Breaker");
                });

        public static IAsyncPolicy<T> GetTimeoutPolicy<T>(TimeSpan timeSpan) => Policy.TimeoutAsync<T>(timeSpan);

        public static IAsyncPolicy<T> GetFallBackPolicy<T>() => 
            Policy<T>.Handle<Exception>().FallbackAsync(default(T), d =>
            {
                Log.Warning($"Fallback: {d.Exception.GetType().Name} {d.Exception.Message}");
                return Task.CompletedTask;
            });

        public static IAsyncPolicy<T> GetDbConnectionPolicy<T>() =>
            Policy.WrapAsync(GetFallBackPolicy<T>(), GetDbRetryPolicy<T>(), GetDbCircuitBreakerPolicy<T>(), GetTimeoutPolicy<T>(TimeSpan.FromMilliseconds(10)));

        public static IAsyncPolicy<T> GetDbCommandPolicy<T>() =>
            Policy.WrapAsync(GetFallBackPolicy<T>(), GetDbCircuitBreakerPolicy<T>(), GetTimeoutPolicy<T>(new TimeSpan(0, 1, 0)));

        public static IAsyncPolicy<T> GetRedisCircuitBreakerPolicy<T>() => 
            Policy<T>.Handle<RedisException>()
                .Or<RedisConnectionException>()
                .Or<RedisCommandException>()
                .Or<RedisServerException>()
                .Or<RedisTimeoutException>()
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(3, new TimeSpan(0, 0, 30), (d, ts) =>
                {
                    Log.Warning($"Open Redis Circuit Breaker：{ts.TotalMilliseconds}");
                }, () =>
                {
                    Log.Warning("Closed Redis Circuit Breaker");
                }, () =>
                {
                    Log.Warning("HalfOpen Redis Circuit Breaker");
                });

        public static IAsyncPolicy<T> GetRedisCommandPolicy<T>() =>
            Policy.WrapAsync(GetFallBackPolicy<T>(), GetRedisCircuitBreakerPolicy<T>(), GetTimeoutPolicy<T>(new TimeSpan(0, 0, 30)));
    }
}