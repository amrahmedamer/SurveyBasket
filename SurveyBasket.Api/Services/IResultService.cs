using SurveyBasket.Api.Contracts.Result;

namespace SurveyBasket.Api.Services
{
    public interface IResultService
    {
         Task<Result<PollVoteResponse>> GetPollVoteAsync(int pollId, CancellationToken cancellationToken);
        Task<Result<IEnumerable<VotePerDayResponse>>> GetVotePerDayAsync(int pollId, CancellationToken cancellationToken);
        Task<Result<IEnumerable<VotePerQuestionResponse>>> GetVotePerQuestionAsync(int pollId, CancellationToken cancellationToken);



    }
}
