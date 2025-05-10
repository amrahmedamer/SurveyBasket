namespace SurveyBasket.Api.Services;

public interface IQuestionService
{
    Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int PollId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int PollId, string UserId, CancellationToken cancellationToken = default);
    Task<Result<QuestionResponse>> GetByIdAsync(int PollId,int Id, CancellationToken cancellationToken = default);
    Task<Result> ToggleStatusAsync(int PollId,int Id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int PollId,int Id, QuestionRequest request, CancellationToken cancellationToken = default);
    Task<Result<QuestionResponse>> AddAsync(int PollId, QuestionRequest request, CancellationToken cancellationToken = default);
}
