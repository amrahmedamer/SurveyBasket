﻿using FluentValidation;

namespace SurveyBasket.Api.Contracts.Vote;

public class VoteAnswerRequestValidator : AbstractValidator<VoteAnswerRequest>
{
    public VoteAnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .GreaterThan(0); 
        
        RuleFor(x => x.AnswerId)
            .GreaterThan(0);
    }
}
