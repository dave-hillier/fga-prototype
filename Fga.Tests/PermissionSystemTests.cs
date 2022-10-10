namespace Fga.Tests;

public class PermissionSystemTests
{
    [Fact]
    public void OwnerIsEditor()
    {
        var ownerRel = new ModelRelationship("owner");
        var editorRel = new ModelRelationship("editor") { Union = ownerRel };
 
        var system = new PermissionSystem(new ModelType("doc")
        {Relationships = new[] { editorRel, ownerRel }});
    
        system.Write(RelationTuple.Parse("doc:123#owner@kuba"));
        
        Assert.True(system.Check(new User.UserId("kuba"), 
            "editor", 
            new RelationObject("doc", "123")));
    }
    
    [Fact]
    public void FolderOwnerIsEditor()
    {
        var ownerRel = new ModelRelationship("owner");
        var parentRel = new ModelRelationship("parent");
        var viewerRel = new ModelRelationship("viewer") { Lookup = parentRel };
 
        var system = new PermissionSystem(
            new ModelType("folder") {Relationships = new [] { ownerRel }},
            new ModelType("doc") {Relationships = new[] { viewerRel, parentRel }});
    
        system.Write(
            RelationTuple.Parse("folder:456#owner@dave"),
            RelationTuple.Parse("doc:123#parent@folder:456")
            );
        
        Assert.True(system.Check(new User.UserId("dave"), 
            "viewer", 
            new RelationObject("doc", "123")));
    }    
}