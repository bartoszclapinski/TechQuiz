using FluentAssertions;
using TechQuiz.Domain;

namespace TechQuiz.Domain.Tests;

public class TrackTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        var id = Guid.NewGuid();

        var track = new Track(id, ".NET", "The .NET platform and its ecosystem", "dotnet", position: 0);

        track.Id.Should().Be(id);
        track.Name.Should().Be(".NET");
        track.Description.Should().Be("The .NET platform and its ecosystem");
        track.IconCode.Should().Be("dotnet");
        track.Position.Should().Be(0);
    }
}
