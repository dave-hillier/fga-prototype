namespace Fga.Core;

public record RelationObject(string Namespace, string ObjectId)
{
    public override string ToString()
    {
        return $"{Namespace}:{ObjectId}";
    }

    public static RelationObject Parse(string s)
    {
        var tokens = s.Split(':');
        return new RelationObject(tokens[0], tokens[1]);
    }
}