using Newtonsoft.Json;

namespace Fga.Tests;

public class AuthorizationModel
{
    public TypeDefinition[] TypeDefinitions { get; set; }
}

public class TypeDefinition
{
    public string Type { get; set; }

    public Dictionary<string, Relation> Relations { get; set; }
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
    public Child[]? Child { get; set; }
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
    
    public string Relation { get; set; }
}

public class Tupleset
{
    public string Relation { get; set; }
}


public class This
{
}

public class TupleToUserset
{
    public Tupleset Tupleset { get; set; }
    
    public ComputedUserset ComputedUserset { get; set; }
}