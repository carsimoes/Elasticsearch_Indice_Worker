using Nest;

namespace Elasticsearch_Indice_Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IndexHandler _indexHandler;
        public IConfiguration _configuration { get; set; }
        
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;

            _indexHandler = new IndexHandler(_logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await _indexHandler.RunAllCleaning();

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
