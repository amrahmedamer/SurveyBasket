using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Contracts.Vote;
using SurveyBasket.Api.Extensions;
using System.Security.Claims;

namespace SurveyBasket.Api.Controllers;
[Route("api/poll/{pollId}/vote")]
[ApiController]
[Authorize]
public class VotesController(IQuestionService questionService, IVoteService voteService, ILogger<VotesController> logger) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;
    private readonly IVoteService _voteService = voteService;
    private readonly ILogger<VotesController> _logger = logger;

    [HttpGet("")]
    public async Task<IActionResult> Start([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetAvailableAsync(pollId, User.GetUserId()!, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();

    }
    [HttpPost("")]
    public async Task<IActionResult> Vote([FromRoute] int pollId, [FromBody] VoteRequest voteRequest, CancellationToken cancellationToken)
    {
        var result = await _voteService.AddAsync(pollId, User.GetUserId()!, voteRequest, cancellationToken);

        if (result.IsSuccess)
            return Created();

        return result.ToProblem();
    }
}
