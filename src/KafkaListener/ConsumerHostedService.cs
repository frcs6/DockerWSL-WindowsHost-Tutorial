using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaListener
{
    public class ConsumerHostedService : BackgroundService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly HttpClient _httpClient;
        private readonly IConsumer<string, string> _consumer;
        private readonly ListenerConfig _listenerConfig;

        public ConsumerHostedService(
            ILogger<ConsumerHostedService> logger,
            IHostApplicationLifetime applicationLifetime,
            IHttpClientFactory httpClientFactory,
            IOptions<ConsumerConfig> consumerConfig,
            IOptions<ListenerConfig> listenerConfig)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _listenerConfig = listenerConfig.Value;

            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_listenerConfig.Host);

            _consumer = new ConsumerBuilder<string, string>(consumerConfig.Value).Build();
            _consumer.Subscribe(listenerConfig.Value.Topic);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _applicationLifetime.ApplicationStopping);
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationTokenSource.Token);

                    if (consumeResult != null)
                    {
                        _logger.LogInformation("Received message on topic '{topic}'", consumeResult.Topic);

                        try
                        {
                            _logger.LogInformation("Send message to '{route}'", _listenerConfig.Route);

                            using var httpContent = new StringContent(consumeResult.Message.Value.ToString(), Encoding.UTF8);
                            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            var result = await _httpClient.PostAsync(_listenerConfig.Route, httpContent, cancellationTokenSource.Token);

                            if (!result.IsSuccessStatusCode)
                            {
                                _logger.LogError(result.ReasonPhrase);
                                throw new Exception(result.ReasonPhrase);
                            }
                            else
                                _logger.LogInformation("Message sended to '{route}'", _listenerConfig.Route);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                        }
                    }
                }
                catch (ConsumeException ex)
                {
                    if (ex.Error.IsFatal)
                    {
                        _logger.LogCritical(ex, ex.Error.Reason);
                        _logger.LogCritical("Stopping the application");
                        _applicationLifetime.StopApplication();
                        break;
                    }

                    _logger.LogError(ex, ex.Error.Reason);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                    _logger.LogCritical("Stopping the background service");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
