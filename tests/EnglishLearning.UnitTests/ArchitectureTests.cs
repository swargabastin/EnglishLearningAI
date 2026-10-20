using System.Reflection;
using EnglishLearning.Domain.Common;
using FluentAssertions;
using Xunit;

namespace EnglishLearning.UnitTests;

public class ArchitectureTests
{
    [Fact]
    public void DomainAssembly_ShouldNotReference_OtherApplicationLayers()
    {
        // Arrange
        var domainAssembly = typeof(BaseEntity).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        // Assert
        referencedAssemblies.Should().NotContain("EnglishLearning.Application");
        referencedAssemblies.Should().NotContain("EnglishLearning.Infrastructure");
        referencedAssemblies.Should().NotContain("EnglishLearning.Api");
        referencedAssemblies.Should().NotContain("Microsoft.EntityFrameworkCore");
        referencedAssemblies.Should().NotContain("Npgsql");
    }

    [Fact]
    public void BaseEntity_ShouldInitializeWith_ValidGuidAndUtcTimestamp()
    {
        // Arrange & Act
        var testEntity = new TestEntity();

        // Assert
        testEntity.Id.Should().NotBeEmpty();
        testEntity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    private class TestEntity : BaseEntity { }
}
