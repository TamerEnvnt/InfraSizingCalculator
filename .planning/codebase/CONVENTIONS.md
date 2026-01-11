# Coding Conventions

**Analysis Date:** 2026-01-11

## Naming Patterns

**Files:**
- PascalCase.cs for all C# files
- PascalCase.razor for Blazor components
- {ClassName}Tests.cs for test files
- I{Name}.cs for interfaces

**Functions:**
- PascalCase for public methods
- camelCase for private methods
- Get{Property} for property-like methods
- Calculate{Thing} for calculation methods

**Variables:**
- camelCase for local variables
- _camelCase for private fields
- PascalCase for properties

**Types:**
- PascalCase for all types
- I prefix for interfaces (IVMSizingService)
- {Thing}Config for configuration classes
- {Thing}Result for result classes

## Code Style

**Formatting:**
- 4 space indentation
- Braces on new lines
- File-scoped namespaces

**Linting:**
- .NET analyzers enabled
- Nullable reference types enabled

## Import Organization

**Order:**
1. System namespaces
2. Microsoft namespaces
3. Third-party packages
4. Project namespaces

## Error Handling

**Patterns:**
- ArgumentNullException for null inputs
- Validation before processing
- Clear error messages

**Example:**
```csharp
public VMSizingResult Calculate(VMSizingInput input)
{
    ArgumentNullException.ThrowIfNull(input);
    // ... validation and calculation
}
```

## Testing Patterns

**Test Structure:**
```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var input = CreateTestInput();

    // Act
    var result = _sut.Method(input);

    // Assert
    result.Should().NotBeNull();
}
```

**Theory Tests:**
```csharp
[Theory]
[InlineData(HAPattern.None, 1.0)]
[InlineData(HAPattern.ActiveActive, 2.0)]
public void GetHAMultiplier_Pattern_ReturnsMultiplier(HAPattern pattern, double expected)
{
    var result = _sut.GetHAMultiplier(pattern);
    result.Should().Be(expected);
}
```

## Comments

**When to Comment:**
- Business rules (e.g., "Java needs 1.5x memory")
- Complex calculations
- Non-obvious logic

**XML Documentation:**
- Required for public APIs
- Include `<summary>`, `<param>`, `<returns>`

## Function Design

**Size:**
- Keep under 50 lines
- Extract helpers for complex logic

**Parameters:**
- Use strongly-typed inputs (VMSizingInput, not loose params)
- Return strongly-typed results

---

*Convention analysis: 2026-01-11*
*Update when patterns change*
