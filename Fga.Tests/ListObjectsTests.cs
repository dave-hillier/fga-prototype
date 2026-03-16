using Fga.Api.Core;

namespace Fga.Tests;

public class ListObjectsTests
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
                    },
                    { "viewer", new Relation { This = new This() } }
                }
            }
        }
    };

    [Fact]
    public void ListDirectlyOwnedDocs()
    {
        var system = new AuthorizationSystem(_model);
        system.Write(
            RelationTuple.Parse("doc:a#owner@dave"),
            RelationTuple.Parse("doc:b#owner@dave"),
            RelationTuple.Parse("doc:c#owner@andrew")
        );

        var result = system.ListObjects(new User.UserId("dave"), "owner", "doc");

        Assert.Equal(2, result.Count);
        Assert.Contains(new RelationObject("doc", "a"), result);
        Assert.Contains(new RelationObject("doc", "b"), result);
    }

    [Fact]
    public void ListIncludesComputedRelations()
    {
        var system = new AuthorizationSystem(_model);
        system.Write(
            RelationTuple.Parse("doc:a#owner@dave"),
            RelationTuple.Parse("doc:b#editor@dave")
        );

        // dave should be editor of both (owner of a implies editor, direct editor of b)
        var result = system.ListObjects(new User.UserId("dave"), "editor", "doc");

        Assert.Equal(2, result.Count);
        Assert.Contains(new RelationObject("doc", "a"), result);
        Assert.Contains(new RelationObject("doc", "b"), result);
    }

    [Fact]
    public void ListIncludesIndirectGroupAccess()
    {
        var system = new AuthorizationSystem(_model);
        system.Write(
            RelationTuple.Parse("team:labs#member@dave"),
            RelationTuple.Parse("doc:a#viewer@team:labs#member"),
            RelationTuple.Parse("doc:b#viewer@team:labs#member"),
            RelationTuple.Parse("doc:c#viewer@andrew")
        );

        var result = system.ListObjects(new User.UserId("dave"), "viewer", "doc");

        Assert.Equal(2, result.Count);
        Assert.Contains(new RelationObject("doc", "a"), result);
        Assert.Contains(new RelationObject("doc", "b"), result);
    }

    [Fact]
    public void ListReturnsEmptyWhenNoAccess()
    {
        var system = new AuthorizationSystem(_model);
        system.Write(
            RelationTuple.Parse("doc:a#owner@andrew")
        );

        var result = system.ListObjects(new User.UserId("dave"), "owner", "doc");

        Assert.Empty(result);
    }

    [Fact]
    public void ListFiltersToRequestedType()
    {
        var system = new AuthorizationSystem(_model);
        system.Write(
            RelationTuple.Parse("doc:a#owner@dave"),
            RelationTuple.Parse("team:labs#member@dave")
        );

        var docs = system.ListObjects(new User.UserId("dave"), "owner", "doc");
        var teams = system.ListObjects(new User.UserId("dave"), "member", "team");

        Assert.Single(docs);
        Assert.Single(teams);
        Assert.Equal(new RelationObject("doc", "a"), docs[0]);
        Assert.Equal(new RelationObject("team", "labs"), teams[0]);
    }
}
