using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuiz.Api.Contracts.Quizzes;
using TechQuiz.Application.Features.Quizzes;

namespace TechQuiz.Api.Controllers;

[ApiController]
[Route("api/quizzes")]
[Authorize]
public sealed class QuizzesController(IMediator mediator) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<QuizSessionDto>> Start(
        [FromBody] StartQuizRequest request,
        CancellationToken cancellationToken)
    {
        var session = await mediator.Send(new StartQuizCommand(request.CategoryId), cancellationToken);
        return Ok(session);
    }

    [HttpPost("{attemptId:guid}/answer")]
    public async Task<IActionResult> Answer(
        Guid attemptId,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new SubmitAnswerCommand(attemptId, request.QuestionId, request.SelectedOptionId),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{attemptId:guid}/complete")]
    public async Task<ActionResult<QuizResultDto>> Complete(
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CompleteQuizCommand(attemptId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{attemptId:guid}/result")]
    public async Task<ActionResult<QuizResultDto>> Result(
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetQuizResultQuery(attemptId), cancellationToken);
        return Ok(result);
    }
}
