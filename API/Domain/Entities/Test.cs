using API.Domain.Entities.QuestionTypes;
using API.Domain.Enums.Subject;

namespace API.Domain.Entities.Test;

public class Test
{
    public required KazakhHistoryTest KazakhHistory { get; set; }
    public required FunctionalLiteracyTest FunctionalLiteracy { get; set; }
    public required MathematicalLiteracyTest MathematicalLiteracy { get; set; }
    public required SecondarySubjectTest SecondarySubject1 { get; set; }
    public required SecondarySubjectTest SecondarySubject2 { get; set; }
}

public class TestResultClient
{
    public required DateTime TakenAt { get; set; }

    public required Subject SecondarySubject1 { get; set; }
    public required Subject SecondarySubject2 { get; set; }

    public required int KazakhHistoryScore { get; set; }
    public required int FunctionalLiteracyScore { get; set; }
    public required int MathematicalLiteracyScore { get; set; }
    public required int SecondarySubject1Score { get; set; }
    public required int SecondarySubject2Score { get; set; }

    public required int TotalScore { get; set; }
}

public class TestResult: TestResultClient
{
    public required string StudentId { get; set; }
}

public class TestAnswers
{
    public required KazakhHistoryTestAnswers KazakhHistoryAnswers { get; set; }
    public required FunctionalLiteracyTestAnswers FunctionalLiteracyAnswers { get; set; }
    public required MathematicalLiteracyTestAnswers MathematicalLiteracyAnswers { get; set; }
    public required SecondarySubjectTestAnswers SecondarySubject1Answers { get; set; }
    public required SecondarySubjectTestAnswers SecondarySubject2Answers { get; set; }
}

public class KazakhHistoryTestAnswers
{
    public required bool[ , ] SingleChoiceAnswers { get; set; } 
    public required List<bool[ , ]> ContextAnswers { get; set; }
}

public class FunctionalLiteracyTestAnswers
{
    public required List<bool[ , ]> ContextAnswers { get; set; }
}

public class MathematicalLiteracyTestAnswers
{
    public required bool[ , ] SingleChoiceAnswers { get; set; }
}

public class SecondarySubjectTestAnswers
{
    public required bool[ , ] SingleChoiceAnswers { get; set; }
    public required bool[ , ] MultipleChoiceAnswers { get; set; }
    public required List<bool[ , ]> ContextAnswers { get; set; }
    public required bool[ , ] MatchAnswers { get; set; }
}

public class TestResultPremium
{
    //Later
}

public class KazakhHistoryTest
{
    public required List<SingleChoiceQuestion> SingleChoiceQuestions { get; set; }
    public required List<ContextQuestion> ContextQuestions { get; set; }
}
public class FunctionalLiteracyTest
{
    public required List<ContextQuestion> ContextQuestions { get; set; }
}
public class MathematicalLiteracyTest
{
    public required List<SingleChoiceQuestion> SingleChoiceQuestions { get; set; }
}
public class SecondarySubjectTest
{
    public required Subject Subject { get; set; }
    public required List<SingleChoiceQuestion> SingleChoiceQuestions { get; set; }
    public required List<MultipleChoiceQuestion> MultipleChoiceQuestions { get; set; }
    public required List<ContextQuestion> ContextQuestions { get; set; }
    public required List<MatchQuestion> MatchQuestions { get; set; }

}