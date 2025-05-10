using Microsoft.Extensions.Caching.Hybrid;
using SurveyBasket.Api.Contracts.Answer;

namespace SurveyBasket.Api.Services;

public class QuestionService(ApplicationDbContext context,
    ILogger<QuestionService> logger,
     HybridCache hybridCache ) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<QuestionService> _logger = logger;
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly string _cachePrefix = "availableQuestion";

    public async Task<Result<QuestionResponse>> GetByIdAsync(int PollId, int Id, CancellationToken cancellationToken = default)
    {
        var PollExists = await _context.Polls.AnyAsync(x => x.Id == PollId, cancellationToken);
        if (!PollExists)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var questions = await _context.Questions
         .Include(x => x.Answers)
         .Where(x => x.PollId == PollId && x.Id == Id)
         .ProjectToType<QuestionResponse>()
         .AsNoTracking()
         .SingleOrDefaultAsync(cancellationToken);

        if (questions is not null)
            return Result.Success<QuestionResponse>(questions);
        else
            return Result.Failure<QuestionResponse>(QuestionErrors.NoQuestion);
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int PollId, string UserId, CancellationToken cancellationToken = default)
    {
        //var hasVote = await _context.Votes.AnyAsync(x => x.PollId == PollId && x.UserId == UserId, cancellationToken);
        //if (hasVote)
        //    return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.DuplicatedVote);

        //var pollIsExist = await _context.Polls.AnyAsync(x => x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        //if (!pollIsExist)
        //    return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var cacheKey = $"{_cachePrefix}-{PollId}";


        var question = await _hybridCache.GetOrCreateAsync<IEnumerable<QuestionResponse>>(
            cacheKey,
            async cacheKey =>
                await _context.Questions
                 .Where(x => x.PollId == PollId && x.IsActive)
                 .Include(x => x.Answers)
                 .Select(q => new QuestionResponse
                 (
                     q.Id,
                     q.Content,
                     q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
                  ))
                 .AsNoTracking()
                 .ToListAsync(cancellationToken)
        );

        return Result.Success<IEnumerable<QuestionResponse>>(question!);


    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int PollId, CancellationToken cancellationToken = default)
    {
        var PollExists = await _context.Polls.AnyAsync(x => x.Id == PollId, cancellationToken);
        if (!PollExists)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _context.Questions
            .Include(x => x.Answers)
            .Where(x => x.PollId == PollId)
            //.Select(x => new QuestionResponse(
            //    x.Id,
            //    x.Content,
            //    x.Answers.Select(a => new AnswerResponse(a.Id, a.Content))
            //    ))
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (questions.Count > 0)
            return Result.Success<IEnumerable<QuestionResponse>>(questions);
        else
            return Result.Failure<IEnumerable<QuestionResponse>>(QuestionErrors.NoQuestion);

    }
    public async Task<Result<QuestionResponse>> AddAsync(int PollId, QuestionRequest request, CancellationToken cancellationToken)
    {
        var PollExists = await _context.Polls.AnyAsync(x => x.Id == PollId, cancellationToken);
        if (!PollExists)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var questionExists = await _context.Questions.AnyAsync(x => x.Content == request.Content && x.PollId == PollId, cancellationToken);
        if (questionExists)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        var question = request.Adapt<Question>();
        question.PollId = PollId;

        //request.Answers.ForEach(Answer => question.Answers.Add(new Answer { Content = Answer }));

        await _context.Questions.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
       await _hybridCache.RemoveAsync($"{_cachePrefix}-{PollId}", cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());

    }

    public async Task<Result> ToggleStatusAsync(int PollId, int Id, CancellationToken cancellationToken = default)
    {

        var question = await _context.Questions.SingleOrDefaultAsync(x => x.Id == Id && x.PollId == PollId, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.IsActive = !question.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(int PollId, int Id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var questionExists = await _context.Questions
            .AnyAsync(x => x.Id != Id
                && x.Content == request.Content
                && x.PollId == PollId,
                cancellationToken
            );

        if (questionExists)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        var question = await _context.Questions
            .Include(x => x.Answers)
            .SingleOrDefaultAsync(x => x.Id == Id && x.PollId == PollId, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

        question.Content = request.Content;

        //current answer
        var currentAnswer = question.Answers.Select(x => x.Content).ToList();
        //add new answer
        var newAnswer = request.Answers.Except(currentAnswer).ToList();

        newAnswer.ForEach(answer =>
        {
            question.Answers.Add(new Answer { Content = answer });
        });

        question.Answers.ToList().ForEach(answer =>
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        });

        await _context.SaveChangesAsync(cancellationToken);
        await _hybridCache.RemoveAsync($"{_cachePrefix}-{PollId}", cancellationToken);
        return Result.Success();
    }


}
