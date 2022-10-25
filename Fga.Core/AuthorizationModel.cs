using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Fga.Core;

// designed to be compatible with OpenFGA's schema, https://openfga.dev/
// The mixture of snake and pascal case is strange 

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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Union? Union { get; set; }
    
    [JsonPropertyName("this")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public This? This { get; set; }

    [JsonPropertyName("computed_userset")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ComputedUserset? ComputedUserset { get; set; }
    
    [JsonPropertyName("tupleToUserset")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TupleToUserset? TupleToUserset { get; set; }
}

public class ComputedUserset
{
    [JsonPropertyName("object")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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