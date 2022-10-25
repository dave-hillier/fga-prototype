using Fga.Core;
using Microsoft.AspNetCore.Mvc;

namespace Fga.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StoresController : ControllerBase
{
    private readonly ILogger<StoresController> _logger;
    private readonly AuthorizationSystem _authorizationSystem;

    public StoresController(ILogger<StoresController> logger, AuthorizationSystem authorizationSystem)
    {
        _logger = logger;
        _authorizationSystem = authorizationSystem;
    }

    [HttpPost("write")]
    public IActionResult Write([FromBody]WriteRequest writeRequest)
    {
        if (writeRequest.Writes.TupleKeys.Any())
        {
            _authorizationSystem.Write(writeRequest.Writes.TupleKeys.ToArray());
        }
        
        if (writeRequest.Deletes.TupleKeys.Any())
        {
            _authorizationSystem.Delete(writeRequest.Deletes.TupleKeys.ToArray());
        }
        
        return Accepted();
    }
    
    [HttpGet("check")]
    public CheckResponse Check(CheckRequest request)
    {
        var requestTupleKey = request.TupleKey;
        var result = _authorizationSystem.Check(requestTupleKey.User, requestTupleKey.Relation, requestTupleKey.Object);
        return new CheckResponse(Allowed: result);
    }
}

public record CheckRequest(RelationTuple TupleKey);
public record CheckResponse(bool Allowed);

public class TupleContainer
{
    public IEnumerable<RelationTuple> TupleKeys { get; set; } = Array.Empty<RelationTuple>();
}

public class WriteRequest
{
    public TupleContainer Writes { get; set; } = new();
    public TupleContainer Deletes { get; set; } = new();
}