using ProjectTemplate.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ProjectTemplate.Services
{
    public class RabbitMQService : IDisposable
    {
        private IConnection connection;
        private IModel publishChannel;
        private IModel consumeChannel;
        private ConnectionFactory connectionFactory;
        private List<AmqpTcpEndpoint> endpoints;
        private ConcurrentDictionary<string, Func<string, bool>> consumers =
            new ConcurrentDictionary<string, Func<string, bool>>();
        private ConcurrentDictionary<ExchangeQueue, Func<string, bool>> exchangeConsumers =
            new ConcurrentDictionary<ExchangeQueue, Func<string, bool>>();

        public RabbitMQService(ConnectionFactory connectionFactory, List<AmqpTcpEndpoint> endpoints)
        {
            this.connectionFactory = connectionFactory;
            this.endpoints = endpoints;
            this.Connect();
        }

        public void Publish(string queue, string message)
        {
            if (string.IsNullOrWhiteSpace(queue) || string.IsNullOrWhiteSpace(message) ||
                this.publishChannel == null || this.publishChannel.IsClosed)
            {
                return;
            }

            this.publishChannel.QueueDeclare(queue, true, false, false, null);
            var body = Encoding.UTF8.GetBytes(message);
            var properties = this.publishChannel.CreateBasicProperties();
            properties.Persistent = true;
            this.publishChannel.BasicPublish(string.Empty, queue, properties, body);
        }

        public void PublishExchange(string exchange, string type, string routeKey, params string[] messages)
        {
            if (string.IsNullOrWhiteSpace(exchange) || string.IsNullOrWhiteSpace(type) || messages.IsNullOrEmpty() ||
                this.publishChannel == null || this.publishChannel.IsClosed)
            {
                return;
            }

            this.publishChannel.ExchangeDeclare(exchange, type, true, false);
            var properties = this.publishChannel.CreateBasicProperties();
            properties.Persistent = true;
            foreach (var message in messages)
            {
                var body = Encoding.UTF8.GetBytes(message);
                this.publishChannel.BasicPublish(exchange, routeKey, properties, body);
            }
        }

        public void Consume(string queue, Func<string, bool> consume)
        {
            if (string.IsNullOrWhiteSpace(queue) || consume == null)
            {
                return;
            }

            this.consumers.TryAdd(queue, consume);
            this.ConsumeQueue(queue, consume);
        }

        public void ConsumeExchange(string exchange, string type, Func<string, bool> consume,
            string queue = "", string routeKey = "")
        {
            if (string.IsNullOrWhiteSpace(exchange) || string.IsNullOrWhiteSpace(type) || consume == null)
            {
                return;
            }

            var eq = new ExchangeQueue
            {
                Exchange = exchange,
                Type = type,
                Queue = queue,
                RouteKey = routeKey
            };
            this.exchangeConsumers.TryAdd(eq, consume);
            this.ConsumeExchange(eq, consume);
        }

        public uint GetMessageCount(string queue)
        {
            return this.consumeChannel.MessageCount(queue);
        }

        private void Connect()
        {
            if (this.connection != null && this.connection.IsOpen && this.consumeChannel != null &&
                 this.consumeChannel.IsOpen && this.publishChannel != null && this.publishChannel.IsOpen)
            {
                return;
            }

            var success = false;
            while (!success)
            {
                try
                {
                    this.connection = this.connectionFactory.CreateConnection(this.endpoints);
                    this.publishChannel = this.connection.CreateModel();
                    this.consumeChannel = this.connection.CreateModel();
                    this.connection.ConnectionShutdown += this.OnShutDown;
                    this.publishChannel.ModelShutdown += this.OnShutDown;
                    this.consumeChannel.ModelShutdown += this.OnShutDown;
                    if (this.consumers.Count > 0)
                    {
                        foreach (var pair in this.consumers)
                        {
                            this.ConsumeQueue(pair.Key, pair.Value);
                        }
                    }
                    if (this.exchangeConsumers.Count > 0)
                    {
                        foreach (var pair in this.exchangeConsumers)
                        {
                            this.ConsumeExchange(pair.Key, pair.Value);
                        }
                    }
                    success = true;
                    Log.Information("RabbitMQService 连接成功");
                }
                catch (Exception e)
                {
                    Log.Error(e, "RabbitMQService.Connect");
                    Thread.Sleep(3000); // 3 秒钟尝试一次重连
                    Log.Information("尝试重连 RabbitMQ");
                }
            }
        }

        private void OnShutDown(object sender, ShutdownEventArgs e)
        {
            Log.Warning($"RabbitMQService.OnShutDown {e}");
            Thread.Sleep(3000); // 3 秒钟尝试一次重连
            Log.Information("尝试重连 RabbitMQ");
            Connect();
        }

        private void ConsumeQueue(string queue, Func<string, bool> consume)
        {
            this.consumeChannel.QueueDeclare(queue, true, false, false, null);
            var consumer = new EventingBasicConsumer(this.consumeChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                if (consume(message))
                {
                    this.consumeChannel.BasicAck(ea.DeliveryTag, false);
                }
            };
            consumer.ConsumerCancelled += (sender, ea) =>
            {
                Log.Warning($"consumer cancelled, queue: {queue}, ea: {ea.ToJson()}");
            };
            consumer.Shutdown += (sender, ea) =>
            {
                Log.Warning($"consumer Shutdown, queue: {queue}, ea: {ea.ToJson()}");
            };
            consumer.Unregistered += (sender, ea) =>
            {
                Log.Warning($"consumer Unregistered, queue: {queue}, ea: {ea.ToJson()}");
            };
            this.consumeChannel.BasicConsume(queue, false, consumer);
        }

        private void ConsumeExchange(ExchangeQueue eq, Func<string, bool> consume)
        {
            this.consumeChannel.ExchangeDeclare(eq.Exchange, eq.Type, true, false);
            var queueName = eq.Queue;
            if (string.IsNullOrWhiteSpace(queueName))
            {
                queueName = this.consumeChannel.QueueDeclare().QueueName;
            }
            this.consumeChannel.QueueBind(queueName, eq.Exchange, eq.RouteKey);

            var consumer = new EventingBasicConsumer(this.consumeChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                if (consume(message))
                {
                    this.consumeChannel.BasicAck(ea.DeliveryTag, false);
                }
            };
            consumer.ConsumerCancelled += (sender, ea) =>
            {
                Log.Warning($"consumer cancelled, queue: {queueName}, ea: {ea.ToJson()}");
            };
            consumer.Shutdown += (sender, ea) =>
            {
                Log.Warning($"consumer Shutdown, queue: {queueName}, ea: {ea.ToJson()}");
            };
            consumer.Unregistered += (sender, ea) =>
            {
                Log.Warning($"consumer Unregistered, queue: {queueName}, ea: {ea.ToJson()}");
            };
            this.consumeChannel.BasicConsume(queueName, false, consumer);
        }

        public void Dispose()
        {
            Log.Information("RabbitMQService.Dispose");
            try
            {
                this.consumeChannel.Dispose();
                this.publishChannel.Dispose();
                this.connection.Dispose();
            }
            catch { }
        }

        public class ExchangeQueue
        {
            public string Exchange { get; set; }

            public string Type { get; set; }

            public string Queue { get; set; } = string.Empty;

            public string RouteKey { get; set; } = string.Empty;

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (obj is ExchangeQueue eq)
                {
                    return this.Exchange == eq.Exchange && this.Type == eq.Type && this.Queue == eq.Queue && this.RouteKey == eq.RouteKey;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode() => $"{this.Exchange}.{this.Type}.{this.Queue}.{this.RouteKey}".GetHashCode();
        }
    }
}
