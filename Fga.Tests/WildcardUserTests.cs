using Fga.Api.Core;

namespace Fga.Tests;

public class WildcardUserTests
{
    private readonly AuthorizationSystem _system = new(
        new AuthorizationModel
        {
            TypeDefinitions = new[]
            {
                new TypeDefinition
                {
                    Type = "doc",
                    Relations = new Dictionary<string, Relation>
                    {
                        { "editor", new Relation {This = new This()} },
                        { "viewer", new Relation {This = new This()} }
                    }
                },
            }
        }
    );
    
    [Fact]
    public void WildCardRelationship()
    {
        _system.Write(RelationTuple.Parse("doc:readme.md#viewer@*"));
        
        Assert.True(_system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "readme.md")));
    }

}