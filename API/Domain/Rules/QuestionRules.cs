using API.Domain.Entities.QuestionTypes;
using API.Domain.Enums.Subject;

namespace API.Domain.Rules.QuestionRules;

public static class QuestionValidator
{
    public static bool isSingleChoiceQuestionValid(SingleChoiceQuestion question)
    {
        if (question.Options.Count(o => o.IsCorrect) != 1) {return false;}
        else if (question.Options.Count != 4) {return false;}
        else if (string.IsNullOrEmpty(question.Text) && string.IsNullOrEmpty(question.ImageLink)) {return false;}
        else if (question.Options.Any(o => string.IsNullOrEmpty(o.Text) && string.IsNullOrEmpty(o.ImageLink))) {return false;}
        return true;
    }

    public static bool isMultipleChoiceQuestionValid(MultipleChoiceQuestion question)
    {
        if (question.Options.Count(o => o.IsCorrect) > 3 || question.Options.Count(o => o.IsCorrect) == 0) {return false;}
        else if (question.Options.Count != 6) {return false;}
        else if (string.IsNullOrEmpty(question.Text) && string.IsNullOrEmpty(question.ImageLink)) {return false;}
        else if (question.Options.Any(o => string.IsNullOrEmpty(o.Text) && string.IsNullOrEmpty(o.ImageLink))) {return false;}
        return true;
    }
    public static bool isContextQuestionValid(ContextQuestion question)
    {
        if (string.IsNullOrEmpty(question.ContextText) && string.IsNullOrEmpty(question.ContextImageLink)) {return false;}
        else if (question.Questions.Count > 10){return false;}
        else if (question.Questions.Any(q => !isSingleChoiceQuestionValid(q))){return false;}
        return true;
    }

    public static bool isContextQuestionValidForSecondarySubject(ContextQuestion question)
    {
        bool isOk = isContextQuestionValid(question);
        if (!isOk){return false;}
        else if (question.Questions.Count != 5){return false;}
        return true;
    }

    public static bool isMatchQuestionValid(MatchQuestion question)
    {
        if (string.IsNullOrEmpty(question.Text) && string.IsNullOrEmpty(question.ImageLink)) {return false;}
        else if (question.LeftSide.Count != 2){return false;}
        else if (question.RightSide.Count != 4){return false;}
        else if (question.Pairs.Count != 2){return false;}
        if (question.Pairs[0].Count != 2){return false;}
        if (question.Pairs[1].Count != 2){return false;}
        return true;
    }
}