namespace Fga.Tests;

public class ModelRelationship
{
    public ModelRelationship(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public ModelRelationship? Union { get; set; }
    public (ModelRelationship model, string relation)? Lookup { get; set; }
}