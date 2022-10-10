namespace Fga.Tests;

public class PermissionSystemTests
{
    [Fact]
    public void OwnerIsEditor()
    {
        var typeSystem = new TypeSystem
        {
            TypeDefinitions = new[]
            {
                new TypeDefinition
                {
                    Type = "doc",
                    Relations = new Dictionary<string, Relation>
                    {
                        {
                            "owner", new Relation {This = new This()}
                        },
                        {
                            "editor", new Relation
                            {
                                Union = new Union
                                {
                                    Child = new Child[]
                                    {
                                        new() {This = new This()},
                                        new() {ComputedUserset = new ComputedUserset {Relation = "owner"}}
                                    }
                                }
                            }
                        }
                    }
                },
            }
        };

        var system = new PermissionSystem(typeSystem);

        system.Write(RelationTuple.Parse("doc:123#owner@kuba"));

        Assert.True(system.Check(new User.UserId("kuba"),
            "editor",
            new RelationObject("doc", "123")));

        Assert.False(system.Check(new User.UserId("dave"),
            "editor",
            new RelationObject("doc", "123")));
    }

    [Fact]
    public void FolderOwnerIsEditor()
    {
        var typeSystem = new TypeSystem
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
        var system = new PermissionSystem(typeSystem);

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