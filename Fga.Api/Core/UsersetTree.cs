namespace Fga.Api.Core;

/// <summary>
/// Represents the result of an Expand call — a tree showing how a relation is satisfied.
/// </summary>
public abstract record UsersetTree(string Relation, RelationObject Object)
{
    /// <summary>
    /// A leaf node containing directly assigned users/usersets.
    /// </summary>
    public record Leaf(string Relation, RelationObject Object, User[] Users)
        : UsersetTree(Relation, Object);

    /// <summary>
    /// A union node — any child being satisfied is sufficient.
    /// </summary>
    public record UnionNode(string Relation, RelationObject Object, UsersetTree[] Children)
        : UsersetTree(Relation, Object);

    /// <summary>
    /// An intersection node — all children must be satisfied.
    /// </summary>
    public record IntersectionNode(string Relation, RelationObject Object, UsersetTree[] Children)
        : UsersetTree(Relation, Object);

    /// <summary>
    /// An exclusion node — base must be satisfied and subtract must not.
    /// </summary>
    public record ExclusionNode(string Relation, RelationObject Object, UsersetTree Base, UsersetTree Subtract)
        : UsersetTree(Relation, Object);
}
