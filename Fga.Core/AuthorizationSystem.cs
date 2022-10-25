namespace Fga.Core;

public class AuthorizationSystem
{
    private readonly HashSet<RelationTuple> _all = new();
    
    private readonly HashSet<RelationTuple> _groupToGroupCache = new();
    private IEnumerable<RelationTuple> GroupToGroup => _groupToGroupCache.Union(_all.Where(t => t.User is not User.UserId));

    private readonly AuthorizationModel _model;
 
    public AuthorizationSystem(AuthorizationModel authorizationModel)
    {
        _model = authorizationModel;
    }

    public void Write(params RelationTuple[] tuples)
    {
        foreach (var relationTuple in tuples) _all.Add(relationTuple);

        AddTuplesToCache(tuples);
    }
    
    public void Delete(params RelationTuple[] removedTuples)
    {
        foreach (var relationTuple in removedTuples) _all.Remove(relationTuple);

        // Clear the cache & rebuild
        _groupToGroupCache.Clear();
        AddTuplesToCache(_all);
    }

    private void AddTuplesToCache(IEnumerable<RelationTuple> tuples)
    {
        var groups = tuples.Where(t => t.User is not User.UserId);

        AddGroup2GroupCache(groups.ToArray());
    }

    private void AddGroup2GroupCache(RelationTuple[] tuples)
    {
        while (tuples.Any())
        {
            var objSet = from toInsert in tuples
                from t in GroupToGroup
                where t != toInsert
                let userSet = toInsert.User as User.UserSet
                where t.Object == userSet.Object
                select new RelationTuple(toInsert.Object, toInsert.Relation, t.User);

            tuples = objSet.ToArray();
            foreach (var tuple in tuples) _groupToGroupCache.Add(tuple);
        }
    }

    public bool Check(User user, string relation, RelationObject @object)
    {
        var groupsUserIsIn = GetUserset(user, relation, @object).ToHashSet();
        
        return groupsUserIsIn.Contains((@object, relation)) || 
               groupsUserIsIn.Any(g => Check(g.Object.ToUserset(g.Relation), relation, @object));
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
            var userSet = GetDirectUserset(user);
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
        var groupsForObject = from t in _all
            where t.Object == @object && t.Relation == tuplesetRelation 
            select t.User;

        return from userSet in groupsForObject
            let obj = userSet.ToRelationObject()
            from t in GetUserset(user, computedUserset, obj)
            where t.Relation == computedUserset && t.Object == obj
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

    private IEnumerable<(RelationObject Object, string Relation)> GetDirectUserset(User user)
    {
        return from t in _all
            where t.User == user || t.User == User.Wildcard
            select (t.Object, t.Relation);
    }
}