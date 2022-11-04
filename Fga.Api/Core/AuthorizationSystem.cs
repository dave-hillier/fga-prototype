namespace Fga.Core;

public class AuthorizationSystem
{
    private readonly HashSet<RelationTuple> _tuples = new();
    private readonly AuthorizationModel _model;

    public AuthorizationSystem(AuthorizationModel authorizationModel)
    {
        _model = authorizationModel;
    }

    public void Write(params RelationTuple[] tuples)
    {
        foreach (var relationTuple in tuples) _tuples.Add(relationTuple);
    }
    
    public void Delete(params RelationTuple[] removedTuples)
    {
        foreach (var relationTuple in removedTuples) _tuples.Remove(relationTuple);
    }

    public bool Check(User user, string relation, RelationObject @object)
    {
       if (_tuples.Contains(new RelationTuple(@object, relation, user)) || 
           _tuples.Contains(new RelationTuple(@object, relation, User.Wildcard)))
           return true;
       
       var groupsUserIsIn = GetUserset(user, relation, @object).ToHashSet();
       return groupsUserIsIn.Contains((@object, relation)) || 
              groupsUserIsIn.Any(g => Check(g.Object.ToUserset(g.Relation), relation, @object));
    }

    private static Relation GetRelation(string relation, TypeDefinition? type)
    {
        if (type == null)
            throw new Exception($"Unknown type: {type}");

        if (!type.Relations.TryGetValue(relation, out var rel))
            throw new Exception($"Unknown type: {relation}");
        
        return rel;
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetUserset(User user, string relation, RelationObject @object)
    {
        var type = _model.TypeDefinitions.FirstOrDefault(m => m.Type == @object.Namespace);
        var rel = GetRelation(relation, type);
        return UsersGroups(user, relation, @object, rel);
    }

    private IEnumerable<(RelationObject Object, string Relation)> UsersGroups(User user, string relation, RelationObject @object, Relation rel)
    {
        if (rel.This != null)
        {
            return GetUsersDirectGroups(user);
        }

        return rel.Union is not {Child: { }}
            ? Array.Empty<(RelationObject Object, string Relation)>()
            : rel.Union.Child.SelectMany(child => GetChildUserset(child, user, relation, @object));
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetChildUserset(Child child, User user,
        string relation, RelationObject @object)
    {
        if (child.This != null)
        {
            return GetUsersDirectGroups(user);
        }

        var computedUserset = child.ComputedUserset;
        if (computedUserset != null)
        {
            var userSet = GetUsersDirectGroups(user);
            
            return from t in userSet
                where t.Relation == computedUserset.Relation
                select (t.Object, relation);
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
        var groupsForObject = from t in _tuples
            where t.Object == @object && t.Relation == tuplesetRelation 
            select t.User;

        return from userSet in groupsForObject
            let obj = userSet.ToRelationObject()
            from t in GetUserset(user, computedUserset, obj)
            where t.Relation == computedUserset && t.Object == obj
            select (@object, relation);
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetUsersDirectGroups(User user)
    {
        return from t in _tuples
            where t.User == user || t.User == User.Wildcard
            select (t.Object, t.Relation);
    }
}