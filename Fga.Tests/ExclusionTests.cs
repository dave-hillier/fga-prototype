using Fga.Api.Core;

namespace Fga.Tests;

public class ExclusionTests
{
    // Model: doc has "viewer" = editor BUT NOT blocked
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
                    { "blocked", new Relation { This = new This() } },
                    {
                        "viewer", new Relation
                        {
                            Exclusion = new Exclusion
                            {
                                Base = new Child { ComputedUserset = new ComputedUserset { Relation = "editor" } },
                                Subtract = new Child { ComputedUserset = new ComputedUserset { Relation = "blocked" } }
                            }
                        }
                    }
                }
            }
        }
    };

    [Fact]
    public void ExclusionGrantsAccessWhenBaseMetAndNotSubtracted()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(RelationTuple.Parse("doc:123#editor@dave"));

        Assert.True(system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "123")));
    }

    [Fact]
    public void ExclusionDeniesAccessWhenBlocked()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(
            RelationTuple.Parse("doc:123#editor@dave"),
            RelationTuple.Parse("doc:123#blocked@dave")
        );

        // dave is editor but also blocked — exclusion removes access
        Assert.False(system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "123")));
    }

    [Fact]
    public void ExclusionDeniesWhenBaseNotMet()
    {
        var system = new AuthorizationSystem(_model);

        // dave is not editor at all
        Assert.False(system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "123")));
    }

    [Fact]
    public void ExclusionOnlyAffectsBlockedUser()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(
            RelationTuple.Parse("doc:123#editor@dave"),
            RelationTuple.Parse("doc:123#editor@andrew"),
            RelationTuple.Parse("doc:123#blocked@dave")
        );

        // dave blocked, andrew not
        Assert.False(system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "123")));
        Assert.True(system.Check(new User.UserId("andrew"), "viewer", new RelationObject("doc", "123")));
    }
}
