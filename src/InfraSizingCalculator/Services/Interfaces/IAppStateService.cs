using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Centralized state management service for the application.
/// Follows SPA best practices:
/// - Single source of truth for all UI state
/// - Results persist until explicit reset (not on navigation)
/// - Event-based state change notifications
/// </summary>
public interface IAppStateService
{
    #region Navigation State

    /// <summary>
    /// Current active section in the UI.
    /// Values: "config", "sizing", "cost", "growth"
    /// </summary>
    string ActiveSection { get; set; }

    /// <summary>
    /// Currently expanded card/section in the main view (null = none expanded).
    /// Used for progressive disclosure pattern.
    /// </summary>
    string? ExpandedCard { get; set; }

    /// <summary>
    /// Whether results panel is visible (has results to show)
    /// </summary>
    bool HasResults { get; }

    #endregion

    #region Results State (Persists until explicit reset)

    /// <summary>
    /// K8s sizing calculation results.
    /// NEVER cleared on navigation - only on explicit ResetResults()
    /// </summary>
    K8sSizingResult? K8sResults { get; set; }

    /// <summary>
    /// VM sizing calculation results.
    /// NEVER cleared on navigation - only on explicit ResetResults()
    /// </summary>
    VMSizingResult? VMResults { get; set; }

    /// <summary>
    /// K8s cost estimation results.
    /// NEVER cleared on navigation - only on explicit ResetResults()
    /// </summary>
    CostEstimate? K8sCostEstimate { get; set; }

    /// <summary>
    /// VM cost estimation results.
    /// NEVER cleared on navigation - only on explicit ResetResults()
    /// </summary>
    CostEstimate? VMCostEstimate { get; set; }

    #endregion

    #region Summary Properties (Computed from results)

    /// <summary>
    /// Total nodes across all environments
    /// </summary>
    int TotalNodes { get; }

    /// <summary>
    /// Total vCPU across all environments
    /// </summary>
    double TotalCPU { get; }

    /// <summary>
    /// Total RAM in GB across all environments
    /// </summary>
    double TotalRAM { get; }

    /// <summary>
    /// Monthly cost estimate (from K8s or VM cost estimate)
    /// </summary>
    decimal MonthlyEstimate { get; }

    /// <summary>
    /// Cost provider name for display
    /// </summary>
    string? CostProvider { get; }

    #endregion

    #region Events

    /// <summary>
    /// Fired when any state changes. Components should subscribe to this
    /// to re-render when state changes.
    /// </summary>
    event Action? OnStateChanged;

    #endregion

    #region Methods

    /// <summary>
    /// Notify all subscribers that state has changed.
    /// Call this after modifying state properties.
    /// </summary>
    void NotifyStateChanged();

    /// <summary>
    /// Navigate to a specific section.
    /// Does NOT reset any results.
    /// </summary>
    void NavigateToSection(string section);

    /// <summary>
    /// Set K8s results and auto-navigate to sizing section.
    /// </summary>
    void SetK8sResults(K8sSizingResult results);

    /// <summary>
    /// Set VM results and auto-navigate to sizing section.
    /// </summary>
    void SetVMResults(VMSizingResult results);

    /// <summary>
    /// Set K8s cost estimate. Does NOT change navigation.
    /// </summary>
    void SetK8sCostEstimate(CostEstimate estimate);

    /// <summary>
    /// Set VM cost estimate. Does NOT change navigation.
    /// </summary>
    void SetVMCostEstimate(CostEstimate estimate);

    /// <summary>
    /// Reset only the results (K8s, VM, Cost).
    /// Called when user explicitly wants to clear results.
    /// </summary>
    void ResetResults();

    /// <summary>
    /// Reset all state including navigation.
    /// Called for complete reset.
    /// </summary>
    void ResetAll();

    /// <summary>
    /// Toggle expansion of a card/section.
    /// If already expanded, collapses. If collapsed, expands.
    /// </summary>
    void ToggleExpand(string cardId);

    /// <summary>
    /// Check if a specific card is expanded
    /// </summary>
    bool IsExpanded(string cardId);

    #endregion
}
