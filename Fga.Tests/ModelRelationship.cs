namespace Fga.Tests;

public class ModelRelationship
{
    private readonly string _name;

    public ModelRelationship(string name)
    {
        _name = name;
    }
    
    public ModelRelationship Union(ModelRelationship ownerRel)
    {
        return new ModelRelationship(_name) { Children = new [] { this, ownerRel }};
    }

    public ModelRelationship[] Children { get; set; }
}