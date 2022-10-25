namespace Fga.Core;

public class Group2GroupCache
{
    private readonly HashSet<RelationTuple> _groupToGroupCache = new();

    private void Rebuild(IEnumerable<RelationTuple> allTuples)
    {
        // Clear the cache & rebuild
        _groupToGroupCache.Clear();
        Add(allTuples);
    }
        
    public void Add(IEnumerable<RelationTuple> tuples)
    {
        var groups = tuples.Where(t => t.User is not User.UserId);
        AddGroup2GroupCache(groups.ToArray());
    }

    private void AddGroup2GroupCache(RelationTuple[] tuples)
    {
        while (tuples.Any())
        {
            var tt = tuples;
            var objSet = from toInsert in tuples
                from t in tt.Concat(_groupToGroupCache)
                where t != toInsert
                let userSet = toInsert.User as User.UserSet
                where t.Object == userSet.Object
                select new RelationTuple(toInsert.Object, toInsert.Relation, t.User);

            tuples = objSet.ToArray();
            foreach (var tuple in tuples) _groupToGroupCache.Add(tuple);
        }
    }

    public void Remove(RelationTuple[] removedTuples, HashSet<RelationTuple> all)
    {
        if(removedTuples.Any(t => t.User is User.UserSet))
            Rebuild(all);
    }
}