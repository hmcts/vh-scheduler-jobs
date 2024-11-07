using System;
using FluentAssertions;
using NUnit.Framework;
using SchedulerJobs.Services.Extensions;

namespace SchedulerJobs.Services.UnitTests.Extensions;

public class StringExtensionsTests
{
    [Test]
    public void Capitalise_WithNullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((string)null).Capitalise());
    }

    [Test]
    public void Capitalise_WithEmptyStringInput_ThrowsArgumentException()
    {
        const string input = "";
        Assert.Throws<ArgumentException>(() => input.Capitalise());
    }

    [Test]
    public void Capitalise_WithValidInput_CapitalisesInput()
    {
        // Arrange
        const string input = "string";
        
        // Act
        var result = input.Capitalise();

        // Assert
        result.Should().Be("String");
    }
}