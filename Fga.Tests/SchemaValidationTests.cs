using Fga.Api.Core;

namespace Fga.Tests;

public class SchemaValidationTests
{
    private readonly AuthorizationModel _model = new()
    {
        TypeDefinitions = new[]
        {
            new TypeDefinition
            {
                Type = "doc",
                Relations = new Dictionary<string, Relation>
                {
                    { "editor", new Relation { This = new This() } },
                    { "viewer", new Relation { This = new This() } }
                }
            }
        }
    };

    [Fact]
    public void ValidWriteSucceeds()
    {
        var system = new AuthorizationSystem(_model);

        var exception = Record.Exception(() =>
            system.ValidateWrite(RelationTuple.Parse("doc:123#editor@dave")));

        Assert.Null(exception);
    }

    [Fact]
    public void WriteWithUnknownTypeThrows()
    {
        var system = new AuthorizationSystem(_model);

        var ex = Assert.Throws<ValidationException>(() =>
            system.ValidateWrite(RelationTuple.Parse("folder:123#editor@dave")));

        Assert.Contains("Unknown type: folder", ex.Message);
    }

    [Fact]
    public void WriteWithUnknownRelationThrows()
    {
        var system = new AuthorizationSystem(_model);

        var ex = Assert.Throws<ValidationException>(() =>
            system.ValidateWrite(RelationTuple.Parse("doc:123#admin@dave")));

        Assert.Contains("Unknown relation: admin", ex.Message);
    }

    [Fact]
    public void ValidatesBatchWrites()
    {
        var system = new AuthorizationSystem(_model);

        var ex = Assert.Throws<ValidationException>(() =>
            system.ValidateWrite(
                RelationTuple.Parse("doc:123#editor@dave"),
                RelationTuple.Parse("doc:456#bogus@andrew")
            ));

        Assert.Contains("Unknown relation: bogus", ex.Message);
    }
}
