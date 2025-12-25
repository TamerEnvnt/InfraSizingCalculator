namespace InfraSizingCalculator.Models.Enums;

/// <summary>
/// Server roles for VM deployments
/// </summary>
public enum ServerRole
{
    /// <summary>Web/Frontend servers - Handle HTTP requests</summary>
    Web,

    /// <summary>Application servers - Business logic processing</summary>
    App,

    /// <summary>Database servers - Data persistence</summary>
    Database,

    /// <summary>Cache servers - In-memory caching (Redis, Memcached)</summary>
    Cache,

    /// <summary>Message Queue servers - Async messaging (RabbitMQ, Kafka)</summary>
    MessageQueue,

    /// <summary>Search servers - Full-text search (Elasticsearch, Solr)</summary>
    Search,

    /// <summary>File/Storage servers - File storage and serving</summary>
    Storage,

    /// <summary>Monitoring servers - Metrics, logs, alerting</summary>
    Monitoring,

    /// <summary>Bastion/Jump servers - Secure access point</summary>
    Bastion
}
