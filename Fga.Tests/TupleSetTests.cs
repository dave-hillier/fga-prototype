namespace Fga.Tests;

public class TupleSetTests
{

    [Fact]
    public void FolderOwnerIsEditor()
    {
        var model = new AuthorizationModel
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
                                                    Object = "$TUPLE_USERSET_OBJECT",
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
                        {
                            "owner", new Relation
                            {
                                This = new This()
                            }
                        }
                    }
                },
            }
        };
        var system = new AuthorizationSystem(model);

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
}