using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectTemplate.Services;
#if (RabbitMQ)
using RabbitMQ.Client;
#endif
using System.Collections.Generic;

namespace ProjectTemplate.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// 为父类型和子类型同时添加服务实例
        /// <para>避免通过父类注入的时候，无法通过子类类型查找子类对象，反之亦然</para>
        /// <para>可以避免通过父类注入时，通过 GetServices 获取所有对象再查找子类时导致的内存浪费</para>
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransientBothTypes<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TService, TImplementation>();
            services.AddTransient<TImplementation>();
            return services;
        }

#if (RabbitMQ)
        /// <summary>
        /// 添加 RabbitmqService
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitmq(this IServiceCollection services)
        {
            return services.AddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetService<IConfiguration>();
                var factory = new ConnectionFactory();
                factory.UserName = config["RabbitMQ:UserName"];
                factory.Password = config["RabbitMQ:Password"];
                factory.VirtualHost = config["RabbitMQ:VirtualHost"];
                var endpoints = new List<AmqpTcpEndpoint>();
                config.GetValue<List<string>>("RabbitMQ:Hosts").ForEach(host =>
                {
                    endpoints.Add(new AmqpTcpEndpoint(host));
                });
                return new RabbitMQService(factory, endpoints);
            });
        }
#endif
    }
}