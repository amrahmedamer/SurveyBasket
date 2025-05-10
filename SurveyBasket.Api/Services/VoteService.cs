using Microsoft.EntityFrameworkCore;
using SurveyBasket.Api.Contracts.Vote;
using SurveyBasket.Api.Entities;
using System.Linq;

namespace SurveyBasket.Api.Services;

public class VoteService(ApplicationDbContext context) : IVoteService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result> AddAsync(int pollId, string userId, VoteRequest voteRequest, CancellationToken cancellationToken)
    {
        var hasVote = await _context.Votes.AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);
        if (hasVote)
            return Result.Failure(VoteErrors.DuplicatedVote);

        var pollIsExist = await _context.Polls.AnyAsync(x => x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        if (!pollIsExist)
            return Result.Failure(PollErrors.PollNotFound);

        var availableQuestion = await _context.Questions
            .Where(x => x.PollId == pollId && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (!voteRequest.Answer.Select(a => a.QuestionId).SequenceEqual(availableQuestion))
            return Result.Failure(VoteErrors.InValidQuestion);

        var vote = new Vote
        {
            PollId = pollId,
            UserId = userId,
            VoteAnswers = voteRequest.Answer.Adapt<IEnumerable<VoteAnswer>>().ToList()

        };

        await _context.AddAsync(vote, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();


    }
}
