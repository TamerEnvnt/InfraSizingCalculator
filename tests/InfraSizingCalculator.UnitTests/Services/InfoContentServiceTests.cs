using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Entities;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.UnitTests.Services;

/// <summary>
/// Comprehensive unit tests for InfoContentService.
/// Tests database-first retrieval, caching, and fallback behavior.
/// </summary>
public class InfoContentServiceTests : IDisposable
{
    private readonly InfraSizingDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<InfoContentService> _logger;
    private readonly InfoContentService _service;

    public InfoContentServiceTests()
    {
        var options = new DbContextOptionsBuilder<InfraSizingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new InfraSizingDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = Substitute.For<ILogger<InfoContentService>>();
        _service = new InfoContentService(_dbContext, _cache, _logger);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _cache.Dispose();
    }

    #region GetInfoTypeContentAsync Tests

    [Fact]
    public async Task GetInfoTypeContentAsync_ReturnsCachedContent_WhenInCache()
    {
        // Arrange
        var cachedContent = new InfoContent("Cached Title", "<p>Cached content</p>") { IsFromDatabase = true };
        _cache.Set("info_type_Platform", cachedContent);

        // Act
        var result = await _service.GetInfoTypeContentAsync("Platform");

        // Assert
        Assert.Equal("Cached Title", result.Title);
        Assert.Equal("<p>Cached content</p>", result.ContentHtml);
        Assert.True(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetInfoTypeContentAsync_ReturnsDbContent_WhenInDatabase()
    {
        // Arrange
        _dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
        {
            Id = 1,
            InfoTypeKey = "Platform",
            Title = "DB Platform Types",
            ContentHtml = "<p>Content from database</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetInfoTypeContentAsync("Platform");

        // Assert
        Assert.Equal("DB Platform Types", result.Title);
        Assert.Equal("<p>Content from database</p>", result.ContentHtml);
        Assert.True(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetInfoTypeContentAsync_ReturnsDefault_WhenNotInDatabaseOrCache()
    {
        // Arrange - no data in database or cache

        // Act
        var result = await _service.GetInfoTypeContentAsync("Platform");

        // Assert
        Assert.Equal("Platform Types", result.Title);
        Assert.Contains("Native Applications", result.ContentHtml);
        Assert.False(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetInfoTypeContentAsync_ReturnsDefault_WhenDbContentIsInactive()
    {
        // Arrange
        _dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
        {
            Id = 1,
            InfoTypeKey = "Platform",
            Title = "Inactive Content",
            ContentHtml = "<p>Inactive content</p>",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetInfoTypeContentAsync("Platform");

        // Assert
        Assert.Equal("Platform Types", result.Title);
        Assert.False(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetInfoTypeContentAsync_ReturnsDefault_WhenDbContentHtmlIsEmpty()
    {
        // Arrange
        _dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
        {
            Id = 1,
            InfoTypeKey = "Platform",
            Title = "Empty Content",
            ContentHtml = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetInfoTypeContentAsync("Platform");

        // Assert
        Assert.Equal("Platform Types", result.Title);
        Assert.False(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetInfoTypeContentAsync_CachesDbContent()
    {
        // Arrange
        _dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
        {
            Id = 1,
            InfoTypeKey = "Platform",
            Title = "Cached DB Content",
            ContentHtml = "<p>Will be cached</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.GetInfoTypeContentAsync("Platform");

        // Assert - verify it was cached
        Assert.True(_cache.TryGetValue("info_type_Platform", out InfoContent? cached));
        Assert.NotNull(cached);
        Assert.Equal("Cached DB Content", cached!.Title);
    }

    [Theory]
    [InlineData("Platform")]
    [InlineData("Deployment")]
    [InlineData("Technology")]
    [InlineData("Distribution")]
    [InlineData("ClusterMode")]
    [InlineData("NodeSpecs")]
    [InlineData("AppConfig")]
    public async Task GetInfoTypeContentAsync_ReturnsDefaultForAllKnownTypes(string infoType)
    {
        // Act
        var result = await _service.GetInfoTypeContentAsync(infoType);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Title);
        Assert.NotEmpty(result.ContentHtml);
        Assert.False(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetInfoTypeContentAsync_ReturnsEmpty_ForUnknownType()
    {
        // Act
        var result = await _service.GetInfoTypeContentAsync("UnknownType");

        // Assert
        Assert.Equal("Information", result.Title);
        Assert.Empty(result.ContentHtml);
        Assert.False(result.IsFromDatabase);
    }

    #endregion

    #region GetDistributionInfoAsync Tests

    [Fact]
    public async Task GetDistributionInfoAsync_ReturnsCachedContent_WhenInCache()
    {
        // Arrange
        var cachedContent = new InfoContent("Cached OpenShift", "<p>Cached</p>") { IsFromDatabase = true };
        _cache.Set("distro_info_OpenShift", cachedContent);

        // Act
        var result = await _service.GetDistributionInfoAsync(Distribution.OpenShift);

        // Assert
        Assert.Equal("Cached OpenShift", result.Title);
        Assert.True(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetDistributionInfoAsync_ReturnsDbContent_WhenInDatabase()
    {
        // Arrange
        _dbContext.DistributionConfigs.Add(new DistributionConfigEntity
        {
            Id = 1,
            DistributionKey = "OpenShift",
            Name = "DB OpenShift",
            DetailedInfoHtml = "<p>OpenShift from database</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetDistributionInfoAsync(Distribution.OpenShift);

        // Assert
        Assert.Equal("DB OpenShift", result.Title);
        Assert.Equal("<p>OpenShift from database</p>", result.ContentHtml);
        Assert.True(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetDistributionInfoAsync_ReturnsDefault_WhenNotInDatabaseOrCache()
    {
        // Act
        var result = await _service.GetDistributionInfoAsync(Distribution.OpenShift);

        // Assert
        Assert.Equal("Red Hat OpenShift (On-Prem)", result.Title);
        Assert.Contains("Red Hat (IBM)", result.ContentHtml);
        Assert.False(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetDistributionInfoAsync_ReturnsDefault_WhenDbContentIsInactive()
    {
        // Arrange
        _dbContext.DistributionConfigs.Add(new DistributionConfigEntity
        {
            Id = 1,
            DistributionKey = "OpenShift",
            Name = "Inactive OpenShift",
            DetailedInfoHtml = "<p>Inactive</p>",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetDistributionInfoAsync(Distribution.OpenShift);

        // Assert
        Assert.Contains("Red Hat (IBM)", result.ContentHtml);
        Assert.False(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetDistributionInfoAsync_ReturnsDefault_WhenDetailedInfoHtmlIsEmpty()
    {
        // Arrange
        _dbContext.DistributionConfigs.Add(new DistributionConfigEntity
        {
            Id = 1,
            DistributionKey = "OpenShift",
            Name = "Empty OpenShift",
            DetailedInfoHtml = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetDistributionInfoAsync(Distribution.OpenShift);

        // Assert
        Assert.False(result.IsFromDatabase);
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.OpenShiftROSA)]
    [InlineData(Distribution.OpenShiftARO)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RancherHosted)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.MicroK8s)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Tanzu)]
    [InlineData(Distribution.TanzuCloud)]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    [InlineData(Distribution.OKE)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.OpenShiftDedicated)]
    [InlineData(Distribution.OpenShiftIBM)]
    public async Task GetDistributionInfoAsync_ReturnsDefaultForAllDistributions(Distribution distribution)
    {
        // Act
        var result = await _service.GetDistributionInfoAsync(distribution);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Title);
        Assert.NotEmpty(result.ContentHtml);
    }

    [Fact]
    public async Task GetDistributionInfoAsync_CachesDbContent()
    {
        // Arrange
        _dbContext.DistributionConfigs.Add(new DistributionConfigEntity
        {
            Id = 1,
            DistributionKey = "EKS",
            Name = "AWS EKS",
            DetailedInfoHtml = "<p>EKS content</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.GetDistributionInfoAsync(Distribution.EKS);

        // Assert
        Assert.True(_cache.TryGetValue("distro_info_EKS", out InfoContent? cached));
        Assert.NotNull(cached);
        Assert.Equal("AWS EKS", cached!.Title);
    }

    #endregion

    #region GetTechnologyInfoAsync Tests

    [Fact]
    public async Task GetTechnologyInfoAsync_ReturnsCachedContent_WhenInCache()
    {
        // Arrange
        var cachedContent = new InfoContent("Cached .NET", "<p>Cached</p>") { IsFromDatabase = true };
        _cache.Set("tech_info_DotNet", cachedContent);

        // Act
        var result = await _service.GetTechnologyInfoAsync(Technology.DotNet);

        // Assert
        Assert.Equal("Cached .NET", result.Title);
        Assert.True(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetTechnologyInfoAsync_ReturnsDbContent_WhenInDatabase()
    {
        // Arrange
        _dbContext.TechnologyConfigs.Add(new TechnologyConfigEntity
        {
            Id = 1,
            TechnologyKey = "DotNet",
            Name = "DB .NET",
            DetailedInfoHtml = "<p>.NET from database</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetTechnologyInfoAsync(Technology.DotNet);

        // Assert
        Assert.Equal("DB .NET", result.Title);
        Assert.Equal("<p>.NET from database</p>", result.ContentHtml);
        Assert.True(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetTechnologyInfoAsync_ReturnsDefault_WhenNotInDatabaseOrCache()
    {
        // Act
        var result = await _service.GetTechnologyInfoAsync(Technology.DotNet);

        // Assert
        Assert.Equal(".NET", result.Title);
        Assert.Contains("Microsoft", result.ContentHtml);
        Assert.False(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetTechnologyInfoAsync_ReturnsDefault_WhenDbContentIsInactive()
    {
        // Arrange
        _dbContext.TechnologyConfigs.Add(new TechnologyConfigEntity
        {
            Id = 1,
            TechnologyKey = "DotNet",
            Name = "Inactive .NET",
            DetailedInfoHtml = "<p>Inactive</p>",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetTechnologyInfoAsync(Technology.DotNet);

        // Assert
        Assert.Contains("Microsoft", result.ContentHtml);
        Assert.False(result.IsFromDatabase);
    }

    [Fact]
    public async Task GetTechnologyInfoAsync_ReturnsDefault_WhenDetailedInfoHtmlIsEmpty()
    {
        // Arrange
        _dbContext.TechnologyConfigs.Add(new TechnologyConfigEntity
        {
            Id = 1,
            TechnologyKey = "DotNet",
            Name = "Empty .NET",
            DetailedInfoHtml = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetTechnologyInfoAsync(Technology.DotNet);

        // Assert
        Assert.False(result.IsFromDatabase);
    }

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Go)]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public async Task GetTechnologyInfoAsync_ReturnsDefaultForAllTechnologies(Technology technology)
    {
        // Act
        var result = await _service.GetTechnologyInfoAsync(technology);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Title);
        Assert.NotEmpty(result.ContentHtml);
    }

    [Fact]
    public async Task GetTechnologyInfoAsync_CachesDbContent()
    {
        // Arrange
        _dbContext.TechnologyConfigs.Add(new TechnologyConfigEntity
        {
            Id = 1,
            TechnologyKey = "Java",
            Name = "Java Tech",
            DetailedInfoHtml = "<p>Java content</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.GetTechnologyInfoAsync(Technology.Java);

        // Assert
        Assert.True(_cache.TryGetValue("tech_info_Java", out InfoContent? cached));
        Assert.NotNull(cached);
        Assert.Equal("Java Tech", cached!.Title);
    }

    #endregion

    #region HasCustomContentAsync Tests

    [Fact]
    public async Task HasCustomContentAsync_ReturnsTrue_WhenInfoTypeContentExists()
    {
        // Arrange
        _dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
        {
            Id = 1,
            InfoTypeKey = "Platform",
            Title = "Platform",
            ContentHtml = "<p>Content</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasCustomContentAsync("Platform");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasCustomContentAsync_ReturnsTrue_WhenDistributionContentExists()
    {
        // Arrange
        _dbContext.DistributionConfigs.Add(new DistributionConfigEntity
        {
            Id = 1,
            DistributionKey = "OpenShift",
            Name = "OpenShift",
            DetailedInfoHtml = "<p>Content</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasCustomContentAsync("OpenShift");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasCustomContentAsync_ReturnsTrue_WhenTechnologyContentExists()
    {
        // Arrange
        _dbContext.TechnologyConfigs.Add(new TechnologyConfigEntity
        {
            Id = 1,
            TechnologyKey = "DotNet",
            Name = ".NET",
            DetailedInfoHtml = "<p>Content</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasCustomContentAsync("DotNet");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasCustomContentAsync_ReturnsFalse_WhenNoContentExists()
    {
        // Act
        var result = await _service.HasCustomContentAsync("NonExistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasCustomContentAsync_ReturnsFalse_WhenContentIsInactive()
    {
        // Arrange
        _dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
        {
            Id = 1,
            InfoTypeKey = "Platform",
            Title = "Platform",
            ContentHtml = "<p>Content</p>",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasCustomContentAsync("Platform");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasCustomContentAsync_ReturnsFalse_WhenContentHtmlIsEmpty()
    {
        // Arrange
        _dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
        {
            Id = 1,
            InfoTypeKey = "Platform",
            Title = "Platform",
            ContentHtml = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasCustomContentAsync("Platform");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasCustomContentAsync_ChecksAllTables()
    {
        // Arrange - only add to technology table
        _dbContext.TechnologyConfigs.Add(new TechnologyConfigEntity
        {
            Id = 1,
            TechnologyKey = "CustomKey",
            Name = "Custom",
            DetailedInfoHtml = "<p>Content</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasCustomContentAsync("CustomKey");

        // Assert - should find it in technology table
        Assert.True(result);
    }

    #endregion

    #region InfoContent Record Tests

    [Fact]
    public void InfoContent_Empty_ReturnsCorrectValues()
    {
        // Act
        var empty = InfoContent.Empty;

        // Assert
        Assert.Equal("Information", empty.Title);
        Assert.Empty(empty.ContentHtml);
        Assert.False(empty.IsFromDatabase);
    }

    [Fact]
    public void InfoContent_Constructor_SetsPropertiesCorrectly()
    {
        // Act
        var content = new InfoContent("Test Title", "<p>Test content</p>") { IsFromDatabase = true };

        // Assert
        Assert.Equal("Test Title", content.Title);
        Assert.Equal("<p>Test content</p>", content.ContentHtml);
        Assert.True(content.IsFromDatabase);
    }

    #endregion

    #region DefaultInfoTypeContent Tests

    [Fact]
    public void DefaultInfoTypeContent_Get_ReturnsCorrectContent_ForPlatform()
    {
        // Act
        var result = DefaultInfoTypeContent.Get("Platform");

        // Assert
        Assert.Equal("Platform Types", result.Title);
        Assert.Contains("Native Applications", result.ContentHtml);
        Assert.Contains("Low-Code Platforms", result.ContentHtml);
    }

    [Fact]
    public void DefaultInfoTypeContent_Get_ReturnsCorrectContent_ForDeployment()
    {
        // Act
        var result = DefaultInfoTypeContent.Get("Deployment");

        // Assert
        Assert.Equal("Deployment Models", result.Title);
        Assert.Contains("Kubernetes", result.ContentHtml);
        Assert.Contains("Virtual Machines", result.ContentHtml);
    }

    [Fact]
    public void DefaultInfoTypeContent_Get_ReturnsEmpty_ForUnknownType()
    {
        // Act
        var result = DefaultInfoTypeContent.Get("Unknown");

        // Assert
        Assert.Equal("Information", result.Title);
        Assert.Empty(result.ContentHtml);
    }

    #endregion

    #region DefaultDistributionContent Tests

    [Fact]
    public void DefaultDistributionContent_Get_ReturnsCorrectContent_ForOpenShift()
    {
        // Act
        var result = DefaultDistributionContent.Get(Distribution.OpenShift);

        // Assert
        Assert.Equal("Red Hat OpenShift (On-Prem)", result.Title);
        Assert.Contains("Red Hat (IBM)", result.ContentHtml);
        Assert.Contains("Node Architecture", result.ContentHtml);
    }

    [Fact]
    public void DefaultDistributionContent_Get_ReturnsCorrectContent_ForEKS()
    {
        // Act
        var result = DefaultDistributionContent.Get(Distribution.EKS);

        // Assert
        Assert.Equal("Amazon EKS", result.Title);
        Assert.Contains("Amazon Web Services", result.ContentHtml);
    }

    [Fact]
    public void DefaultDistributionContent_Get_ReturnsCorrectContent_ForAKS()
    {
        // Act
        var result = DefaultDistributionContent.Get(Distribution.AKS);

        // Assert
        Assert.Equal("Azure AKS", result.Title);
        Assert.Contains("Microsoft Azure", result.ContentHtml);
    }

    [Fact]
    public void DefaultDistributionContent_Get_ReturnsCorrectContent_ForGKE()
    {
        // Act
        var result = DefaultDistributionContent.Get(Distribution.GKE);

        // Assert
        Assert.Equal("Google GKE", result.Title);
        Assert.Contains("Google Cloud", result.ContentHtml);
    }

    #endregion

    #region DefaultTechnologyContent Tests

    [Fact]
    public void DefaultTechnologyContent_Get_ReturnsCorrectContent_ForDotNet()
    {
        // Act
        var result = DefaultTechnologyContent.Get(Technology.DotNet);

        // Assert
        Assert.Equal(".NET", result.Title);
        Assert.Contains("Microsoft", result.ContentHtml);
        Assert.Contains("Resource Profile", result.ContentHtml);
    }

    [Fact]
    public void DefaultTechnologyContent_Get_ReturnsCorrectContent_ForJava()
    {
        // Act
        var result = DefaultTechnologyContent.Get(Technology.Java);

        // Assert
        Assert.Equal("Java", result.Title);
        Assert.Contains("Oracle", result.ContentHtml);
    }

    [Fact]
    public void DefaultTechnologyContent_Get_ReturnsCorrectContent_ForMendix()
    {
        // Act
        var result = DefaultTechnologyContent.Get(Technology.Mendix);

        // Assert
        Assert.Equal("Mendix", result.Title);
        Assert.Contains("Siemens", result.ContentHtml);
        Assert.Contains("low-code", result.ContentHtml);
    }

    [Fact]
    public void DefaultTechnologyContent_Get_ReturnsCorrectContent_ForOutSystems()
    {
        // Act
        var result = DefaultTechnologyContent.Get(Technology.OutSystems);

        // Assert
        Assert.Equal("OutSystems", result.Title);
        Assert.Contains("low-code", result.ContentHtml);
    }

    #endregion

    #region Caching Behavior Tests

    [Fact]
    public async Task GetInfoTypeContentAsync_CachesDefaultContent()
    {
        // Act
        await _service.GetInfoTypeContentAsync("Platform");

        // Assert
        Assert.True(_cache.TryGetValue("info_type_Platform", out InfoContent? cached));
        Assert.NotNull(cached);
        Assert.Equal("Platform Types", cached!.Title);
    }

    [Fact]
    public async Task GetDistributionInfoAsync_CachesDefaultContent()
    {
        // Act
        await _service.GetDistributionInfoAsync(Distribution.OpenShift);

        // Assert
        Assert.True(_cache.TryGetValue("distro_info_OpenShift", out InfoContent? cached));
        Assert.NotNull(cached);
        Assert.Contains("Red Hat", cached!.Title);
    }

    [Fact]
    public async Task GetTechnologyInfoAsync_CachesDefaultContent()
    {
        // Act
        await _service.GetTechnologyInfoAsync(Technology.DotNet);

        // Assert
        Assert.True(_cache.TryGetValue("tech_info_DotNet", out InfoContent? cached));
        Assert.NotNull(cached);
        Assert.Equal(".NET", cached!.Title);
    }

    [Fact]
    public async Task MultipleCallsToSameContent_UseCache()
    {
        // Arrange - add DB content
        _dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
        {
            Id = 1,
            InfoTypeKey = "Platform",
            Title = "DB Platform",
            ContentHtml = "<p>DB content</p>",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act - call twice
        var result1 = await _service.GetInfoTypeContentAsync("Platform");

        // Remove from DB to prove cache is used
        _dbContext.InfoTypeContents.RemoveRange(_dbContext.InfoTypeContents);
        await _dbContext.SaveChangesAsync();

        var result2 = await _service.GetInfoTypeContentAsync("Platform");

        // Assert - both should return same cached content
        Assert.Equal(result1.Title, result2.Title);
        Assert.Equal(result1.ContentHtml, result2.ContentHtml);
    }

    #endregion
}
