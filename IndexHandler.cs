using Nest;

namespace Elasticsearch_Indice_Worker
{
    public class IndexHandler
    {
        protected InventoryIndice _inventoryIndice;
        protected LocationIndice _locationIndice;
        private readonly ILogger<Worker> _logger;

        public IndexHandler(ILogger<Worker> logger)
        {
            _logger = logger;
            this._inventoryIndice = new InventoryIndice(_logger, GetConfig());
            this._locationIndice = new LocationIndice(_logger, GetConfig());
        }

        public async Task RunAllCleaning()
        {
            await this._inventoryIndice.RunCleaning();
            await this._locationIndice.RunCleaning();
        }

        private string GetConfig()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.json");
            var config = configuration.Build();

            return config["ElasticSearchSettings:Uri"];
        }
    }

    public class IndiceBase
    {
        public string _elasticSearchUri;
    }

    public class InventoryIndice : IndiceBase
    {
        private readonly ILogger<Worker> _logger;

        public InventoryIndice(ILogger<Worker> logger, string elasticSearchUri)
        {
            _elasticSearchUri = elasticSearchUri;
            _logger = logger;
        }

        public async Task RunCleaning()
        {
            var indice = new Index()
            {
                Id = "inventory-worker",
                MaximumAgeInMonths = 6
            };

            try
            {
                var settings = new ConnectionSettings(new Uri(_elasticSearchUri));
                var _elasticClient = new ElasticClient(settings);

                var result = await _elasticClient.Indices.GetAsync(new GetIndexRequest(Indices.All));

                if (result.ApiCall.Success)
                {
                    var months = Enumerable.Range(1, indice.MaximumAgeInMonths).Select(n => DateTime.Now.AddMonths(-n));

                    var indecesToExclude = new List<string>();

                    foreach (var month in months)
                    {
                        string indiceToCheck = GetIndiceName(month, indice);

                        var indiceChecked = result.Indices.ToList().Where(x => x.Key.Name.Contains(indiceToCheck));

                        if (indiceChecked.Any())
                            indecesToExclude.Add(indiceChecked.FirstOrDefault().Key.Name);
                    }

                    foreach (var indiceToExclude in indecesToExclude)
                    {
                        var indicetemp = result.Indices.ToList().Where(x => x.Key.Name.Contains(indiceToExclude));

                        if (indicetemp.Any())
                            _elasticClient.Indices.Delete(indicetemp.FirstOrDefault().Key);
                    }

                    //Sucess
                }
                else
                {
                    _logger.LogInformation("Não foi possível conectar com o Elasticsearch: {message}", 
                                           result.ApiCall.DebugInformation.Substring(0, result.ApiCall.DebugInformation.IndexOf(':')));
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Falha ao exlcuir um ou mais Índices: {e}", e.Message);
                throw;
            }
        }

        private string GetIndiceName(DateTime month, Index indice)
        {
            string monthNormalized = month.Month < 10 ? $"0{month.Month}" : month.Month.ToString();

            return $"{indice.Id}-{month.Year.ToString()}.{monthNormalized}";
        }
    }

    public class LocationIndice : IndiceBase
    {
        private readonly ILogger<Worker> _logger;

        public LocationIndice(ILogger<Worker> logger, string elasticSearchUri)
        {
            _elasticSearchUri = elasticSearchUri;
            _logger = logger;
        }

        public async Task RunCleaning()
        {
            var indice = new Index()
            {
                Id = "location-movements",
                MaximumAgeInMonths = 12,
                TenantId = "1"
            };

            //TODO: prosseguir de acordo com a forma de tratar o TennatID
        }
    }
}
