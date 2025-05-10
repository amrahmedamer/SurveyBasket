using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Api.Abstractions;

namespace SurveyBasket.Api.Controllers;
[Route("api/Poll/{PollId}/[controller]")]
[ApiController]
[Authorize]
public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;
    [HttpGet("{Id}")]
    public async Task<IActionResult> GetById([FromRoute] int PollId, [FromRoute] int Id, CancellationToken cancellationToken)
    {
        var restul = await _questionService.GetByIdAsync(PollId, Id, cancellationToken);
        return restul.IsSuccess ?
            Ok(restul.Value)
            : restul.ToProblem();
    }
    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromRoute] int PollId, CancellationToken cancellationToken)
    {
        var restul = await _questionService.GetAllAsync(PollId, cancellationToken);
        return restul.IsSuccess ?
            Ok(restul.Value)
            : restul.ToProblem();
    }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromRoute] int PollId, [FromBody] QuestionRequest request, CancellationToken cancellationToken)
    {
        var response = await _questionService.AddAsync(PollId, request, cancellationToken);
        if (response.IsSuccess)
            return CreatedAtAction(nameof(GetById), new { PollId, response.Value.Id }, response.Value);

        return response.ToProblem();
    }

    [HttpPut("{Id}")]
    public async Task<IActionResult> Update([FromRoute] int PollId, [FromRoute] int Id, [FromBody] QuestionRequest request, CancellationToken cancellationToken)
    {
        var response = await _questionService.UpdateAsync(PollId, Id,request, cancellationToken);
        if (response.IsSuccess)
            return NoContent();

        return response.ToProblem();
    }
    [HttpPut("{Id}/ToggleStatus")]
    public async Task<IActionResult> ToggleStatus([FromRoute] int PollId, [FromRoute] int Id, CancellationToken cancellationToken)
    {
        var result = await _questionService.ToggleStatusAsync(PollId, Id, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }

}
