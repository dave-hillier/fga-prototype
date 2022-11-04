using System.Text.Json;
using Fga.Api.Core;

namespace Fga.Tests;

public class TypeDefinitionJsonDeserializationTests
{
    private readonly string _example = @"{
  ""type_definitions"": [
    {
      ""type"": ""document"",
      ""relations"": {
        ""writer"": {
          ""this"": {}
        },
        ""reader"": {
          ""union"": {
            ""child"": [
              {
                ""this"": {}
              },
              {
                ""computedUserset"": {
                  ""object"": """",
                  ""relation"": ""writer""
                }
              }
            ]
          }
        }
      }
    }
  ]
}
";

    [Fact]
    public void CanParse()
    {
      var result = JsonSerializer.Deserialize<AuthorizationModel>(_example);
      
      Assert.Equal("document", result.TypeDefinitions.First().Type);
      Assert.Equal("writer", result.TypeDefinitions.First().Relations.First().Key);
      Assert.Equal("reader", result.TypeDefinitions.First().Relations.Last().Key);
      Assert.Equal("reader", result.TypeDefinitions.First().Relations.Last().Key);
    }
}