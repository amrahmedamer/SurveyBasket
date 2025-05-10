using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Api.Entities;

namespace SurveyBasket.Api.Controllers;
[Route("api/polls/{pollId}/[controller]")]
[ApiController]
[Authorize]
public class ResultsController(IResultService resultService) : ControllerBase
{
    private readonly IResultService _resultService = resultService;

    [HttpGet("row-data")]
    public async Task<IActionResult> PollVote([FromRoute] int pollId,CancellationToken cancellationToken)
    {
        var result = await _resultService.GetPollVoteAsync(pollId, cancellationToken);

        return result.IsSuccess 
            ? Ok(result.Value)
            :result.ToProblem();
    } 
    [HttpGet("votes-per-day")]
    public async Task<IActionResult> VotePerDay([FromRoute] int pollId,CancellationToken cancellationToken)
    {
        var result = await _resultService.GetVotePerDayAsync(pollId, cancellationToken);

        return result.IsSuccess 
            ? Ok(result.Value)
            :result.ToProblem();
    } 
    [HttpGet("votes-per-question")]
    public async Task<IActionResult> VotePerQuestion([FromRoute] int pollId,CancellationToken cancellationToken)
    {
        var result = await _resultService.GetVotePerQuestionAsync(pollId, cancellationToken);

        return result.IsSuccess 
            ? Ok(result.Value)
            :result.ToProblem();
    }
}
