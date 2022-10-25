using Fga.Core;

namespace Fga.Tests;

public class ComputedRelationTests
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

    [Fact]
    public void OwnerIsEditor()
    {
        var system = new AuthorizationSystem(_model);

        system.Write(RelationTuple.Parse("doc:123#owner@kuba"));

        Assert.True(system.Check(new User.UserId("kuba"),
            "editor",
            new RelationObject("doc", "123")));

        Assert.False(system.Check(new User.UserId("dave"),
            "editor",
            new RelationObject("doc", "123")));
    }

}