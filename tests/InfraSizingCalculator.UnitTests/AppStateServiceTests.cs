using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class AppStateServiceTests
{
    private readonly AppStateService _service;

    public AppStateServiceTests()
    {
        _service = new AppStateService();
    }

    #region Initial State Tests

    [Fact]
    public void InitialState_ActiveSectionIsConfig()
    {
        // Assert
        _service.ActiveSection.Should().Be("config");
    }

    [Fact]
    public void InitialState_ExpandedCardIsNull()
    {
        // Assert
        _service.ExpandedCard.Should().BeNull();
    }

    [Fact]
    public void InitialState_HasResultsIsFalse()
    {
        // Assert
        _service.HasResults.Should().BeFalse();
    }

    [Fact]
    public void InitialState_K8sResultsIsNull()
    {
        // Assert
        _service.K8sResults.Should().BeNull();
    }

    [Fact]
    public void InitialState_VMResultsIsNull()
    {
        // Assert
        _service.VMResults.Should().BeNull();
    }

    [Fact]
    public void InitialState_SummaryPropertiesAreZero()
    {
        // Assert
        _service.TotalNodes.Should().Be(0);
        _service.TotalCPU.Should().Be(0);
        _service.TotalRAM.Should().Be(0);
        _service.MonthlyEstimate.Should().Be(0);
    }

    [Fact]
    public void InitialState_CostProviderIsNull()
    {
        // Assert
        _service.CostProvider.Should().BeNull();
    }

    #endregion

    #region ActiveSection Tests

    [Fact]
    public void ActiveSection_CanBeChanged()
    {
        // Act
        _service.ActiveSection = "sizing";

        // Assert
        _service.ActiveSection.Should().Be("sizing");
    }

    [Fact]
    public void ActiveSection_ChangingDoesNotClearResults()
    {
        // Arrange
        var results = CreateK8sSizingResult();
        _service.SetK8sResults(results);

        // Act
        _service.ActiveSection = "cost";

        // Assert
        _service.K8sResults.Should().NotBeNull();
        _service.HasResults.Should().BeTrue();
    }

    #endregion

    #region NavigateToSection Tests

    [Theory]
    [InlineData("config")]
    [InlineData("sizing")]
    [InlineData("cost")]
    [InlineData("growth")]
    public void NavigateToSection_ValidSection_SetsActiveSection(string section)
    {
        // Act
        _service.NavigateToSection(section);

        // Assert
        _service.ActiveSection.Should().Be(section);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("unknown")]
    public void NavigateToSection_InvalidSection_DefaultsToConfig(string section)
    {
        // Act
        _service.NavigateToSection(section);

        // Assert
        _service.ActiveSection.Should().Be("config");
    }

    [Fact]
    public void NavigateToSection_FiresOnStateChangedEvent()
    {
        // Arrange
        var eventFired = false;
        _service.OnStateChanged += () => eventFired = true;

        // Act
        _service.NavigateToSection("sizing");

        // Assert
        eventFired.Should().BeTrue();
    }

    #endregion

    #region SetK8sResults Tests

    [Fact]
    public void SetK8sResults_SetsResults()
    {
        // Arrange
        var results = CreateK8sSizingResult();

        // Act
        _service.SetK8sResults(results);

        // Assert
        _service.K8sResults.Should().Be(results);
        _service.HasResults.Should().BeTrue();
    }

    [Fact]
    public void SetK8sResults_ClearsVMResults()
    {
        // Arrange
        var vmResults = CreateVMSizingResult();
        _service.SetVMResults(vmResults);
        var k8sResults = CreateK8sSizingResult();

        // Act
        _service.SetK8sResults(k8sResults);

        // Assert
        _service.VMResults.Should().BeNull();
        _service.VMCostEstimate.Should().BeNull();
    }

    [Fact]
    public void SetK8sResults_NavigatesToSizingSection()
    {
        // Arrange
        var results = CreateK8sSizingResult();

        // Act
        _service.SetK8sResults(results);

        // Assert
        _service.ActiveSection.Should().Be("sizing");
    }

    [Fact]
    public void SetK8sResults_FiresOnStateChangedEvent()
    {
        // Arrange
        var eventFired = false;
        _service.OnStateChanged += () => eventFired = true;
        var results = CreateK8sSizingResult();

        // Act
        _service.SetK8sResults(results);

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void SetK8sResults_UpdatesSummaryProperties()
    {
        // Arrange
        var results = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            Configuration = new K8sSizingInput(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 25,
                TotalCpu = 200,
                TotalRam = 400
            }
        };

        // Act
        _service.SetK8sResults(results);

        // Assert
        _service.TotalNodes.Should().Be(25);
        _service.TotalCPU.Should().Be(200);
        _service.TotalRAM.Should().Be(400);
    }

    #endregion

    #region SetVMResults Tests

    [Fact]
    public void SetVMResults_SetsResults()
    {
        // Arrange
        var results = CreateVMSizingResult();

        // Act
        _service.SetVMResults(results);

        // Assert
        _service.VMResults.Should().Be(results);
        _service.HasResults.Should().BeTrue();
    }

    [Fact]
    public void SetVMResults_ClearsK8sResults()
    {
        // Arrange
        var k8sResults = CreateK8sSizingResult();
        _service.SetK8sResults(k8sResults);
        var vmResults = CreateVMSizingResult();

        // Act
        _service.SetVMResults(vmResults);

        // Assert
        _service.K8sResults.Should().BeNull();
        _service.K8sCostEstimate.Should().BeNull();
    }

    [Fact]
    public void SetVMResults_NavigatesToSizingSection()
    {
        // Arrange
        var results = CreateVMSizingResult();

        // Act
        _service.SetVMResults(results);

        // Assert
        _service.ActiveSection.Should().Be("sizing");
    }

    [Fact]
    public void SetVMResults_UpdatesSummaryProperties()
    {
        // Arrange
        var results = new VMSizingResult
        {
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 15,
                TotalCpu = 120,
                TotalRam = 240
            }
        };

        // Act
        _service.SetVMResults(results);

        // Assert
        _service.TotalNodes.Should().Be(15);
        _service.TotalCPU.Should().Be(120);
        _service.TotalRAM.Should().Be(240);
    }

    #endregion

    #region SetCostEstimate Tests

    [Fact]
    public void SetK8sCostEstimate_SetsCostEstimate()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        _service.SetK8sCostEstimate(estimate);

        // Assert
        _service.K8sCostEstimate.Should().Be(estimate);
    }

    [Fact]
    public void SetK8sCostEstimate_DoesNotChangeActiveSection()
    {
        // Arrange
        _service.ActiveSection = "sizing";
        var estimate = CreateCostEstimate();

        // Act
        _service.SetK8sCostEstimate(estimate);

        // Assert
        _service.ActiveSection.Should().Be("sizing");
    }

    [Fact]
    public void SetK8sCostEstimate_UpdatesMonthlyEstimate()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 5000m };

        // Act
        _service.SetK8sCostEstimate(estimate);

        // Assert
        _service.MonthlyEstimate.Should().Be(5000m);
    }

    [Fact]
    public void SetK8sCostEstimate_UpdatesCostProvider()
    {
        // Arrange
        var estimate = new CostEstimate { Provider = CloudProvider.AWS };

        // Act
        _service.SetK8sCostEstimate(estimate);

        // Assert
        _service.CostProvider.Should().Be("AWS");
    }

    [Fact]
    public void SetVMCostEstimate_SetsCostEstimate()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        _service.SetVMCostEstimate(estimate);

        // Assert
        _service.VMCostEstimate.Should().Be(estimate);
    }

    [Fact]
    public void SetVMCostEstimate_UpdatesMonthlyEstimate()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 7500m };

        // Act
        _service.SetVMCostEstimate(estimate);

        // Assert
        _service.MonthlyEstimate.Should().Be(7500m);
    }

    #endregion

    #region ResetResults Tests

    [Fact]
    public void ResetResults_ClearsAllResults()
    {
        // Arrange
        _service.SetK8sResults(CreateK8sSizingResult());
        _service.SetK8sCostEstimate(CreateCostEstimate());

        // Act
        _service.ResetResults();

        // Assert
        _service.K8sResults.Should().BeNull();
        _service.VMResults.Should().BeNull();
        _service.K8sCostEstimate.Should().BeNull();
        _service.VMCostEstimate.Should().BeNull();
    }

    [Fact]
    public void ResetResults_ClearsExpandedCard()
    {
        // Arrange
        _service.ExpandedCard = "some-card";

        // Act
        _service.ResetResults();

        // Assert
        _service.ExpandedCard.Should().BeNull();
    }

    [Fact]
    public void ResetResults_NavigatesToConfig()
    {
        // Arrange
        _service.ActiveSection = "sizing";

        // Act
        _service.ResetResults();

        // Assert
        _service.ActiveSection.Should().Be("config");
    }

    [Fact]
    public void ResetResults_HasResultsIsFalse()
    {
        // Arrange
        _service.SetK8sResults(CreateK8sSizingResult());

        // Act
        _service.ResetResults();

        // Assert
        _service.HasResults.Should().BeFalse();
    }

    [Fact]
    public void ResetResults_FiresOnStateChangedEvent()
    {
        // Arrange
        var eventFired = false;
        _service.OnStateChanged += () => eventFired = true;

        // Act
        _service.ResetResults();

        // Assert
        eventFired.Should().BeTrue();
    }

    #endregion

    #region ResetAll Tests

    [Fact]
    public void ResetAll_ResetsEverything()
    {
        // Arrange
        _service.SetK8sResults(CreateK8sSizingResult());
        _service.SetK8sCostEstimate(CreateCostEstimate());
        _service.ExpandedCard = "card1";
        _service.ActiveSection = "growth";

        // Act
        _service.ResetAll();

        // Assert
        _service.K8sResults.Should().BeNull();
        _service.K8sCostEstimate.Should().BeNull();
        _service.ExpandedCard.Should().BeNull();
        _service.ActiveSection.Should().Be("config");
    }

    #endregion

    #region ToggleExpand Tests

    [Fact]
    public void ToggleExpand_NoCardExpanded_ExpandsCard()
    {
        // Arrange
        _service.ExpandedCard = null;

        // Act
        _service.ToggleExpand("card1");

        // Assert
        _service.ExpandedCard.Should().Be("card1");
    }

    [Fact]
    public void ToggleExpand_SameCardExpanded_CollapsesCard()
    {
        // Arrange
        _service.ExpandedCard = "card1";

        // Act
        _service.ToggleExpand("card1");

        // Assert
        _service.ExpandedCard.Should().BeNull();
    }

    [Fact]
    public void ToggleExpand_DifferentCardExpanded_SwitchesToNewCard()
    {
        // Arrange
        _service.ExpandedCard = "card1";

        // Act
        _service.ToggleExpand("card2");

        // Assert
        _service.ExpandedCard.Should().Be("card2");
    }

    [Fact]
    public void ToggleExpand_FiresOnStateChangedEvent()
    {
        // Arrange
        var eventFired = false;
        _service.OnStateChanged += () => eventFired = true;

        // Act
        _service.ToggleExpand("card1");

        // Assert
        eventFired.Should().BeTrue();
    }

    #endregion

    #region IsExpanded Tests

    [Fact]
    public void IsExpanded_CardIsExpanded_ReturnsTrue()
    {
        // Arrange
        _service.ExpandedCard = "card1";

        // Act & Assert
        _service.IsExpanded("card1").Should().BeTrue();
    }

    [Fact]
    public void IsExpanded_CardIsNotExpanded_ReturnsFalse()
    {
        // Arrange
        _service.ExpandedCard = "card1";

        // Act & Assert
        _service.IsExpanded("card2").Should().BeFalse();
    }

    [Fact]
    public void IsExpanded_NoCardExpanded_ReturnsFalse()
    {
        // Arrange
        _service.ExpandedCard = null;

        // Act & Assert
        _service.IsExpanded("card1").Should().BeFalse();
    }

    #endregion

    #region NotifyStateChanged Tests

    [Fact]
    public void NotifyStateChanged_NoSubscribers_DoesNotThrow()
    {
        // Act & Assert
        var action = () => _service.NotifyStateChanged();
        action.Should().NotThrow();
    }

    [Fact]
    public void NotifyStateChanged_WithSubscriber_FiresEvent()
    {
        // Arrange
        var eventFired = false;
        _service.OnStateChanged += () => eventFired = true;

        // Act
        _service.NotifyStateChanged();

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void NotifyStateChanged_MultipleSubscribers_FiresAllEvents()
    {
        // Arrange
        var count = 0;
        _service.OnStateChanged += () => count++;
        _service.OnStateChanged += () => count++;
        _service.OnStateChanged += () => count++;

        // Act
        _service.NotifyStateChanged();

        // Assert
        count.Should().Be(3);
    }

    #endregion

    #region HasResults Tests

    [Fact]
    public void HasResults_WithK8sResults_ReturnsTrue()
    {
        // Arrange
        _service.SetK8sResults(CreateK8sSizingResult());

        // Assert
        _service.HasResults.Should().BeTrue();
    }

    [Fact]
    public void HasResults_WithVMResults_ReturnsTrue()
    {
        // Arrange
        _service.SetVMResults(CreateVMSizingResult());

        // Assert
        _service.HasResults.Should().BeTrue();
    }

    [Fact]
    public void HasResults_WithNoResults_ReturnsFalse()
    {
        // Assert
        _service.HasResults.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private K8sSizingResult CreateK8sSizingResult()
    {
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            Configuration = new K8sSizingInput(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 20,
                TotalCpu = 160,
                TotalRam = 320
            }
        };
    }

    private VMSizingResult CreateVMSizingResult()
    {
        return new VMSizingResult
        {
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 10,
                TotalCpu = 80,
                TotalRam = 160
            }
        };
    }

    private CostEstimate CreateCostEstimate()
    {
        return new CostEstimate
        {
            MonthlyTotal = 5000m,
            Provider = CloudProvider.AWS
        };
    }

    #endregion
}
