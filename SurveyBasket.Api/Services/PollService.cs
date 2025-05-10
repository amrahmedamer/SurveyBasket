
using SurveyBasket.Api.Entities;

namespace SurveyBasket.Api.Services;

public class PollService : IPollService
{
    private readonly ApplicationDbContext _context;

    public PollService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
      await _context.Polls
            .AsNoTracking()
            .ProjectToType<PollResponse>()
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<PollResponse>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
      await _context.Polls
            .AsNoTracking()
            .Where(x => x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow))
            .ProjectToType<PollResponse>()
            .ToListAsync(cancellationToken);


    public async Task<Result<PollResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id, cancellationToken);
        return poll is not null
            ? Result.Success(poll.Adapt<PollResponse>())
            : Result.Failure<PollResponse>(PollErrors.PollNotFound);
    }
    public async Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken = default)
    {
        var poll = request.Adapt<Poll>();

        var isExistingTitle = await _context.Polls.AnyAsync(x => x.Title == poll.Title, cancellationToken);

        if (isExistingTitle)
            return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);


        if (poll is null)
            return Result.Failure<PollResponse>(PollErrors.PollNotFound);

        var result = await _context.Polls.AddAsync(poll, cancellationToken);
        var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

        if (rowsAffected > 0)
            return Result.Success(poll.Adapt<PollResponse>());
        else
            return Result.Failure<PollResponse>(new Error("DatabaseError", "No rows were affected.", StatusCodes.Status500InternalServerError));

    }

    public async Task<Result> UpdateAsync(int id, PollRequest request, CancellationToken cancellationToken = default)
    {
        var isExistingTitle = await _context.Polls.AnyAsync(x => x.Title == request.Title && x.Id != id, cancellationToken);

        if (isExistingTitle)
            return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);


        var poll = await _context.Polls.FindAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);


        poll.Title = request.Title;
        poll.Summery = request.Summery;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        _context.Polls.Remove(poll);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        poll.IsPublished = !poll.IsPublished;

        var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

        if (rowsAffected > 0)
            return Result.Success();
        else
            return Result.Failure(new Error("DatabaseError", "No rows were affected.", StatusCodes.Status500InternalServerError));
    }

}
