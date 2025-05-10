using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Api.Contracts.Result;

namespace SurveyBasket.Api.Services
{
    public class ResultService(ApplicationDbContext context) : IResultService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<Result<PollVoteResponse>> GetPollVoteAsync(int pollId, CancellationToken cancellationToken)
        {

            var pollVote = await _context.Polls
                .Where(x => x.Id == pollId)
                .Select(x => new PollVoteResponse(
                     x.Title,
                     x.Votes.Select(v => new VoteResponse(
                         $"{v.User.LastName} {v.User.LastName}",
                         v.SubmittedOn,
                         v.VoteAnswers.Select(a => new QuestionAnswerResponse(
                             a.Question.Content,
                             a.Answer.Content
                             ))
                     ))
                )).SingleOrDefaultAsync(cancellationToken);

            return pollVote is null
                ? Result.Failure<PollVoteResponse>(PollErrors.PollNotFound)
                : Result.Success(pollVote);
        }

        public async Task<Result<IEnumerable<VotePerDayResponse>>> GetVotePerDayAsync(int pollId, CancellationToken cancellationToken)
        {
            var pollIsExist = await _context.Polls.AnyAsync(x => x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

            if (!pollIsExist)
                return Result.Failure<IEnumerable<VotePerDayResponse>>(PollErrors.PollNotFound);

            var votesPerDay = await _context.Votes
                .Where(x => x.PollId == pollId)
                .GroupBy(x => new { Date = DateOnly.FromDateTime(x.SubmittedOn) })
                .Select(x => new VotePerDayResponse(
                    x.Key.Date,
                    x.Count()
                )).ToListAsync(cancellationToken);

            return votesPerDay is null
                ? Result.Failure<IEnumerable<VotePerDayResponse>>(VoteErrors.NotFound)
                : Result.Success<IEnumerable<VotePerDayResponse>>(votesPerDay);


        }

        public async Task<Result<IEnumerable<VotePerQuestionResponse>>> GetVotePerQuestionAsync(int pollId, CancellationToken cancellationToken)
        {
            var pollIsExist = await _context.Polls.AnyAsync(x => x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

            if (!pollIsExist)
                return Result.Failure<IEnumerable<VotePerQuestionResponse>>(PollErrors.PollNotFound);

            var votesPerQuestion = await _context.Questions
                .Where(x => x.PollId == pollId)
                .Select(x => new VotePerQuestionResponse(
                    x.Content,
                   x.VoteAnswers.GroupBy(x => new { AnswerId = x.Answer.Id, AnswerContent = x.Answer.Content })
                   .Select(x => new VotePerAnswerResponse(
                    x.Key.AnswerContent,
                    x.Count()
                    )
                   ))
                ).ToListAsync(cancellationToken);

            return votesPerQuestion is null
                ? Result.Failure<IEnumerable<VotePerQuestionResponse>>(VoteErrors.NotFound)
                : Result.Success<IEnumerable<VotePerQuestionResponse>>(votesPerQuestion);


        }
    }
}
