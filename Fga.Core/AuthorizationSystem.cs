namespace Fga;

public class AuthorizationSystem
{
    private readonly List<RelationTuple> _all = new();
    
    private readonly HashSet<RelationTuple> _groupToGroup = new();
    private readonly HashSet<RelationTuple> _memberToGroup = new();
    private readonly AuthorizationModel _model;
 
    public AuthorizationSystem(AuthorizationModel authorizationModel)
    {
        _model = authorizationModel;
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

    private void WriteMember2GroupCache(IEnumerable<RelationTuple> tuples)
    {
        foreach (var tuple in tuples) _memberToGroup.Add(tuple);
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
        var type = _model.TypeDefinitions.FirstOrDefault(m => m.Type == @object.Namespace);

        if (type == null)
            throw new Exception($"Unknown type: {type}");

        if (!type.Relations.TryGetValue(relation, out var rel))
            throw new Exception($"Unknown type: {relation}");

        (RelationObject Object, string Relation)[] userSet = {};

        if (rel.This != null)
        {
            userSet = GetUserset(user).ToArray();
        }
        else if (rel.Union is {Child: { }})
        {
            var computedSets = rel.Union.Child.Select(child => GetComputed(user, relation, @object, child));
            foreach (var set in computedSets) userSet = set.Concat(userSet).ToArray();
        }
        
        return userSet.Contains((@object, relation)) || 
               userSet.Intersect(GetGroupset(relation)).Any();
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetComputed(User user, string relation, RelationObject @object, Child child)
    {
        if (child.This != null)
        {
            return GetUserset(user);
        }

        var computedUserset = child.ComputedUserset;
        if (computedUserset != null)
        {
            return ModifyRelation(GetUserset(user), relation, computedUserset.Relation);
        }

        var tupleToUserset = child.TupleToUserset;
        
        if (tupleToUserset == null) 
            return Array.Empty<(RelationObject Object, string Relation)>();

        return TupleToUserset(user, relation, @object, 
            tupleToUserset.Tupleset.Relation, 
            tupleToUserset.ComputedUserset.Relation);

    }

    private IEnumerable<(RelationObject Object, string Relation)> TupleToUserset(
        User user, 
        string relation, 
        RelationObject @object, 
        string tuplesetRelation,
        string computedUserset)
    {
        var groupsForObject = from t in _memberToGroup
            where t.Object == @object && t.Relation == tuplesetRelation 
            select t.User;

        return from userSet in groupsForObject
            from t in GetUserset(user)
            where t.Relation == computedUserset && t.Object.ToString() == userSet.ToString()
            select (@object, relation);
    }

    private static IEnumerable<(RelationObject Object, string Relation)> ModifyRelation(
        IEnumerable<(RelationObject Object, string Relation)> userSet,
        string targetRelation,
        string computedRelation)
    {
        return from t in userSet
            where t.Relation == computedRelation
            select (t.Object, relation: targetRelation);
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetGroupset(string relation)
    {
        return from t in _groupToGroup
            let obj = t.User as User.UserSet
            where t.Relation == relation
            select (obj.Object, obj.Relation);
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetUserset(User user)
    {
        return from t in _memberToGroup
            where t.User == user
            select (t.Object, t.Relation);
    }
}