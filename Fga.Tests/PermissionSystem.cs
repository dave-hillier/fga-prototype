namespace Fga.Tests;

public class PermissionSystem
{
    private readonly List<RelationTuple> _all = new();
    
    private readonly HashSet<RelationTuple> _groupToGroup = new();
    private readonly HashSet<RelationTuple> _memberToGroup = new();
    private readonly AuthorizationModel _types;
 
    public PermissionSystem(AuthorizationModel authorizationModel)
    {
        _types = authorizationModel;
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
        var type = _types.TypeDefinitions.FirstOrDefault(m => m.Type == @object.Namespace);

        if (type == null)
            throw new Exception($"Unknown type: {type}");

        if (!type.Relations.TryGetValue(relation, out var rel))
            throw new Exception($"Unknown type: {relation}");

  
        var userSet = (from t in _memberToGroup
            where t.User == user
            select (t.Object, t.Relation)).ToArray();
        
        if (userSet.Contains((@object, relation)))
            return true;
        
        if (rel.Union != null && rel.Union.Child != null && rel.Union.Child.Any())
        {
            var computedUsersets =
                from c in rel.Union.Child
                where c.ComputedUserset != null
                select c.ComputedUserset;

            foreach (var computedUserset in computedUsersets)
            {
                userSet = (from t in userSet
                        where t.Relation == computedUserset.Relation
                        select (t.Object, relation)).
                    Concat(userSet).
                    ToArray();
            }
            
            var tupleToUsersets =
                from c in rel.Union.Child
                where c.TupleToUserset != null
                select c.TupleToUserset;

            foreach (var tupleToUserset in tupleToUsersets)
            {
                var tuplesetRelation = tupleToUserset.Tupleset.Relation; // Parent
                var computedUserset = tupleToUserset.ComputedUserset;

                var userSetToFind = from t in _memberToGroup
                    where t.Object == @object && t.Relation == tuplesetRelation 
                    select t.User; // parent folder

                var tuplesetSearch = from userId in userSetToFind
                    from t in _memberToGroup
                    where t.User == user && t.Relation == computedUserset.Relation && t.Object.ToString() == userId.ToString()
                    select (@object, relation);
                
                userSet = tuplesetSearch.Concat(userSet).
                    ToArray();
            }
        }

        if (userSet.Contains((@object, relation)))
            return true;
        
        var objSet = from t in _groupToGroup
            let obj = t.User as User.UserSet
            where t.Relation == relation
            select (obj.Object, obj.Relation);

        return userSet.Intersect(objSet).Any();
    }
    
}