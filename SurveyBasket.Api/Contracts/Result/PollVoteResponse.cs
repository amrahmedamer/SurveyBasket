namespace SurveyBasket.Api.Contracts.Result
{
    public record PollVoteResponse(
        string Title,
        IEnumerable<VoteResponse> Votes
        
        );
   
}
