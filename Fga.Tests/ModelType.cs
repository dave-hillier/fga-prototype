namespace Fga.Tests;

public class ModelType
{
    public ModelType(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IEnumerable<ModelRelationship> Relationships { get; init; }
}