using FileTransfer.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace FileTransfer.Infrastructure.Transport
{
    public class RabbitMqChunkTransport : IChunkTransport, IAsyncDisposable
    {
        private readonly RabbitMqOptions _options;

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqChunkTransport(
            RabbitMqOptions options)
        {
            _options = options;
        }

        private async Task<IChannel> GetChannelAsync(
            CancellationToken cancellationToken)
        {
            if (_channel is not null)
            {
                return _channel;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            _connection = await factory.CreateConnectionAsync(
                cancellationToken);

            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            return _channel;
        }

        public async ValueTask SendChunk(
            FileChunk chunk,
            CancellationToken token)
        {
            var channel = await GetChannelAsync(token);

            var body = JsonSerializer.SerializeToUtf8Bytes(chunk);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.QueueName,
                body: body,
                cancellationToken: token);
        }

        public async IAsyncEnumerable<FileChunk> ReceiveChunk(
            [EnumeratorCancellation] CancellationToken token)
        {
            var rabbitChannel = await GetChannelAsync(token);

            var chunks = Channel.CreateUnbounded<FileChunk>();

            var consumer = new AsyncEventingBasicConsumer(rabbitChannel);

            consumer.ReceivedAsync += async (_, args) =>
            {
                try
                {
                    var chunk = JsonSerializer.Deserialize<FileChunk>(
                        args.Body.Span);

                    if (chunk is null)
                    {
                        await rabbitChannel.BasicNackAsync(
                            args.DeliveryTag,
                            multiple: false,
                            requeue: false);

                        return;
                    }

                    await chunks.Writer.WriteAsync(chunk, token);

                    await rabbitChannel.BasicAckAsync(
                        args.DeliveryTag,
                        multiple: false);
                }
                catch (Exception ex)
                {
                    chunks.Writer.TryComplete(ex);

                    await rabbitChannel.BasicNackAsync(
                        args.DeliveryTag,
                        multiple: false,
                        requeue: true);
                }
            };

            await rabbitChannel.BasicConsumeAsync(
                queue: _options.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: token);

            await foreach (var chunk in chunks.Reader.ReadAllAsync(token))
            {
                yield return chunk;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
