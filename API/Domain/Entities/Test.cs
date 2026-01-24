using API.Domain.Entities.QuestionTypes;
using API.Domain.Enums.Subject;
using System.ComponentModel.DataAnnotations.Schema;

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
    [Column("taken_at")]
    public required DateTime TakenAt { get; set; }
    [Column("secondary_subject_1")]
    public required Subject SecondarySubject1 { get; set; }
    [Column("secondary_subject_2")]
    public required Subject SecondarySubject2 { get; set; }
    [Column("kazakh_history_score") ]
    public required int KazakhHistoryScore { get; set; }
    [Column("functional_literacy_score")]
    public required int FunctionalLiteracyScore { get; set; }
    [Column("mathematical_literacy_score")]
    public required int MathematicalLiteracyScore { get; set; }
    [Column("secondary_subject_1_score")]
    public required int SecondarySubject1Score { get; set; }
    [Column("secondary_subject_2_score")]
    public required int SecondarySubject2Score { get; set; }
    [Column("total_score")]
    public required int TotalScore { get; set; }
}
[Table("test_results")]
public class TestResult: TestResultClient
{
    [Column("student_id")]
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
    public required bool[ , ] SingleChoiceAnswers { get; set; } = new bool[10 , 4];
    public required List<bool[,]> ContextAnswers { get; set; } = new List<bool[,]>();
}

public class FunctionalLiteracyTestAnswers
{
    public required List<bool[ , ]> ContextAnswers { get; set; }
}

public class MathematicalLiteracyTestAnswers
{
    public required bool[,] SingleChoiceAnswers { get; set; } = new bool[10 , 4];
}

public class SecondarySubjectTestAnswers
{
    public required bool[,] SingleChoiceAnswers { get; set; } = new bool[25, 4];
    public required bool[ , ] MultipleChoiceAnswers { get; set; } = new bool[5 , 6];
    public required bool[ , ] ContextAnswers { get; set; } = new bool[5 , 4];
    public required List<bool[ , ]> MatchAnswers { get; set; } = new List<bool[,]>();
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
    public required ContextQuestion ContextQuestion { get; set; }
    public required List<MatchQuestion> MatchQuestions { get; set; }

}