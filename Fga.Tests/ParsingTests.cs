using Fga.Core;

namespace Fga.Tests;

public class ParsingTests
{
    [Fact]
    public void Single()
    {
        var str = "doc:123#editor@dave";
        var tuple = RelationTuple.Parse(str);
        Assert.Equal(str, tuple.ToString());
    }
    
    [Fact]
    public void Group()
    {
        var str = "doc:123#editor@team:labs#member";
        var tuple = RelationTuple.Parse(str);
        Assert.Equal(str, tuple.ToString());
    }
}