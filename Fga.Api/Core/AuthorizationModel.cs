using System.Text.Json.Serialization;

namespace Fga.Api.Core;

// designed to be compatible with OpenFGA's schema, https://openfga.dev/

public class AuthorizationModel
{
    [JsonPropertyName("type_definitions")]
    public TypeDefinition[] TypeDefinitions { get; init; }
}

public class TypeDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonPropertyName("relations")]
    public Dictionary<string, Relation> Relations { get; init; }
}

public class Relation
{
    [JsonPropertyName("union")]
    public Union? Union { get; set; }

    [JsonPropertyName("this")]
    public This? This { get; set; }
}

public class Union
{
    [JsonPropertyName("child")]
    public Child[]? Child { get; init; }
}

public class Child
{
    [JsonPropertyName("this")]
    public This? This { get; set; }

    [JsonPropertyName("computed_userset")]
    public ComputedUserset? ComputedUserset { get; set; }

    [JsonPropertyName("tupleToUserset")]
    public TupleToUserset? TupleToUserset { get; set; }
}

public class ComputedUserset
{
    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("relation")]
    public string Relation { get; init; }
}

public class Tupleset
{
    [JsonPropertyName("relation")]
    public string Relation { get; init; }
}


public class This
{
}

public class TupleToUserset
{
    [JsonPropertyName("tupleset")]
    public Tupleset Tupleset { get; init; }

    [JsonPropertyName("computedUserset")]
    public ComputedUserset ComputedUserset { get; init; }
}
