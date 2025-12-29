using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Pages;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
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

    #region FormatCurrency Tests

    [Fact]
    public async Task Scenarios_FormatsMonthlyCost_InThousands()
    {
        // Arrange - scenario with 15,000 monthly cost should show as $15.0K
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Test Scenario",
                Type = "k8s",
                MonthlyEstimate = 15000m,
                CreatedAt = DateTime.UtcNow
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var costText = cut.Find(".meta-item.cost").TextContent;
            costText.Should().Contain("$15.0K");
        });
    }

    [Fact]
    public async Task Scenarios_FormatsMonthlyCost_InMillions()
    {
        // Arrange - scenario with 1.5M monthly cost
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Large Scale",
                Type = "k8s",
                MonthlyEstimate = 1500000m,
                CreatedAt = DateTime.UtcNow
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var costText = cut.Find(".meta-item.cost").TextContent;
            costText.Should().Contain("$1.50M");
        });
    }

    [Fact]
    public async Task Scenarios_DoesNotShowCost_WhenZero()
    {
        // Arrange - scenario with no cost estimate
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Free Tier",
                Type = "k8s",
                MonthlyEstimate = 0m,
                CreatedAt = DateTime.UtcNow
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var costItems = cut.FindAll(".meta-item.cost");
            costItems.Should().BeEmpty();
        });
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task Scenarios_DeleteSelected_ShowsDeleteButton()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Wait for render
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".scenario-card").Should().HaveCount(scenarios.Count);
        });

        // Act - Select a scenario
        await cut.InvokeAsync(() => cut.Find(".scenario-select input[type='checkbox']").Change(true));

        // Assert - Delete button appears
        cut.WaitForAssertion(() =>
        {
            var deleteBtn = cut.FindAll(".btn-secondary.btn-danger").FirstOrDefault();
            deleteBtn.Should().NotBeNull();
            deleteBtn!.TextContent.Should().Contain("Delete (1)");
        });
    }

    [Fact]
    public async Task Scenarios_DeleteSelected_CallsDeleteForEachSelected()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = id1, Name = "First", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = id2, Name = "Second", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Select both scenarios
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(2));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[0].Change(true));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[1].Change(true));

        // Act - Click bulk delete
        await cut.InvokeAsync(() => cut.Find(".btn-secondary.btn-danger").Click());

        // Assert
        await _scenarioService.Received(1).DeleteScenarioAsync(id1);
        await _scenarioService.Received(1).DeleteScenarioAsync(id2);
    }

    [Fact]
    public async Task Scenarios_CompareSelected_ShowsCompareButtonWithCount()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Wait and select 2 scenarios
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(scenarios.Count));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[0].Change(true));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[1].Change(true));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var compareBtn = cut.FindAll(".btn-primary").FirstOrDefault(b => b.TextContent.Contains("Compare"));
            compareBtn.Should().NotBeNull();
            compareBtn!.TextContent.Should().Contain("Compare (2)");
        });
    }

    [Fact]
    public async Task Scenarios_SelectingScenario_MaximumOf4Allowed()
    {
        // Arrange - Create 5 scenarios
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "S1", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "S2", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "S3", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "S4", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "S5", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Select 4 scenarios
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(5));
        for (int i = 0; i < 4; i++)
        {
            await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[i].Change(true));
        }

        // Try to select 5th - should not add
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[4].Change(true));

        // Assert - Only 4 should be selected
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".scenario-card.selected").Should().HaveCount(4);
        });
    }

    #endregion

    #region Comparison View Tests

    [Fact]
    public async Task Scenarios_CompareSelected_SwitchesToCompareView()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = id1, Name = "First", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = id2, Name = "Second", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Setup comparison result
        var comparison = new ScenarioComparison
        {
            Scenarios = new[]
            {
                new Scenario { Id = id1, Name = "First", Type = "k8s" },
                new Scenario { Id = id2, Name = "Second", Type = "k8s" }
            },
            Metrics = new List<ComparisonMetric>
            {
                new ComparisonMetric
                {
                    Name = "Total Nodes",
                    Category = "Resources",
                    Unit = "nodes",
                    Values = new Dictionary<Guid, decimal> { { id1, 5 }, { id2, 10 } },
                    WinnerId = id1
                }
            },
            Insights = new List<string> { "First scenario uses fewer nodes" }
        };
        _scenarioService.CompareByIdsAsync(Arg.Any<Guid[]>()).Returns(comparison);

        var cut = RenderComponent<Scenarios>();

        // Select 2 and compare
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(2));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[0].Change(true));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[1].Change(true));
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert - Comparison view should be visible
        cut.WaitForAssertion(() =>
        {
            cut.Find(".comparison-view").Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Scenarios_ComparisonView_ShowsMetrics()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = id1, Name = "First", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = id2, Name = "Second", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        var comparison = new ScenarioComparison
        {
            Scenarios = new[]
            {
                new Scenario { Id = id1, Name = "First", Type = "k8s" },
                new Scenario { Id = id2, Name = "Second", Type = "k8s" }
            },
            Metrics = new List<ComparisonMetric>
            {
                new ComparisonMetric
                {
                    Name = "Monthly Cost",
                    Category = "Cost",
                    Unit = "$",
                    Values = new Dictionary<Guid, decimal> { { id1, 5000 }, { id2, 10000 } },
                    WinnerId = id1
                }
            }
        };
        _scenarioService.CompareByIdsAsync(Arg.Any<Guid[]>()).Returns(comparison);

        var cut = RenderComponent<Scenarios>();
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(2));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[0].Change(true));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[1].Change(true));
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".comparison-table").Should().NotBeNull();
            cut.Find(".metric-name").TextContent.Should().Contain("Monthly Cost");
        });
    }

    [Fact]
    public async Task Scenarios_ComparisonView_ShowsInsights()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = id1, Name = "A", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = id2, Name = "B", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        var comparison = new ScenarioComparison
        {
            Scenarios = new[]
            {
                new Scenario { Id = id1, Name = "A", Type = "k8s" },
                new Scenario { Id = id2, Name = "B", Type = "k8s" }
            },
            Metrics = new List<ComparisonMetric>(),
            Insights = new List<string> { "Scenario A offers better cost efficiency" }
        };
        _scenarioService.CompareByIdsAsync(Arg.Any<Guid[]>()).Returns(comparison);

        var cut = RenderComponent<Scenarios>();
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(2));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[0].Change(true));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[1].Change(true));
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".comparison-insights").Should().NotBeNull();
            cut.Find(".comparison-insights li").TextContent.Should().Contain("cost efficiency");
        });
    }

    [Fact]
    public async Task Scenarios_ComparisonView_ShowsRecommendation()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = id1, Name = "Recommended One", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = id2, Name = "Other", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        var comparison = new ScenarioComparison
        {
            Scenarios = new[]
            {
                new Scenario { Id = id1, Name = "Recommended One", Type = "k8s" },
                new Scenario { Id = id2, Name = "Other", Type = "k8s" }
            },
            Metrics = new List<ComparisonMetric>(),
            RecommendedScenarioId = id1,
            RecommendationReason = "Best value for resources"
        };
        _scenarioService.CompareByIdsAsync(Arg.Any<Guid[]>()).Returns(comparison);

        var cut = RenderComponent<Scenarios>();
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(2));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[0].Change(true));
        await cut.InvokeAsync(() => cut.FindAll(".scenario-select input[type='checkbox']")[1].Change(true));
        await cut.InvokeAsync(() => cut.Find(".btn-primary").Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".recommendation-banner").Should().NotBeNull();
            cut.Find(".recommendation-banner").TextContent.Should().Contain("Recommended One");
        });
    }

    #endregion

    #region Tab Badge Count Tests

    [Fact]
    public async Task Scenarios_TabsShowCorrectCounts()
    {
        // Arrange - 2 saved, 1 draft
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Saved1", IsDraft = false, Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Saved2", IsDraft = false, Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Draft1", IsDraft = true, Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var tabs = cut.FindAll(".scenario-tab");
            var savedTab = tabs[0];
            var draftTab = tabs[1];

            savedTab.TextContent.Should().Contain("2");
            draftTab.TextContent.Should().Contain("1");
        });
    }

    [Fact]
    public async Task Scenarios_SwitchingTabs_ClearsSelection()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Saved", IsDraft = false, Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "Draft", IsDraft = true, Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Select saved scenario
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(1));
        await cut.InvokeAsync(() => cut.Find(".scenario-select input[type='checkbox']").Change(true));

        // Act - Switch to drafts tab
        await cut.InvokeAsync(() => cut.FindAll(".scenario-tab")[1].Click());

        // Switch back to saved
        await cut.InvokeAsync(() => cut.FindAll(".scenario-tab")[0].Click());

        // Assert - Selection should be cleared
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".scenario-card.selected").Should().BeEmpty();
        });
    }

    #endregion

    #region Filter By Type Tests

    [Fact]
    public async Task Scenarios_FilterByK8s_ShowsOnlyK8sScenarios()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "K8s One", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "VM One", Type = "vm", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Act - Filter by k8s
        cut.WaitForAssertion(() => cut.Find(".filter-select").Should().NotBeNull());
        await cut.InvokeAsync(() => cut.Find(".filter-select").Change("k8s"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var cards = cut.FindAll(".scenario-card");
            cards.Should().HaveCount(1);
            cards[0].TextContent.Should().Contain("K8s One");
        });
    }

    [Fact]
    public async Task Scenarios_FilterByVM_ShowsOnlyVMScenarios()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "K8s", Type = "k8s", CreatedAt = DateTime.UtcNow },
            new ScenarioSummary { Id = Guid.NewGuid(), Name = "VM Only", Type = "vm", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Act
        cut.WaitForAssertion(() => cut.Find(".filter-select").Should().NotBeNull());
        await cut.InvokeAsync(() => cut.Find(".filter-select").Change("vm"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var cards = cut.FindAll(".scenario-card");
            cards.Should().HaveCount(1);
            cards[0].TextContent.Should().Contain("VM Only");
        });
    }

    #endregion

    #region ViewScenario Tests

    [Fact]
    public async Task Scenarios_ClickingScenario_LoadsFullScenario()
    {
        // Arrange
        var scenarioId = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = scenarioId, Name = "Test", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        var fullScenario = new Scenario
        {
            Id = scenarioId,
            Name = "Test",
            Type = "k8s",
            K8sInput = new K8sSizingInput
            {
                Distribution = Distribution.OpenShift
            }
        };
        _scenarioService.GetScenarioAsync(scenarioId).Returns(fullScenario);

        var cut = RenderComponent<Scenarios>();

        // Act
        cut.WaitForAssertion(() => cut.Find(".scenario-info").Should().NotBeNull());
        await cut.InvokeAsync(() => cut.Find(".scenario-info").Click());

        // Assert
        await _scenarioService.Received(1).GetScenarioAsync(scenarioId);
    }

    [Fact]
    public async Task Scenarios_ClickingScenario_NavigatesWithConfig()
    {
        // Arrange
        var scenarioId = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = scenarioId, Name = "Test", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        var fullScenario = new Scenario
        {
            Id = scenarioId,
            Name = "Test",
            Type = "k8s",
            K8sInput = new K8sSizingInput { Distribution = Distribution.OpenShift }
        };
        _scenarioService.GetScenarioAsync(scenarioId).Returns(fullScenario);

        var cut = RenderComponent<Scenarios>();
        var nav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

        // Act
        cut.WaitForAssertion(() => cut.Find(".scenario-info").Should().NotBeNull());
        await cut.InvokeAsync(() => cut.Find(".scenario-info").Click());

        // Assert - Should navigate with config param
        await Task.Delay(50); // Small delay for async navigation
        nav!.NavigatedUri.Should().StartWith("/?config=");
    }

    [Fact]
    public async Task Scenarios_ClickingScenario_NavigatesToHome_WhenScenarioNotFound()
    {
        // Arrange
        var scenarioId = Guid.NewGuid();
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary { Id = scenarioId, Name = "Gone", Type = "k8s", CreatedAt = DateTime.UtcNow }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        _scenarioService.GetScenarioAsync(scenarioId).Returns((Scenario?)null);

        var cut = RenderComponent<Scenarios>();
        var nav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

        // Act
        cut.WaitForAssertion(() => cut.Find(".scenario-info").Should().NotBeNull());
        await cut.InvokeAsync(() => cut.Find(".scenario-info").Click());

        // Assert
        nav!.NavigatedUri.Should().Be("/");
    }

    #endregion

    #region Tags Display Tests

    [Fact]
    public async Task Scenarios_ShowsTags_WhenPresent()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Tagged",
                Type = "k8s",
                CreatedAt = DateTime.UtcNow,
                Tags = new List<string> { "production", "high-availability" }
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var tags = cut.FindAll(".tag");
            tags.Should().HaveCountGreaterThan(0);
            tags.Should().Contain(t => t.TextContent.Contains("production"));
        });
    }

    [Fact]
    public async Task Scenarios_LimitsTags_ToThree()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "Many Tags",
                Type = "k8s",
                CreatedAt = DateTime.UtcNow,
                Tags = new List<string> { "one", "two", "three", "four", "five" }
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert - Only first 3 tags should be shown
        cut.WaitForAssertion(() =>
        {
            var tags = cut.FindAll(".tag");
            tags.Should().HaveCount(3);
        });
    }

    #endregion

    #region Deselection Tests

    [Fact]
    public async Task Scenarios_DeselectingScenario_RemovesFromSelection()
    {
        // Arrange
        var scenarios = CreateSampleScenarios();
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);
        var cut = RenderComponent<Scenarios>();

        // Select first scenario
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card").Should().HaveCount(scenarios.Count));
        await cut.InvokeAsync(() => cut.Find(".scenario-select input[type='checkbox']").Change(true));
        cut.WaitForAssertion(() => cut.FindAll(".scenario-card.selected").Should().HaveCount(1));

        // Act - Deselect
        await cut.InvokeAsync(() => cut.Find(".scenario-select input[type='checkbox']").Change(false));

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".scenario-card.selected").Should().BeEmpty();
        });
    }

    #endregion

    #region VM Type Badge Tests

    [Fact]
    public async Task Scenarios_ShowsVMBadge_ForVMScenarios()
    {
        // Arrange
        var scenarios = new List<ScenarioSummary>
        {
            new ScenarioSummary
            {
                Id = Guid.NewGuid(),
                Name = "VM Setup",
                Type = "vm",
                TotalNodes = 5,
                CreatedAt = DateTime.UtcNow
            }
        };
        _scenarioService.GetScenarioSummariesAsync().Returns(scenarios);

        // Act
        var cut = RenderComponent<Scenarios>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".scenario-type-badge").TextContent.Should().Contain("VM");
            cut.Find(".scenario-meta").TextContent.Should().Contain("VMs"); // "5 VMs" not "5 nodes"
        });
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
