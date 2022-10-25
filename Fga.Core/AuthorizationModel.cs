using Newtonsoft.Json;

namespace Fga.Core;

// designed to be compatible with OpenFGA's schema,
// though with a different naming strategy https://www.newtonsoft.com/json/help/html/NamingStrategySnakeCase.htm

public class AuthorizationModel
{
    public TypeDefinition[] TypeDefinitions { get; init; }
}

public class TypeDefinition
{
    public string Type { get; init; }

    public Dictionary<string, Relation> Relations { get; init; }
}

public class Relation
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Union? Union { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public This? This { get; set; }
}

public class Union
{
    public Child[]? Child { get; init; }
}

public class Child
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public This? This { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ComputedUserset? ComputedUserset { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TupleToUserset? TupleToUserset { get; set; }
}

public class ComputedUserset
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Object { get; set; }
    
    public string Relation { get; init; }
}

public class Tupleset
{
    public string Relation { get; init; }
}


public class This
{
}

public class TupleToUserset
{
    public Tupleset Tupleset { get; init; }
    
    public ComputedUserset ComputedUserset { get; init; }
}