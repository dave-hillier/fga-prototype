namespace Fga.Tests;

public class PermissionSystemTests
{
    
    [Fact]
    public void OwnerIsEditor()
    {
        var ownerRel = new ModelRelationship("owner");
        var editorRel = new ModelRelationship("editor");

        editorRel = editorRel.Union(ownerRel);
        
        var system = new PermissionsSystem(new ModelType("doc")
        {Relationships = new[] { editorRel, ownerRel }});
    
        system.Write(
            RelationTuple.Parse("doc:123#owner@kuba")
        );
        
        Assert.True(system.Check(new User.UserId("kuba"), 
            "editor", 
            new RelationObject("doc", "123")));
    }    
}