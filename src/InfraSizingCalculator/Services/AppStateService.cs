using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Centralized state management service for the application.
/// Implements SPA best practices:
/// - Single source of truth for all UI state
/// - Results persist until explicit reset (not on navigation)
/// - Event-based state change notifications
/// </summary>
public class AppStateService : IAppStateService
{
    #region Landing Page State

    /// <inheritdoc />
    public bool HasStartedScenario { get; set; }

    /// <inheritdoc />
    public Guid? CurrentScenarioId { get; set; }

    /// <inheritdoc />
    public bool IsViewingLanding { get; set; } = true;

    #endregion

    #region Navigation State

    private string _activeSection = "config";

    /// <inheritdoc />
    public string ActiveSection
    {
        get => _activeSection;
        set
        {
            if (_activeSection != value)
            {
                _activeSection = value;
                // Don't clear results on navigation change - that's the key fix!
            }
        }
    }

    /// <inheritdoc />
    public string? ExpandedCard { get; set; }

    /// <inheritdoc />
    public bool HasResults => K8sResults != null || VMResults != null;

    #endregion

    #region Results State (Persists until explicit reset)

    /// <inheritdoc />
    public K8sSizingResult? K8sResults { get; set; }

    /// <inheritdoc />
    public VMSizingResult? VMResults { get; set; }

    /// <inheritdoc />
    public CostEstimate? K8sCostEstimate { get; set; }

    /// <inheritdoc />
    public CostEstimate? VMCostEstimate { get; set; }

    #endregion

    #region Summary Properties (Computed from results)

    /// <inheritdoc />
    public int TotalNodes
    {
        get
        {
            if (K8sResults != null)
                return K8sResults.GrandTotal.TotalNodes;
            if (VMResults != null)
                return VMResults.GrandTotal.TotalVMs;
            return 0;
        }
    }

    /// <inheritdoc />
    public double TotalCPU
    {
        get
        {
            if (K8sResults != null)
                return K8sResults.GrandTotal.TotalCpu;
            if (VMResults != null)
                return VMResults.GrandTotal.TotalCpu;
            return 0;
        }
    }

    /// <inheritdoc />
    public double TotalRAM
    {
        get
        {
            if (K8sResults != null)
                return K8sResults.GrandTotal.TotalRam;
            if (VMResults != null)
                return VMResults.GrandTotal.TotalRam;
            return 0;
        }
    }

    /// <inheritdoc />
    public decimal MonthlyEstimate
    {
        get
        {
            if (K8sCostEstimate != null)
                return K8sCostEstimate.MonthlyTotal;
            if (VMCostEstimate != null)
                return VMCostEstimate.MonthlyTotal;
            return 0;
        }
    }

    /// <inheritdoc />
    public string? CostProvider
    {
        get
        {
            if (K8sCostEstimate != null)
                return K8sCostEstimate.Provider.ToString();
            if (VMCostEstimate != null)
                return VMCostEstimate.Provider.ToString();
            return null;
        }
    }

    #endregion

    #region Events

    /// <inheritdoc />
    public event Action? OnStateChanged;

    #endregion

    #region Methods

    /// <inheritdoc />
    public void NotifyStateChanged() => OnStateChanged?.Invoke();

    /// <inheritdoc />
    public void NavigateToSection(string section)
    {
        // Validate section
        var validSections = new[] { "config", "sizing", "cost", "growth" };
        if (!validSections.Contains(section))
        {
            section = "config";
        }

        ActiveSection = section;
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public void SetK8sResults(K8sSizingResult results)
    {
        K8sResults = results;
        // Clear any previous VM results when we get K8s results
        VMResults = null;
        VMCostEstimate = null;
        // Navigate to sizing section to show results
        ActiveSection = "sizing";
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public void SetVMResults(VMSizingResult results)
    {
        VMResults = results;
        // Clear any previous K8s results when we get VM results
        K8sResults = null;
        K8sCostEstimate = null;
        // Navigate to sizing section to show results
        ActiveSection = "sizing";
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public void SetK8sCostEstimate(CostEstimate estimate)
    {
        K8sCostEstimate = estimate;
        // Do NOT change navigation - user might want to stay on current view
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public void SetVMCostEstimate(CostEstimate estimate)
    {
        VMCostEstimate = estimate;
        // Do NOT change navigation - user might want to stay on current view
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public void ResetResults()
    {
        K8sResults = null;
        VMResults = null;
        K8sCostEstimate = null;
        VMCostEstimate = null;
        ExpandedCard = null;
        // Navigate back to config when results are cleared
        ActiveSection = "config";
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public void ResetAll()
    {
        ResetResults();
        // Reset landing page state
        HasStartedScenario = false;
        CurrentScenarioId = null;
        IsViewingLanding = true;
        // Reset navigation to initial state
        ActiveSection = "config";
        ExpandedCard = null;
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public void ToggleExpand(string cardId)
    {
        if (ExpandedCard == cardId)
        {
            ExpandedCard = null; // Collapse if already expanded
        }
        else
        {
            ExpandedCard = cardId; // Expand the new card
        }
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public bool IsExpanded(string cardId)
    {
        return ExpandedCard == cardId;
    }

    #endregion
}
