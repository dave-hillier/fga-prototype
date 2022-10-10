namespace Fga.Tests;

public class PermissionSystem
{
    private readonly List<RelationTuple> _all = new();
    
    private readonly HashSet<RelationTuple> _groupToGroup = new();
    private readonly HashSet<RelationTuple> _memberToGroup = new();
    private readonly ModelType[] _types;
 
    public PermissionSystem(params ModelType[] types)
    {
        _types = types;
    }

    public void Write(params RelationTuple[] tuples)
    {
        foreach (var relationTuple in tuples) _all.Add(relationTuple);

        WriteCache(tuples);
    }

    private void WriteCache(IEnumerable<RelationTuple> tuples)
    {
        var grouped = tuples.GroupBy(t => t.User is User.UserId).ToDictionary(g => g.Key, g => g.ToArray());

        if (grouped.ContainsKey(true))
            WriteMember2GroupCache(grouped[true]);

        if (grouped.ContainsKey(false))
            WriteGroup2GroupCache(grouped[false]);
    }

    private void WriteMember2GroupCache(IEnumerable<RelationTuple> permissionsTuples)
    {
        foreach (var permissionsTuple in permissionsTuples) _memberToGroup.Add(permissionsTuple);
    }

    private void WriteGroup2GroupCache(RelationTuple[] tuples)
    {
        while (tuples.Any())
        {
            foreach (var tuple in tuples) _groupToGroup.Add(tuple);

            var objSet = from toInsert in tuples
                from t in _groupToGroup
                where t != toInsert
                let userSet = toInsert.User as User.UserSet
                where t.Object == userSet.Object
                select new RelationTuple(toInsert.Object, toInsert.Relation, t.User);

            tuples = objSet.ToArray();
        }
    }

    public bool Check(User user, string relation, RelationObject @object)
    {
        var rel = _types.FirstOrDefault(m => m.Name == @object.Namespace)?.
            Relationships.FirstOrDefault(r => r.Name == relation);
        
        if (rel == null)
            throw new Exception($"Unknown relation {relation}");
  
        var subjectSet = (from t in _memberToGroup
            where t.User == user
            select (t.Object, t.Relation)).ToArray();

        if (rel.Union != null)
        {
            var subjectSet2 = from t in subjectSet
                where t.Relation == rel.Union.Name
                select (t.Object, relation);
            subjectSet = subjectSet2.Concat(subjectSet).ToArray();
        }

        if (subjectSet.Contains((@object, relation)))
            return true;
        
        var objSet = from t in _groupToGroup
            let obj = t.User as User.UserSet
            where t.Relation == relation
            select (obj.Object, obj.Relation);

        return subjectSet.Intersect(objSet).Any();
    }


}