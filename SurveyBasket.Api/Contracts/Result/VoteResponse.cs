namespace SurveyBasket.Api.Contracts.Result
{
    public record VoteResponse(
        string VoterName,
        DateTime VoterDate,
        IEnumerable<QuestionAnswerResponse> SelectedAnswer
        );

}
