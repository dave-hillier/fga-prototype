using Fga.Api.Core;

namespace Fga.Tests;

public class TupleSetTests
{
    private readonly AuthorizationModel _model = new AuthorizationModel
    {
        TypeDefinitions = new[]
        {
            new TypeDefinition
            {
                Type = "doc",
                Relations = new Dictionary<string, Relation>
                {
                    {
                        "viewer", new Relation
                        {
                            Union = new Union
                            {
                                Child = new Child[]
                                {
                                    new() {This = new This()},
                                    new()
                                    {
                                        TupleToUserset = new TupleToUserset
                                        {
                                            Tupleset = new Tupleset {Relation = "parent"},
                                            ComputedUserset = new ComputedUserset
                                            {
                                                Relation = "owner"
                                            }
                                        },
                                    }
                                }
                            }
                        }
                    },
                    {"parent", new Relation {This = new This()}}
                }
            },
            new TypeDefinition
            {
                Type = "folder",
                Relations = new Dictionary<string, Relation>
                {
                    {"parent", new Relation {This = new This()}},
                    {
                        "owner", new Relation
                        {
                            Union = new Union
                            {
                                Child = new Child[]
                                {
                                    new() {This = new This()},
                                    new()
                                    {
                                        TupleToUserset = new TupleToUserset
                                        {
                                            Tupleset = new Tupleset {Relation = "parent"},
                                            ComputedUserset = new ComputedUserset
                                            {
                                                Relation = "owner"
                                            }
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            },
        }
    };

    [Fact]
    public void FolderOwnerIsEditor()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(
            RelationTuple.Parse("folder:456#owner@dave"),
            RelationTuple.Parse("doc:123#parent@folder:456")
        );

        Assert.True(system.Check(new User.UserId("dave"),
            "viewer",
            new RelationObject("doc", "123")));

        Assert.False(system.Check(new User.UserId("andrew"),
            "viewer",
            new RelationObject("doc", "123")));
    }
    
    [Fact]
    public void NestedFolderOwnerIsEditor()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(
            RelationTuple.Parse("folder:root#owner@dave"),
            RelationTuple.Parse("folder:docs#parent@folder:root"),
            RelationTuple.Parse("doc:123#parent@folder:docs")
        );

        Assert.True(system.Check(new User.UserId("dave"),
            "viewer",
            new RelationObject("doc", "123")));

        Assert.False(system.Check(new User.UserId("andrew"),
            "viewer",
            new RelationObject("doc", "123")));
    }
}