
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using SurveyBasket.Api.Helpers;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SurveyBasket.Api.Services;

public class NotificationService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender
    ) : INotificationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task SendNewPollNotification(int? pollId = null)
    {
        IEnumerable<Poll> polls = [];
        if (pollId.HasValue)
        {
            var poll = await _context.Polls.SingleOrDefaultAsync(x => x.Id == pollId & x.IsPublished);
            polls = [poll!];
        }
        else
        {

            var poll = await _context.Polls
                .Where(x => x.IsPublished & x.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
                .AsNoTracking()
                .ToListAsync();

            polls = poll;
        }
        //TODO: select member only
        var users = await _userManager.Users.ToListAsync();
        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        foreach (var poll in polls)
        {
            foreach (var user in users)
            {
                var bodyBuilder = EmailBodyBuilder.GenerateEmailBody("PollNotification", new Dictionary<string, string>
                {
                    {"{{name}}",user.FirstName},
                    {"{{pollTill}}",poll.Title},
                    {"{{endDate}}",poll.EndsAt.ToString()},
                    {"{{url}}",$"{origin}/api/poll/{poll.Id}/vote" }
                });

                await _emailSender.SendEmailAsync(user.Email!, $"🔊 Survey Basket: New Poll - {poll.Id}", bodyBuilder);

            }
        }
    }
}
