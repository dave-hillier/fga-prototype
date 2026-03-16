using Fga.Api.Core;

namespace Fga.Tests;

public class ExpandTests
{
    private readonly AuthorizationModel _model = new()
    {
        TypeDefinitions = new[]
        {
            new TypeDefinition
            {
                Type = "team",
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
                    { "owner", new Relation { This = new This() } },
                    {
                        "editor", new Relation
                        {
                            Union = new Union
                            {
                                Child = new Child[]
                                {
                                    new() { This = new This() },
                                    new() { ComputedUserset = new ComputedUserset { Relation = "owner" } }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    [Fact]
    public void ExpandReturnsDirectUsers()
    {
        var system = new AuthorizationSystem(_model);
        system.Write(
            RelationTuple.Parse("doc:123#owner@dave"),
            RelationTuple.Parse("doc:123#owner@andrew")
        );

        var tree = system.Expand("owner", new RelationObject("doc", "123"));

        // Should be a leaf with both users
        var leaf = Assert.IsType<UsersetTree.Leaf>(tree);
        Assert.Equal(2, leaf.Users.Length);
    }

    [Fact]
    public void ExpandReturnsUnionTree()
    {
        var system = new AuthorizationSystem(_model);
        system.Write(
            RelationTuple.Parse("doc:123#editor@kuba"),
            RelationTuple.Parse("doc:123#owner@dave")
        );

        var tree = system.Expand("editor", new RelationObject("doc", "123"));

        // Should be a union node (direct editors + computed from owners)
        var union = Assert.IsType<UsersetTree.UnionNode>(tree);
        Assert.Equal(2, union.Children.Length);
    }

    [Fact]
    public void ExpandShowsGroupMembership()
    {
        var system = new AuthorizationSystem(_model);
        system.Write(
            RelationTuple.Parse("team:labs#member@dave"),
            RelationTuple.Parse("team:labs#member@kuba"),
            RelationTuple.Parse("doc:123#editor@team:labs#member")
        );

        var tree = system.Expand("editor", new RelationObject("doc", "123"));

        // The tree should expand the team:labs#member userset
        var union = Assert.IsType<UsersetTree.UnionNode>(tree);
        Assert.True(union.Children.Length >= 1);
    }

    [Fact]
    public void ExpandEmptyRelation()
    {
        var system = new AuthorizationSystem(_model);

        var tree = system.Expand("owner", new RelationObject("doc", "123"));

        var leaf = Assert.IsType<UsersetTree.Leaf>(tree);
        Assert.Empty(leaf.Users);
    }
}
