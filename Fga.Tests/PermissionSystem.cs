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
            throw new Exception($"Unknown relation: {relation}");
  
        var userSet = (from t in _memberToGroup
            where t.User == user
            select (t.Object, t.Relation)).ToArray();
        
        if (userSet.Contains((@object, relation)))
            return true;
        
        if (rel.Union != null)
        {
            userSet = ComputerUserSet(relation, userSet, rel.Union.Name).
                Concat(userSet).
                ToArray();
        }

        if (rel.Lookup.HasValue)
        {
            var valueTuple = rel.Lookup.Value;
            (RelationObject Object, string Relation)[] subjectSet = userSet;
            var subjectSet2 = TupleSetToUserSet(relation, @object, valueTuple, subjectSet);

            subjectSet = subjectSet2.Concat(subjectSet).ToArray();
            userSet = subjectSet;
        }

        if (userSet.Contains((@object, relation)))
            return true;
        
        var objSet = from t in _groupToGroup
            let obj = t.User as User.UserSet
            where t.Relation == relation
            select (obj.Object, obj.Relation);

        return userSet.Intersect(objSet).Any();
    }

    private static IEnumerable<(RelationObject Object, string relation)> ComputerUserSet(string relation, (RelationObject Object, string Relation)[] userSet,
        string unionName)
    {
        return from t in userSet
            where t.Relation == unionName
            select (t.Object, relation);
    }

    private IEnumerable<(RelationObject @object, string relation)> TupleSetToUserSet(string relation, RelationObject @object,
        (ModelRelationship model, string relation) valueTuple, IEnumerable<(RelationObject Object, string Relation)> subjectSet)
    {
        var tupleSet = (from t in _memberToGroup
            where t.Relation == valueTuple.model.Name
            select t).First();

        return from t in subjectSet
            where t.Relation == valueTuple.relation && t.Object.ToString() == tupleSet.User.ToString()
            select (@object, relation);
    }
}