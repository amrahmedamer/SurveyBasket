using Microsoft.AspNetCore.Authorization;
using SurveyBasket.Api.Entities;

namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;

    public PollsController(IPollService pollService)
    {
        _pollService = pollService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return  Ok(await _pollService.GetAllAsync(cancellationToken));
    } 
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        return  Ok(await _pollService.GetCurrentAsync(cancellationToken));
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var poll = await _pollService.GetByIdAsync(id, cancellationToken);
        return poll.IsSuccess
            ? Ok(poll.Value)
            : Problem(statusCode: StatusCodes.Status404NotFound, title: poll.Error.code, detail: poll.Error.descripiton);
    }
    [HttpPost("")]
    public async Task<IActionResult> Add(PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _pollService.AddAsync(request, cancellationToken);
        return result.IsSuccess
             ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
             : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error.code, detail: result.Error.descripiton);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PollRequest request, CancellationToken cancellationToken)
    {
        var result = await _pollService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status409Conflict, title: result.Error.code, detail: result.Error.descripiton);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error.code, detail: result.Error.descripiton);
    }
    [HttpPut("{id}/TogglePublish")]
    public async Task<IActionResult> TogglePublish(int id, CancellationToken cancellationToken)
    {
        var result = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error.code, detail: result.Error.descripiton);
    }

}
