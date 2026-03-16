namespace Fga.Api.Core;

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

    public void ValidateWrite(params RelationTuple[] tuples)
    {
        foreach (var tuple in tuples)
        {
            var type = _model.TypeDefinitions.FirstOrDefault(t => t.Type == tuple.Object.Namespace);
            if (type == null)
                throw new ValidationException($"Unknown type: {tuple.Object.Namespace}");

            if (!type.Relations.ContainsKey(tuple.Relation))
                throw new ValidationException($"Unknown relation: {tuple.Relation} on type {tuple.Object.Namespace}");
        }
    }

    public bool Check(User user, string relation, RelationObject @object)
    {
        return Check(user, relation, @object, new HashSet<(User, string, RelationObject)>());
    }

    private bool Check(User user, string relation, RelationObject @object,
        HashSet<(User, string, RelationObject)> visited)
    {
        if (!visited.Add((user, relation, @object)))
            return false;

        if (_tuples.Contains(new RelationTuple(@object, relation, user)) ||
           _tuples.Contains(new RelationTuple(@object, relation, User.Wildcard)))
           return true;

        var type = _model.TypeDefinitions.FirstOrDefault(m => m.Type == @object.Namespace);
        var rel = GetRelation(relation, @object.Namespace, type);

        return EvaluateRelation(user, relation, @object, rel, visited);
    }

    private bool EvaluateRelation(User user, string relation, RelationObject @object,
        Relation rel, HashSet<(User, string, RelationObject)> visited)
    {
        if (rel.This != null)
        {
            return CheckDirectGroups(user, relation, @object, visited);
        }

        if (rel.Union is { Child: { } })
        {
            return rel.Union.Child.Any(child => EvaluateChild(child, user, relation, @object, visited));
        }

        if (rel.Intersection is { Child: { } })
        {
            return rel.Intersection.Child.All(child => EvaluateChild(child, user, relation, @object, visited));
        }

        if (rel.Exclusion != null)
        {
            return EvaluateChild(rel.Exclusion.Base, user, relation, @object, visited) &&
                   !EvaluateChild(rel.Exclusion.Subtract, user, relation, @object, visited);
        }

        return false;
    }

    private bool EvaluateChild(Child child, User user, string relation, RelationObject @object,
        HashSet<(User, string, RelationObject)> visited)
    {
        if (child.This != null)
        {
            return _tuples.Contains(new RelationTuple(@object, relation, user)) ||
                   _tuples.Contains(new RelationTuple(@object, relation, User.Wildcard)) ||
                   CheckDirectGroups(user, relation, @object, visited);
        }

        if (child.ComputedUserset != null)
        {
            return Check(user, child.ComputedUserset.Relation, @object, visited);
        }

        if (child.TupleToUserset != null)
        {
            return CheckTupleToUserset(user, @object, visited,
                child.TupleToUserset.Tupleset.Relation,
                child.TupleToUserset.ComputedUserset.Relation);
        }

        return false;
    }

    private bool CheckDirectGroups(User user, string relation, RelationObject @object,
        HashSet<(User, string, RelationObject)> visited)
    {
        var groups = GetUsersDirectGroups(user);
        return groups.Any(g => g.Object == @object && g.Relation == relation ||
                               Check(g.Object.ToUserset(g.Relation), relation, @object, visited));
    }

    private bool CheckTupleToUserset(User user, RelationObject @object,
        HashSet<(User, string, RelationObject)> visited,
        string tuplesetRelation, string computedUserset)
    {
        var groupsForObject = from t in _tuples
            where t.Object == @object && t.Relation == tuplesetRelation
            select t.User;

        return groupsForObject.Any(userRef =>
            userRef is User.UserSet us &&
            Check(user, computedUserset, us.Object, visited));
    }

    /// <summary>
    /// Expand returns a userset tree explaining why a user has (or could have) a relation.
    /// </summary>
    public UsersetTree Expand(string relation, RelationObject @object)
    {
        return Expand(relation, @object, new HashSet<(string, RelationObject)>());
    }

    private UsersetTree Expand(string relation, RelationObject @object,
        HashSet<(string, RelationObject)> visited)
    {
        if (!visited.Add((relation, @object)))
            return new UsersetTree.Leaf(relation, @object, Array.Empty<User>());

        var type = _model.TypeDefinitions.FirstOrDefault(m => m.Type == @object.Namespace);
        var rel = GetRelation(relation, @object.Namespace, type);

        return ExpandRelation(relation, @object, rel, visited);
    }

    private UsersetTree ExpandRelation(string relation, RelationObject @object,
        Relation rel, HashSet<(string, RelationObject)> visited)
    {
        if (rel.This != null)
        {
            return ExpandDirect(relation, @object, visited);
        }

        if (rel.Union is { Child: { } })
        {
            var children = rel.Union.Child
                .Select(c => ExpandChild(c, relation, @object, visited))
                .ToArray();
            return new UsersetTree.UnionNode(relation, @object, children);
        }

        if (rel.Intersection is { Child: { } })
        {
            var children = rel.Intersection.Child
                .Select(c => ExpandChild(c, relation, @object, visited))
                .ToArray();
            return new UsersetTree.IntersectionNode(relation, @object, children);
        }

        if (rel.Exclusion != null)
        {
            var baseTree = ExpandChild(rel.Exclusion.Base, relation, @object, visited);
            var subtractTree = ExpandChild(rel.Exclusion.Subtract, relation, @object, visited);
            return new UsersetTree.ExclusionNode(relation, @object, baseTree, subtractTree);
        }

        return new UsersetTree.Leaf(relation, @object, Array.Empty<User>());
    }

    private UsersetTree ExpandChild(Child child, string relation, RelationObject @object,
        HashSet<(string, RelationObject)> visited)
    {
        if (child.This != null)
        {
            return ExpandDirect(relation, @object, visited);
        }

        if (child.ComputedUserset != null)
        {
            return Expand(child.ComputedUserset.Relation, @object, visited);
        }

        if (child.TupleToUserset != null)
        {
            return ExpandTupleToUserset(relation, @object, visited,
                child.TupleToUserset.Tupleset.Relation,
                child.TupleToUserset.ComputedUserset.Relation);
        }

        return new UsersetTree.Leaf(relation, @object, Array.Empty<User>());
    }

    private UsersetTree ExpandDirect(string relation, RelationObject @object,
        HashSet<(string, RelationObject)> visited)
    {
        var directUsers = _tuples
            .Where(t => t.Object == @object && t.Relation == relation)
            .Select(t => t.User)
            .ToList();

        var userSetExpansions = directUsers
            .OfType<User.UserSet>()
            .Select(us => Expand(us.Relation, us.Object, visited))
            .ToArray();

        if (userSetExpansions.Length > 0)
        {
            var leaf = new UsersetTree.Leaf(relation, @object, directUsers.ToArray());
            var all = new UsersetTree[] { leaf }.Concat(userSetExpansions).ToArray();
            return new UsersetTree.UnionNode(relation, @object, all);
        }

        return new UsersetTree.Leaf(relation, @object, directUsers.ToArray());
    }

    private UsersetTree ExpandTupleToUserset(string relation, RelationObject @object,
        HashSet<(string, RelationObject)> visited,
        string tuplesetRelation, string computedUserset)
    {
        var parents = _tuples
            .Where(t => t.Object == @object && t.Relation == tuplesetRelation)
            .Select(t => t.User)
            .OfType<User.UserSet>()
            .ToList();

        var children = parents
            .Select(us => Expand(computedUserset, us.Object, visited))
            .ToArray();

        if (children.Length > 0)
            return new UsersetTree.UnionNode(relation, @object, children);

        return new UsersetTree.Leaf(relation, @object, Array.Empty<User>());
    }

    /// <summary>
    /// ListObjects returns all objects of the given type that the user has the given relation to.
    /// </summary>
    public IReadOnlyList<RelationObject> ListObjects(User user, string relation, string objectType)
    {
        var candidates = _tuples
            .Where(t => t.Object.Namespace == objectType)
            .Select(t => t.Object)
            .Distinct();

        return candidates
            .Where(obj => Check(user, relation, obj))
            .ToList();
    }

    private static Relation GetRelation(string relation, string @namespace, TypeDefinition? type)
    {
        if (type == null)
            throw new Exception($"Unknown type: {@namespace}");

        if (!type.Relations.TryGetValue(relation, out var rel))
            throw new Exception($"Unknown relation: {relation} on type {@namespace}");

        return rel;
    }

    private IEnumerable<(RelationObject Object, string Relation)> GetUsersDirectGroups(User user)
    {
        return from t in _tuples
            where t.User == user || t.User == User.Wildcard
            select (t.Object, t.Relation);
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
