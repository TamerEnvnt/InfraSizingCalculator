using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

/// <summary>
/// Tests for HorizontalSlider generic component - Left-to-right navigation slider
/// </summary>
public class HorizontalSliderTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void HorizontalSlider_RendersContainer()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item)));

        // Assert
        cut.Find(".h-slider").Should().NotBeNull();
    }

    [Fact]
    public void HorizontalSlider_AppliesAdditionalClass()
    {
        // Arrange
        var items = new[] { "Item1" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.AdditionalClass, "custom-slider"));

        // Assert
        cut.Find(".h-slider").ClassList.Should().Contain("custom-slider");
    }

    [Fact]
    public void HorizontalSlider_RendersSliderContainer()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item)));

        // Assert
        cut.Find(".h-slider-container").Should().NotBeNull();
    }

    [Fact]
    public void HorizontalSlider_RendersItemContent()
    {
        // Arrange
        var items = new[] { "TestItem" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "test-content");
                builder.AddContent(2, item);
                builder.CloseElement();
            }));

        // Assert
        cut.Find(".test-content").TextContent.Should().Be("TestItem");
    }

    #endregion

    #region Header Tests

    [Fact]
    public void HorizontalSlider_ShowsHeader_WhenShowHeaderIsTrue()
    {
        // Arrange
        var items = new[] { "Item1" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowHeader, true)
            .Add(p => p.HeaderContent, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "test-header");
                builder.AddContent(2, "Header Content");
                builder.CloseElement();
            }));

        // Assert
        cut.Find(".h-slider-header").Should().NotBeNull();
        cut.Find(".test-header").TextContent.Should().Be("Header Content");
    }

    [Fact]
    public void HorizontalSlider_HidesHeader_WhenShowHeaderIsFalse()
    {
        // Arrange
        var items = new[] { "Item1" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowHeader, false)
            .Add(p => p.HeaderContent, builder => builder.AddContent(0, "Header")));

        // Assert
        cut.FindAll(".h-slider-header").Should().BeEmpty();
    }

    [Fact]
    public void HorizontalSlider_HidesHeader_WhenHeaderContentIsNull()
    {
        // Arrange
        var items = new[] { "Item1" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowHeader, true));

        // Assert
        cut.FindAll(".h-slider-header").Should().BeEmpty();
    }

    #endregion

    #region Navigation Dots Tests

    [Fact]
    public void HorizontalSlider_ShowsNavDots_WhenShowNavDotsIsTrue()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowNavDots, true));

        // Assert
        cut.Find(".h-slider-nav-dots").Should().NotBeNull();
        cut.FindAll(".nav-dot").Should().HaveCount(3);
    }

    [Fact]
    public void HorizontalSlider_HidesNavDots_WhenShowNavDotsIsFalse()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowNavDots, false));

        // Assert
        cut.FindAll(".h-slider-nav-dots").Should().BeEmpty();
    }

    [Fact]
    public void HorizontalSlider_MarksCurrentNavDotAsActive()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowNavDots, true)
            .Add(p => p.CurrentIndex, 1));

        // Assert
        var navDots = cut.FindAll(".nav-dot");
        navDots[0].ClassList.Should().NotContain("active");
        navDots[1].ClassList.Should().Contain("active");
        navDots[2].ClassList.Should().NotContain("active");
    }

    [Fact]
    public void HorizontalSlider_UsesItemTitleFunc_ForNavDotTitle()
    {
        // Arrange
        var items = new[] { "A", "B" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowNavDots, true)
            .Add(p => p.ItemTitleFunc, item => $"Title_{item}"));

        // Assert
        var navDots = cut.FindAll(".nav-dot");
        navDots[0].GetAttribute("title").Should().Be("Title_A");
        navDots[1].GetAttribute("title").Should().Be("Title_B");
    }

    [Fact]
    public void HorizontalSlider_UsesCustomNavDotTemplate()
    {
        // Arrange
        var items = new[] { "Item1" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowNavDots, true)
            .Add(p => p.NavDotTemplate, item => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-dot");
                builder.AddContent(2, $"Custom_{item}");
                builder.CloseElement();
            }));

        // Assert
        cut.Find(".custom-dot").TextContent.Should().Be("Custom_Item1");
    }

    #endregion

    #region Progress Indicator Tests

    [Fact]
    public void HorizontalSlider_ShowsProgress_WhenShowProgressIsTrue()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowProgress, true)
            .Add(p => p.CurrentIndex, 1));

        // Assert
        cut.Find(".h-slider-progress").Should().NotBeNull();
        cut.Find(".progress-text").TextContent.Should().Be("2 of 3");
    }

    [Fact]
    public void HorizontalSlider_HidesProgress_WhenShowProgressIsFalse()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowProgress, false));

        // Assert
        cut.FindAll(".h-slider-progress").Should().BeEmpty();
    }

    [Fact]
    public void HorizontalSlider_CalculatesProgressBarWidth()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3", "Item4" };

        // Act - at index 1 (2 of 4 = 50%)
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowProgress, true)
            .Add(p => p.CurrentIndex, 1));

        // Assert - (1+1)/4 * 100 = 50%
        var progressFill = cut.Find(".progress-fill");
        progressFill.GetAttribute("style").Should().Contain("width: 50%");
    }

    #endregion

    #region Navigation Button Tests

    [Fact]
    public void HorizontalSlider_DisablesPrevButton_OnFirstItem()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, 0));

        // Assert
        var prevBtn = cut.Find(".slider-btn.prev");
        prevBtn.HasAttribute("disabled").Should().BeTrue();
        prevBtn.ClassList.Should().Contain("disabled");
    }

    [Fact]
    public void HorizontalSlider_EnablesPrevButton_NotOnFirstItem()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, 1));

        // Assert
        var prevBtn = cut.Find(".slider-btn.prev");
        prevBtn.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void HorizontalSlider_DisablesNextButton_OnLastItem()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, 2));

        // Assert
        var nextBtn = cut.Find(".slider-btn.next");
        nextBtn.HasAttribute("disabled").Should().BeTrue();
        nextBtn.ClassList.Should().Contain("disabled");
    }

    [Fact]
    public void HorizontalSlider_EnablesNextButton_NotOnLastItem()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, 1));

        // Assert
        var nextBtn = cut.Find(".slider-btn.next");
        nextBtn.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void HorizontalSlider_ShowsPrevLabel_WhenProvided()
    {
        // Arrange
        var items = new[] { "First", "Second", "Third" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, 1)
            .Add(p => p.PrevLabel, "Back")
            .Add(p => p.ItemTitleFunc, item => item));

        // Assert
        var prevBtn = cut.Find(".slider-btn.prev");
        prevBtn.TextContent.Should().Contain("First"); // Previous item title
    }

    [Fact]
    public void HorizontalSlider_ShowsNextLabel_WhenProvided()
    {
        // Arrange
        var items = new[] { "First", "Second", "Third" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, 1)
            .Add(p => p.NextLabel, "Forward")
            .Add(p => p.ItemTitleFunc, item => item));

        // Assert
        var nextBtn = cut.Find(".slider-btn.next");
        nextBtn.TextContent.Should().Contain("Third"); // Next item title
    }

    #endregion

    #region Navigation Action Tests

    [Fact]
    public async Task HorizontalSlider_ClickingNext_IncreasesIndex()
    {
        // Arrange
        int currentIndex = 0;
        var items = new[] { "Item1", "Item2", "Item3" };
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, currentIndex)
            .Add(p => p.CurrentIndexChanged, EventCallback.Factory.Create<int>(this, i => currentIndex = i)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".slider-btn.next").Click());

        // Assert
        currentIndex.Should().Be(1);
    }

    [Fact]
    public async Task HorizontalSlider_ClickingPrev_DecreasesIndex()
    {
        // Arrange
        int currentIndex = 2;
        var items = new[] { "Item1", "Item2", "Item3" };
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, currentIndex)
            .Add(p => p.CurrentIndexChanged, EventCallback.Factory.Create<int>(this, i => currentIndex = i)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".slider-btn.prev").Click());

        // Assert
        currentIndex.Should().Be(1);
    }

    [Fact]
    public async Task HorizontalSlider_ClickingNavDot_ChangesIndex()
    {
        // Arrange
        int currentIndex = 0;
        var items = new[] { "Item1", "Item2", "Item3" };
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowNavDots, true)
            .Add(p => p.CurrentIndex, currentIndex)
            .Add(p => p.CurrentIndexChanged, EventCallback.Factory.Create<int>(this, i => currentIndex = i)));

        // Act - Click the third nav dot (index 2)
        var navDots = cut.FindAll(".nav-dot");
        await cut.InvokeAsync(() => navDots[2].Click());

        // Assert
        currentIndex.Should().Be(2);
    }

    [Fact]
    public async Task HorizontalSlider_InvokesOnItemChanged_WhenNavigating()
    {
        // Arrange
        string? changedItem = null;
        var items = new[] { "Item1", "Item2", "Item3" };
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.CurrentIndex, 0)
            .Add(p => p.OnItemChanged, EventCallback.Factory.Create<string>(this, item => changedItem = item)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".slider-btn.next").Click());

        // Assert
        changedItem.Should().Be("Item2");
    }

    #endregion

    #region Index Clamping Tests

    [Fact]
    public void HorizontalSlider_ClampsIndex_WhenExceedsItemCount()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };

        // Act - Try to set index beyond bounds
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowProgress, true)
            .Add(p => p.CurrentIndex, 10)); // Way beyond bounds

        // Assert - Should clamp to last valid index
        cut.Find(".progress-text").TextContent.Should().Be("2 of 2");
    }

    [Fact]
    public void HorizontalSlider_ClampsNegativeIndex_ToZero()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowProgress, true)
            .Add(p => p.CurrentIndex, -5)); // Negative

        // Assert
        cut.Find(".progress-text").TextContent.Should().Be("1 of 2");
    }

    #endregion

    #region Item CSS Class Tests

    [Fact]
    public void HorizontalSlider_AppliesItemCssClass_ToSlidePanel()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ItemCssClassFunc, item => $"class-{item}"));

        // Assert
        cut.Find(".slide-panel").ClassList.Should().Contain("class-Item1");
    }

    [Fact]
    public void HorizontalSlider_AppliesItemCssClass_ToNavDots()
    {
        // Arrange
        var items = new[] { "A", "B" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowNavDots, true)
            .Add(p => p.ItemCssClassFunc, item => $"class-{item}"));

        // Assert
        var navDots = cut.FindAll(".nav-dot");
        navDots[0].ClassList.Should().Contain("class-A");
        navDots[1].ClassList.Should().Contain("class-B");
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void HorizontalSlider_DefaultShowHeader_IsTrue()
    {
        // Arrange
        var items = new[] { "Item1" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.HeaderContent, builder => builder.AddContent(0, "Header")));

        // Assert - Header should be visible by default
        cut.FindAll(".h-slider-header").Should().HaveCount(1);
    }

    [Fact]
    public void HorizontalSlider_DefaultShowNavDots_IsTrue()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item)));

        // Assert
        cut.FindAll(".h-slider-nav-dots").Should().HaveCount(1);
    }

    [Fact]
    public void HorizontalSlider_DefaultShowProgress_IsTrue()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item)));

        // Assert
        cut.FindAll(".h-slider-progress").Should().HaveCount(1);
    }

    [Fact]
    public void HorizontalSlider_DefaultCurrentIndex_IsZero()
    {
        // Arrange
        var items = new[] { "Item1", "Item2", "Item3" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowProgress, true));

        // Assert
        cut.Find(".progress-text").TextContent.Should().Be("1 of 3");
    }

    #endregion

    #region Empty Items Tests

    [Fact]
    public void HorizontalSlider_WithEmptyItems_DoesNotThrow()
    {
        // Arrange
        var items = Array.Empty<string>();

        // Act
        var action = () => RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item)));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void HorizontalSlider_WithEmptyItems_ShowsNoContent()
    {
        // Arrange
        var items = Array.Empty<string>();

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item)));

        // Assert
        cut.FindAll(".slide-panel").Should().BeEmpty();
        cut.FindAll(".nav-dot").Should().BeEmpty();
    }

    #endregion

    #region Single Item Tests

    [Fact]
    public void HorizontalSlider_WithSingleItem_DisablesBothButtons()
    {
        // Arrange
        var items = new[] { "OnlyItem" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item)));

        // Assert
        cut.Find(".slider-btn.prev").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".slider-btn.next").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void HorizontalSlider_WithSingleItem_ShowsFullProgress()
    {
        // Arrange
        var items = new[] { "OnlyItem" };

        // Act
        var cut = RenderComponent<HorizontalSlider<string>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ItemTemplate, item => builder => builder.AddContent(0, item))
            .Add(p => p.ShowProgress, true));

        // Assert
        cut.Find(".progress-text").TextContent.Should().Be("1 of 1");
        cut.Find(".progress-fill").GetAttribute("style").Should().Contain("width: 100%");
    }

    #endregion
}
