namespace SurveyBasket.Api.Services;

public interface INotificationService
{
    Task SendNewPollNotification(int? pollId = null);
}
