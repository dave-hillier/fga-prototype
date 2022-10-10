namespace Fga.Tests;

public class RelationTests
{
    private readonly PermissionSystem _system = new(
        new AuthorizationModel
        {
            TypeDefinitions = new[]
            {
                new TypeDefinition
                {
                    Type = "team",
                    Relations = new Dictionary<string, Relation>
                    {
                        { "member", new Relation {This = new This()} }
                    }
                },
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
    public void Empty()
    {
        Assert.False(_system.Check(new User.UserId("dave"), "member", new RelationObject("team", "labs")));
    }

    [Fact]
    public void BasicRelationship()
    {
        _system.Write(RelationTuple.Parse("team:labs#member@dave"));
        
        Assert.True(_system.Check(new User.UserId("dave"), "member", new RelationObject("team", "labs")));
    }

    [Fact]
    public void IndirectNoConnection()
    {
        _system.Write(
            RelationTuple.Parse("team:labs#member@dave"),
            RelationTuple.Parse("team:downhamhall#member@andrew"),
            RelationTuple.Parse("team:guestline#member@team:labs#member")
                );

        Assert.False(_system.Check(new User.UserId("andrew"), "member", new RelationObject("team", "guestline")));
    }

    [Fact]
    public void IndirectRelationship()
    {
        _system.Write(
            RelationTuple.Parse("team:labs#member@dave"),
            RelationTuple.Parse("team:guestline#member@team:labs#member"));

        Assert.True(_system.Check(new User.UserId("dave"), "member", new RelationObject("team", "guestline")));
    }
    
    [Fact]
    public void IndirectRelationshipDifferentRelation()
    {
        _system.Write(
            RelationTuple.Parse("team:labs#member@dave"),
            RelationTuple.Parse("doc:123#editor@team:labs#member"));

        Assert.True(_system.Check(new User.UserId("dave"), "editor", new RelationObject("doc", "123")));
    }
    
       
    [Fact]
    public void IndirectRelationshipRandomRelation()
    {
        _system.Write(
            RelationTuple.Parse("team:labs#member@dave"),
            RelationTuple.Parse("doc:123#editor@team:labs#member"));

        Assert.False(_system.Check(new User.UserId("dave"), "viewer", new RelationObject("doc", "123")));
    }


    [Fact]
    public void IndirectRelationshipTwoLevels()
    {
        _system.Write(
            RelationTuple.Parse("team:sugoi#member@kuba"),
            RelationTuple.Parse("team:labs#member@team:sugoi#member"),
            RelationTuple.Parse("team:guestline#member@team:labs#member"));


        Assert.True(_system.Check(new User.UserId("kuba"), "member", new RelationObject("team", "guestline")));
    }
    
    [Fact]
    public void IndirectParentGroup()
    {
        _system.Write(
            RelationTuple.Parse("team:sugoi#member@kuba"),
            RelationTuple.Parse("team:labs#member@team:sugoi#member"),
            RelationTuple.Parse("team:guestline#member@team:labs#member"),
            RelationTuple.Parse("doc:123#editor@team:guestline#member")
            );
        
        Assert.True(_system.Check(new User.UserId("kuba"), "editor", new RelationObject("doc", "123")));
    }
}