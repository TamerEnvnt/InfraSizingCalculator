using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for ExportButtons component
/// </summary>
public class ExportButtonsTests : TestContext
{
    [Fact]
    public void ExportButtons_RendersCsvButton()
    {
        // Act
        var cut = RenderComponent<ExportButtons>();

        // Assert
        cut.FindAll("button.csv").Should().HaveCount(1);
        cut.Find("button.csv").TextContent.Should().Contain("Export CSV");
    }

    [Fact]
    public void ExportButtons_RendersJsonButton()
    {
        // Act
        var cut = RenderComponent<ExportButtons>();

        // Assert
        cut.FindAll("button.json").Should().HaveCount(1);
        cut.Find("button.json").TextContent.Should().Contain("Export JSON");
    }

    [Fact]
    public void ExportButtons_RendersExcelButton()
    {
        // Act
        var cut = RenderComponent<ExportButtons>();

        // Assert
        cut.FindAll("button.excel").Should().HaveCount(1);
        cut.Find("button.excel").TextContent.Should().Contain("Export Excel");
    }

    [Fact]
    public void ExportButtons_RendersSaveProfileButton_WhenShowSaveProfileIsTrue()
    {
        // Act
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.ShowSaveProfile, true));

        // Assert
        cut.FindAll("button.profile").Should().HaveCount(1);
        cut.Find("button.profile").TextContent.Should().Contain("Save Profile");
    }

    [Fact]
    public void ExportButtons_HidesSaveProfileButton_WhenShowSaveProfileIsFalse()
    {
        // Act
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.ShowSaveProfile, false));

        // Assert
        cut.FindAll("button.profile").Should().BeEmpty();
    }

    [Fact]
    public void ExportButtons_RendersDiagramButton_WhenShowDiagramIsTrue()
    {
        // Act
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.ShowDiagram, true));

        // Assert
        cut.FindAll("button.diagram").Should().HaveCount(1);
        cut.Find("button.diagram").TextContent.Should().Contain("Export Diagram");
    }

    [Fact]
    public void ExportButtons_HidesDiagramButton_WhenShowDiagramIsFalse()
    {
        // Act
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.ShowDiagram, false));

        // Assert
        cut.FindAll("button.diagram").Should().BeEmpty();
    }

    [Fact]
    public void ExportButtons_InvokesOnExportCsv_WhenCsvButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.OnExportCsv, () => clicked = true));

        // Act
        cut.Find("button.csv").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void ExportButtons_InvokesOnExportJson_WhenJsonButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.OnExportJson, () => clicked = true));

        // Act
        cut.Find("button.json").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void ExportButtons_InvokesOnExportExcel_WhenExcelButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.OnExportExcel, () => clicked = true));

        // Act
        cut.Find("button.excel").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void ExportButtons_InvokesOnSaveProfile_WhenProfileButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.ShowSaveProfile, true)
            .Add(p => p.OnSaveProfile, () => clicked = true));

        // Act
        cut.Find("button.profile").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void ExportButtons_InvokesOnExportDiagram_WhenDiagramButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.ShowDiagram, true)
            .Add(p => p.OnExportDiagram, () => clicked = true));

        // Act
        cut.Find("button.diagram").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void ExportButtons_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-export"));

        // Assert
        cut.Find(".export-buttons").ClassList.Should().Contain("custom-export");
    }

    [Fact]
    public void ExportButtons_RendersAllButtons_ByDefault()
    {
        // Act
        var cut = RenderComponent<ExportButtons>();

        // Assert - Default: ShowSaveProfile=true, ShowDiagram=true
        cut.FindAll("button.export-btn").Should().HaveCount(5);
    }

    [Fact]
    public void ExportButtons_RendersOnlyCoreButtons_WhenOptionalButtonsDisabled()
    {
        // Act
        var cut = RenderComponent<ExportButtons>(parameters => parameters
            .Add(p => p.ShowSaveProfile, false)
            .Add(p => p.ShowDiagram, false));

        // Assert - Only CSV, JSON, Excel
        cut.FindAll("button.export-btn").Should().HaveCount(3);
    }
}
