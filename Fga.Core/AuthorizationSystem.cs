namespace Fga.Core;

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
        var hasWildcard =
            _memberToGroup.Any(t => t.User == User.Wildcard && t.Object == @object && t.Relation == relation);
        if (hasWildcard)
            return true;
            
        var userSet = GetUserset(user, relation, @object).ToHashSet();

        return userSet.Contains((@object, relation)) || 
               userSet.Intersect(GetGroupset(relation)).Any();
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetUserset(User user, string relation, RelationObject @object)
    {
        var type = _model.TypeDefinitions.FirstOrDefault(m => m.Type == @object.Namespace);

        if (type == null)
            throw new Exception($"Unknown type: {type}");

        if (!type.Relations.TryGetValue(relation, out var rel))
            throw new Exception($"Unknown type: {relation}");

        if (rel.This != null)
        {
            return GetDirectUserset(user);
        }
        
        return rel.Union is not {Child: { }} ? 
            Array.Empty<(RelationObject Object, string Relation)>() : 
            rel.Union.Child.SelectMany(child => GetChildUserset(child, user, relation, @object));
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetChildUserset(Child child, User user,
        string relation, RelationObject @object)
    {
        if (child.This != null)
        {
            return GetDirectUserset(user);
        }

        var computedUserset = child.ComputedUserset;
        if (computedUserset != null)
        {
            // TODO: add test for this
            var userSet = GetUserset(user, computedUserset.Relation, @object);
            //var userSet = GetDirectUserset(user);
            return ModifyRelation(userSet, relation, computedUserset.Relation);
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
            let obj = RelationObject.Parse(userSet.ToString()) // TODO: convert without strings
            from t in GetUserset(user, computedUserset, obj)
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

    private IEnumerable<(RelationObject Object, string Relation)> GetDirectUserset(User user)
    {
        return from t in _memberToGroup
            where t.User == user
            select (t.Object, t.Relation);
    }
}