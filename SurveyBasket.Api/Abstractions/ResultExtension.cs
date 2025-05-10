namespace SurveyBasket.Api.Abstractions;

public static class ResultExtension
{
    public static ObjectResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("cannot convert success result to a problem");

        var problem = Results.Problem(statusCode: result.Error.StatusCode);
        var problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails;
        problemDetails!.Extensions = new Dictionary<string, Object?>
        {
            {"error",result.Error}
        };

        //var problemDetails = new ProblemDetails()
        //{
        //    Status = statusCodes,
        //    Title = title,
        //    Extensions = new Dictionary<string, Object?>
        //    {

        //        {"error",result.Error}

        //    }
        //};
        return new ObjectResult(problemDetails);
    }
}
