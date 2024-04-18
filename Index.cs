namespace Elasticsearch_Indice_Worker
{
    public class Index
    {
        //public const string Name = "ElasticSearchSettings:Index";

        public string Id { get; set; }
        public int MaximumAgeInMonths { get; set; }
        public string TenantId { get; set; }
    }
}
