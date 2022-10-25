namespace Fga.Core;

public record RelationTuple(RelationObject Object, string Relation, User User)
{
    public override string ToString()
    {
        return $"{Object}#{Relation}@{User}";
    }

    public static RelationTuple Parse(string tuple)
    {
        var s = tuple.Split('@');
        var user = User.Parse(s[1]);
        var t = s[0].Split('#');
        var @object = RelationObject.Parse(t[0]);
        var relation = t[1];
        return new RelationTuple(@object, relation, user);
    }
}