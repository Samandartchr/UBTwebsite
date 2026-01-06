using API.Domain.Entities.Test;
using API.Domain.Rules.QuestionRules;

namespace API.Domain.Rules.TestRules;

public static class TestValidator
{
    public static bool isKazakhHistoryTestValid(KazakhHistoryTest test)
    {
        if (test.SingleChoiceQuestions.Count != 10) {return false;}
        else if (test.ContextQuestions.Count != 2) {return false;}
        return true;
    }

    public static bool isFunctionalLiteracyTestValid(FunctionalLiteracyTest test)
    {
        int n = 0;
        foreach (var context in test.ContextQuestions)
        {
            n += context.Questions.Count;
        }
        if (n != 10) {return false;}
        return true;
    }

    public static bool isMathematicalLiteracyTestValid(MathematicalLiteracyTest test)
    {
        if (test.SingleChoiceQuestions.Count != 10) {return false;}
        return true;
    }

    public static bool isSecondarySubjectTestValid(SecondarySubjectTest test)
    {
        if (test.SingleChoiceQuestions.Count != 25) {return false;}
        else if (test.MultipleChoiceQuestions.Count != 5) {return false;}
        else if (test.ContextQuestions.Count != 1) {return false;}
        else if (test.MatchQuestions.Count != 5) {return false;}
        return true;
    }
}