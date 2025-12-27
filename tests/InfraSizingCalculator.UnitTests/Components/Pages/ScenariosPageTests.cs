using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Pages;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pages;

/// <summary>
/// Tests for Scenarios page - Saved scenario management
/// </summary>
public class ScenariosPageTests : TestContext
{
    private readonly IScenarioService _scenarioService;
    private readonly ConfigurationSharingService _sharingService;
    private readonly IJSRuntime _jsRuntime;

    public ScenariosPageTests()
    {
        _scenarioService = Substitute.For<IScenarioService>();
        _jsRuntime = Substitute.For<IJSRuntime>();
        // ConfigurationSharingService is a concrete class requiring IJSRuntime in constructor
        _sharingService = new ConfigurationSharingService(_jsRuntime);

        // Default - return empty list (no scenarios)
        _scenarioService.GetScenarioSummariesAsync().Returns(new List<ScenarioSummary>());

        Services.AddSingleton(_scenarioService);
        Services.AddSingleton(_sharingService);
        Services.AddSingleton(_jsRuntime);
        Services.AddSingleton<NavigationManager>(new FakeNavigationManager(this));
    }

    #region Empty State Tests

    [Fact]
    public async Task Scenarios_EmptyState_ShowsEmptyMessage()
    {
        // Arrange
        _scenarioService.GetScenarioSummariesAsync().Returns(new List<ScenarioSummary>());

        // Act
        var cut = RenderComponent<Scenarios>();
        await cut.InvokeAsync(() => { }); // Wait for async init

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".empty-state").Should().NotBeNull();
            cut.Find(".empty-state h3").TextContent.Should().Contain("No saved scenarios");
        });
    }

    [Fact]
    public async Task Scenarios_EmptyState_HasGoToCalculatorLink()
    {
        // Arrange
        _scenarioService.GetScenarioSummariesAsync().Returns(new List<ScenarioSummary>());

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var link = cut.Find(".empty-state a");
            link.TextContent.Should().Contain("Calculator");
            link.GetAttribute("href").Should().Be("/");
        });
    }

    #endregion

    #region Loading State Tests

    [Fact]
    public void Scenarios_InitiallyShowsLoading()
    {
        // Arrange - Make the service slow to respond
        var tcs = new TaskCompletionSource<List<ScenarioSummary>>();
        _scenarioService.GetScenarioSummariesAsync().Returns(tcs.Task);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.Find(".loading-state").TextContent.Should().Contain("Loading");
    }

    #endregion

    #region Scenarios List Tests

    [Fact]
    public async Task Scenarios_WithScenarios_ShowsList()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".scenarios-list").Should().NotBeNull();
            cut.FindAll(".scenario-card").Should().HaveCount(scenarios.Count);
        });
    }

    [Fact]
    public async Task Scenarios_ShowsScenarioDetails()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Production OpenShift Cluster",
                Type = "k8s",
                TotalNodes = 12,
                TotalCpu = 96,
                DistributionOrTechnology = "OpenShift",
                CreatedAt = DateTime.UtcNow,
                MonthlyEstimate = 15000m
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var card = cut.Find(".scenario-card");
            card.TextContent.Should().Contain("Production OpenShift Cluster");
            card.TextContent.Should().Contain("12");
            card.TextContent.Should().Contain("nodes");
        });
    }

    [Fact]
    public async Task Scenarios_ShowsTypeBadge()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "K8s Scenario",
                Type = "k8s",
                CreatedAt = DateTime.UtcNow
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".scenario-type-badge").TextContent.Should().Contain("K8s");
        });
    }

    [Fact]
    public async Task Scenarios_ShowsFavoriteBadge_WhenFavorite()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Favorite Scenario",
                Type = "k8s",
                IsFavorite = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".favorite-badge").Should().NotBeNull();
        });
    }

    #endregion

    #region Tab Navigation Tests

    [Fact]
    public async Task Scenarios_ShowsSavedAndDraftTabs()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var tabs = cut.FindAll(".scenario-tab");
            tabs.Should().HaveCount(2);
            tabs[0].TextContent.Should().Contain("Saved");
            tabs[1].TextContent.Should().Contain("Draft");
        });
    }

    [Fact]
    public async Task Scenarios_SavedTabActiveByDefault()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".scenario-tab.active").TextContent.Should().Contain("Saved");
        });
    }

    [Fact]
    public async Task Scenarios_ClickingDraftTab_SwitchesToDrafts()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Saved One", IsDraft = false, Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Draft One", IsDraft = true, Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Act
        cut.WaitForAssertion(() =>
        {
            var draftTab = cut.FindAll(".scenario-tab")[1];
            draftTab.Click();
        });

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".scenario-tab.active").TextContent.Should().Contain("Draft");
        });
    }

    #endregion

    #region View Mode Tests

    [Fact]
    public async Task Scenarios_ShowsViewModeToggle()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".view-toggle").Should().NotBeNull();
            var viewBtns = cut.FindAll(".view-btn");
            viewBtns.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task Scenarios_ListViewActiveByDefault()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".view-btn.active").TextContent.Should().Contain("List");
        });
    }

    [Fact]
    public async Task Scenarios_CompareButtonDisabled_WhenLessThan2Selected()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var compareBtn = cut.FindAll(".view-btn").First(b => b.TextContent.Contains("Compare"));
            compareBtn.GetAttribute("disabled").Should().NotBeNull();
        });
    }

    #endregion

    #region Selection Tests

    [Fact]
    public async Task Scenarios_SelectingScenario_AddsToSelection()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Act
        cut.WaitForAssertion(() =>
        {
            var checkbox = cut.Find(".scenario-select input[type='checkbox']");
            checkbox.Change(true);
        });

        // Assert - The card should now have selected class
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".scenario-card.selected").Should().HaveCount(1);
        });
    }

    [Fact]
    public async Task Scenarios_SelectingMultiple_ShowsCompareButton()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Wait for initial render
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".scenario-card").Should().HaveCount(scenarios.Count);
        });

        // Act - Select first scenario and wait for re-render
        await cut.InvokeAsync(() => cut.Find(".scenario-select input[type='checkbox']").Change(true));

        // Select second scenario (re-query after render)
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[1].Change(true));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var compareBtn = cut.FindAll(".btn-primary").FirstOrDefault(b => b.TextContent.Contains("Compare"));
            compareBtn.Should().NotBeNull();
        });
    }

    #endregion

    #region Action Button Tests

    [Fact]
    public async Task Scenarios_HasActionButtons()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert - Each scenario card has 3 action buttons (Favorite, Duplicate, Delete)
        // With 3 scenarios, we expect 9 total action buttons
        cut.WaitForAssertion(() =>
        {
            var actions = cut.FindAll(".scenario-actions .action-btn");
            actions.Should().HaveCount(scenarios.Count * 3); // 3 buttons per scenario
        });
    }

    [Fact]
    public async Task Scenarios_ClickingFavorite_CallsToggleFavorite()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Act
        cut.WaitForAssertion(() =>
        {
            var favoriteBtn = cut.Find(".scenario-actions .action-btn");
            favoriteBtn.Click();
        });

        // Assert
        await _scenarioService.Received(1).ToggleFavoriteAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Scenarios_ClickingDelete_CallsDeleteScenario()
    {
        // Arrange
        var scenarioId = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = scenarioId, Name = "Test", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Act
        cut.WaitForAssertion(() =>
        {
            var deleteBtn = cut.FindAll(".action-btn.danger").First();
            deleteBtn.Click();
        });

        // Assert
        await _scenarioService.Received(1).DeleteScenarioAsync(scenarioId);
    }

    [Fact]
    public async Task Scenarios_ClickingDuplicate_CallsDuplicateScenario()
    {
        // Arrange
        var scenarioId = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = scenarioId, Name = "Original", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Act
        cut.WaitForAssertion(() =>
        {
            // Duplicate is the second action button (after favorite)
            var duplicateBtn = cut.FindAll(".action-btn")[1];
            duplicateBtn.Click();
        });

        // Assert
        await _scenarioService.Received(1).DuplicateScenarioAsync(scenarioId, "Original (Copy)");
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void Scenarios_HasBackButton()
    {
        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        var backBtn = cut.Find(".back-link");
        backBtn.TextContent.Should().Contain("Back to Calculator");
    }

    [Fact]
    public void Scenarios_ClickingBack_NavigatesToHome()
    {
        // Arrange
        var cut = RenderComponent<Scenarios>();
        var nav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

        // Act
        cut.Find(".back-link").Click();

        // Assert
        nav!.NavigatedUri.Should().Be("/");
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task Scenarios_ShowsFilterDropdown()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".filter-select").Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Scenarios_FilterHasTypeOptions()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var options = cut.FindAll(".filter-select option");
            options.Should().HaveCountGreaterThan(1);
            options.Should().Contain(o => o.TextContent.Contains("Kubernetes"));
            options.Should().Contain(o => o.TextContent.Contains("Virtual Machines"));
        });
    }

    #endregion

    #region Header Tests

    [Fact]
    public void Scenarios_ShowsPageTitle()
    {
        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Saved Scenarios");
    }

    [Fact]
    public void Scenarios_ShowsSubtitle()
    {
        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.Find(".subtitle").TextContent.Should().Contain("Compare and manage");
    }

    #endregion

    #region Helper Methods

    private List<ScenarioSummary> CreateSampleScenarios()
    {
        return new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Development Cluster",
                Type = "k8s",
                TotalNodes = 6,
                TotalCpu = 48,
                DistributionOrTechnology = "OpenShift",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsDraft = false,
                IsFavorite = false
            },
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Production Cluster",
                Type = "k8s",
                TotalNodes = 12,
                TotalCpu = 96,
                DistributionOrTechnology = "AKS",
                CreatedAt = DateTime.UtcNow,
                IsDraft = false,
                IsFavorite = true,
                MonthlyEstimate = 25000m
            },
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Test VM Setup",
                Type = "vm",
                TotalNodes = 3,
                TotalCpu = 24,
                DistributionOrTechnology = ".NET",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                IsDraft = false,
                IsFavorite = false
            }
        };
    }

    #endregion

    /// <summary>
    /// Fake NavigationManager for testing navigation
    /// </summary>
    private class FakeNavigationManager : NavigationManager
    {
        private readonly TestContext _context;

        public FakeNavigationManager(TestContext context)
        {
            _context = context;
            Initialize("http://localhost/", "http://localhost/scenarios");
        }

        public string? NavigatedUri { get; private set; }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            NavigatedUri = uri;
        }
    }
}
