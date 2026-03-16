using Fga.Api.Core;
using Microsoft.AspNetCore.Mvc;

namespace Fga.Api.Controllers;

[ApiController]
[Route("[controller]/{store}")]
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
    public IActionResult Write(string store, [FromBody]WriteRequest writeRequest)
    {
        try
        {
            if (writeRequest.Writes?.TupleKeys?.Any() == true)
            {
                var tuples = writeRequest.Writes.TupleKeys.ToArray();
                _authorizationSystem.ValidateWrite(tuples);
                _authorizationSystem.Write(tuples);
            }

            if (writeRequest.Deletes?.TupleKeys?.Any() == true)
            {
                _authorizationSystem.Delete(writeRequest.Deletes.TupleKeys.ToArray());
            }

            return Accepted();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check")]
    public CheckResponse Check(string store, [FromBody]CheckRequest request)
    {
        var requestTupleKey = request.TupleKey;
        var result = _authorizationSystem.Check(requestTupleKey.User, requestTupleKey.Relation, requestTupleKey.Object);
        return new CheckResponse(Allowed: result);
    }

    [HttpPost("expand")]
    public ExpandResponse Expand(string store, [FromBody]ExpandRequest request)
    {
        var tree = _authorizationSystem.Expand(request.Relation, request.Object);
        return new ExpandResponse(Tree: tree);
    }

    [HttpPost("list-objects")]
    public ListObjectsResponse ListObjects(string store, [FromBody]ListObjectsRequest request)
    {
        var objects = _authorizationSystem.ListObjects(request.User, request.Relation, request.Type);
        return new ListObjectsResponse(Objects: objects.Select(o => o.ToString()).ToList());
    }
}

public record CheckRequest(RelationTuple TupleKey);
public record CheckResponse(bool Allowed);

public record ExpandRequest(string Relation, RelationObject Object);
public record ExpandResponse(UsersetTree Tree);

public record ListObjectsRequest(User User, string Relation, string Type);
public record ListObjectsResponse(IReadOnlyList<string> Objects);

public class TupleContainer
{
    public IEnumerable<RelationTuple> TupleKeys { get; set; } = Array.Empty<RelationTuple>();
}

public class WriteRequest
{
    public TupleContainer Writes { get; set; } = new();
    public TupleContainer Deletes { get; set; } = new();
}