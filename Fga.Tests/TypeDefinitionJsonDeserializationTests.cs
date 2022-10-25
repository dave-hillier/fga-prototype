using System.Text.Json;
using Fga.Core;

namespace Fga.Tests;

public class TypeDefinitionJsonDeserializationTests
{
    private readonly string _example = @"{
  ""type_definitions"": [
    {
      ""type"": ""document"",
      ""relations"": {
        ""reader"": {
          ""this"": {}
        },
        ""writer"": {
          ""this"": {}
        },
        ""owner"": {
          ""this"": {}
        }
      }
    }
  ]
}";

    [Fact]
    public void CanParse()
    {
      var result = JsonSerializer.Deserialize<AuthorizationModel>(_example);
      
      Assert.Equal("document", result.TypeDefinitions.First().Type);
      Assert.Equal("reader", result.TypeDefinitions.First().Relations.First().Key);
      Assert.Equal("owner", result.TypeDefinitions.First().Relations.Last().Key);
    }
}