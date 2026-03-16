using Fga.Api.Core;

namespace Fga.Tests;

public class IntersectionTests
{
    // Model: doc has "viewer" requiring BOTH "approved" AND "member" of a group
    // This proves that intersection requires all children to be satisfied.
    private readonly AuthorizationModel _model = new()
    {
        TypeDefinitions = new[]
        {
            new TypeDefinition
            {
                Type = "group",
                Relations = new Dictionary<string, Relation>
                {
                    { "member", new Relation { This = new This() } }
                }
            },
            new TypeDefinition
            {
                Type = "doc",
                Relations = new Dictionary<string, Relation>
                {
                    { "approved", new Relation { This = new This() } },
                    {
                        "writer", new Relation { This = new This() }
                    },
                    {
                        // viewer requires: (direct OR writer) AND approved
                        "viewer", new Relation
                        {
                            Intersection = new Intersection
                            {
                                Child = new Child[]
                                {
                                    new()
                                    {
                                        ComputedUserset = new ComputedUserset { Relation = "writer" }
                                    },
                                    new()
                                    {
                                        ComputedUserset = new ComputedUserset { Relation = "approved" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    [Fact]
    public void IntersectionGrantsAccessWhenAllConditionsMet()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(
            RelationTuple.Parse("doc:123#writer@dave"),
            RelationTuple.Parse("doc:123#approved@dave")
        );

        Assert.True(system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "123")));
    }

    [Fact]
    public void IntersectionDeniesWhenOnlyWriterNotApproved()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(
            RelationTuple.Parse("doc:123#writer@dave")
        );

        // dave is writer but not approved — intersection fails
        Assert.False(system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "123")));
    }

    [Fact]
    public void IntersectionDeniesWhenOnlyApprovedNotWriter()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(
            RelationTuple.Parse("doc:123#approved@dave")
        );

        // dave is approved but not writer — intersection fails
        Assert.False(system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "123")));
    }

    [Fact]
    public void IntersectionDeniesUnrelatedUser()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(
            RelationTuple.Parse("doc:123#writer@dave"),
            RelationTuple.Parse("doc:123#approved@dave")
        );

        Assert.False(system.Check(new User.UserId("andrew"), "viewer", new RelationObject("doc", "123")));
    }
}
