using API.Domain.Entities.User;
using API.Domain.Entities.Test;
using API.Domain.Enums.UserRole;

namespace API.Infrastructure.Persistence.SQLdatabase.SQLite.StudentRepo;

public class StudentRepo: IStudentReader, IStudentWriter
{
    private readonly AppDbContext _context;
    public StudentRepo(AppDbContext context)
    {
        _context = context;
    }

    //Reader methods
    public async Task<List<string>> GetGroupInvitesAsync(string studentId)
    {
        List<string> invites = new List<string>();
        invites = await _context.GroupJoinOrders
                    .Where(gi => gi.AcceptorId == studentId)
                    .Select(gi => gi.GroupId)
                    .ToListAsync();
        return invites;
    }

    public async Task<List<TestResultClient>> GetTestResultsAsync(string studentId)
    {
        List<TestResultClient> results = new List<TestResultClient>();
        results = await _context.TestResults
                    .Where(tr => tr.StudentId == studentId)
                    .Select(tr => new TestResultClient
                    {
                        TakenAt = tr.TakenAt,
                        SecondarySubject1 = tr.SecondarySubject1,
                        SecondarySubject2 = tr.SecondarySubject2,
                        KazakhHistoryScore = tr.KazakhHistoryScore,
                        FunctionalLiteracyScore = tr.FunctionalLiteracyScore,
                        MathematicalLiteracyScore = tr.MathematicalLiteracyScore
                    })
                    .ToListAsync();
        return results;
    }

    public async Task<bool> isStudent(string Id)
    {
        var user = await _context.Users.FindAsync(Id);
        if (user != null && user.Role == UserRole.Student.ToString())
            return true;
        return false;
    }

    //Writer methods
    public async Task JoinGroupAsync(string studentId, string groupId)
    {
        string TeacherId = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId).TeacherId;
        await _context.GroupJoinOrders.AddAsync(new GroupJoinOrder
        {
            GroupId = groupId,
            AcceptorId = TeacherId,
            SenderId = studentId,
        });
    }

    public async Task AcceptGroupInviteAsync(string studentId, string groupId)
    {
        var invite = await _context.GroupJoinOrders.FirstOrDefaultAsync(gi => gi.GroupId == groupId && gi.AcceptorId == studentId);
        if (invite == null) throw new Exception("Invite not found");
        await _context.GroupJoinOrders.RemoveAsync(invite);
    }

    public async Task SubmitTestAsync(string studentId, TestResult testResult)
    {
        await _context.TestResults.AddAsync(testResult);
    }
}

public class StudentCalculator: IStudentCalculator
{
    public async Task<TestAnswers> GetStudentAnswersAsync(string studentId, Test test)
    {
        TestAnswers answers = new TestAnswers();
        for (int i = 0; i < test.KazakhHistory.SingleChoiceQuestions.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                answers.KazakhHistoryAnswers.SingleChoiceAnswers[i, j] = test.KazakhHistory.SingleChoiceQuestions[i].Options[j].IsCorrect;
            }
        }

        for (int i = 0; i < test.KazakhHistory.ContextQuestions.Count; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    answers.KazakhHistoryAnswers.ContextAnswers[i][j, k] = test.KazakhHistory.ContextQuestions[i].Questions[j].Options[k].IsCorrect;
                }
            }
        }

        for (int i = 0; i < test.FunctionalLiteracy.ContextQuestions.Count; i++)
        {
            for (int j = 0; j < test.FunctionalLiteracy.ContextQuestions[i].Questions.Count; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    answers.FunctionalLiteracyAnswers.ContextAnswers[i][j, k] = test.FunctionalLiteracy.ContextQuestions[i].Questions[j].Options[k].IsCorrect;
                }
            }
        }

        for (int i = 0; i < test.MathematicalLiteracy.SingleChoiceQuestions.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                answers.MathematicalLiteracyAnswers.SingleChoiceAnswers[i, j] = test.MathematicalLiteracy.SingleChoiceQuestions[i].Options[j].IsCorrect;
            }
        }

        for (int i = 0; i < test.SecondarySubject1.SingleChoiceQuestions.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                answers.SecondarySubject1Answers.SingleChoiceAnswers[i, j] = test.SecondarySubject1.SingleChoiceQuestions[i].Options[j].IsCorrect;
            }
        }

        for (int i = 0; i < test.SecondarySubject1.MultipleChoiceQuestions.Count; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                answers.SecondarySubject1Answers.MultipleChoiceAnswers[i, j] = test.SecondarySubject1.MultipleChoiceQuestions[i].Options[j].IsCorrect;
            }
        }

        for (int j = 0; j < 5; j++)
        {
            for (int k = 0; k < 4; k++)
            {
                answers.SecondarySubject1Answers.ContextAnswers[j, k] = test.SecondarySubject1.ContextQuestion.Questions[j].Options[k].IsCorrect;
            }
        }

        for (int i = 0; i < test.SecondarySubject2.SingleChoiceQuestions.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                answers.SecondarySubject2Answers.SingleChoiceAnswers[i, j] = test.SecondarySubject2.SingleChoiceQuestions[i].Options[j].IsCorrect;
            }
        }

        for (int i = 0; i < test.SecondarySubject2.MultipleChoiceQuestions.Count; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                answers.SecondarySubject2Answers.MultipleChoiceAnswers[i, j] = test.SecondarySubject2.MultipleChoiceQuestions[i].Options[j].IsCorrect;
            }
        }

        for (int j = 0; j < 5; j++)
        {
            for (int k = 0; k < 4; k++)
            {
                answers.SecondarySubject2Answers.ContextAnswers[j, k] = test.SecondarySubject2.ContextQuestion.Questions[j].Options[k].IsCorrect;
            }
        }

        for (int i = 0; i < test.SecondarySubject1.MatchQuestions.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    answers.SecondarySubject1Answers.MatchAnswers[i][j, k] = test.SecondarySubject1.MatchQuestions[i].CorrectMatches[j, k];
                }
            }
        }

        for (int i = 0; i < test.SecondarySubject2.MatchQuestions.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    answers.SecondarySubject2Answers.MatchAnswers[i][j, k] = test.SecondarySubject2.MatchQuestions[i].CorrectMatches[j, k];
                }
            }
        }

        return answers;
    }

    public async Task<TestResult> CalculateTestResultAsync(string studentId, 
                                            TestAnswers studentAnswers, 
                                            TestAnswers testAnswers, 
                                            Subject SecondarySubject1, 
                                            Subject SecondarySubject2)
    {
        TestResult result = new TestResult();
        result.StudentId = studentId;
        result.TakenAt = DateTime.UtcNow;
        //Calculation logic: Compare booleans and assign scores
        //Kazakh History
        int kazakhHistoryScore = 20;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (studentAnswers.KazakhHistoryAnswers.SingleChoiceAnswers[i, j] != testAnswers.KazakhHistoryAnswers.SingleChoiceAnswers[i, j])
                {
                    kazakhScore -= 1;
                }
            }
        }

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (studentAnswers.KazakhHistoryAnswers.ContextAnswers[i][j, k] != testAnswers.KazakhHistoryAnswers.ContextAnswers[i][j, k])
                    {
                        kazakhScore -= 1;
                    }
                }
            }
        }
        result.KazakhHistoryScore = kazakhHistoryScore;

        //Functional Literacy
        int functionalScore = 10;
        for (int i = 0; i < testAnswers.FunctionalLiteracyAnswers.ContextAnswers.Count; i++)
        {
            for (int j = 0; j < testAnswers.FunctionalLiteracyAnswers.ContextAnswers[i].Count; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (studentAnswers.FunctionalLiteracyAnswers.ContextAnswers[i][j, k] != testAnswers.FunctionalLiteracyAnswers.ContextAnswers[i][j, k])
                    {
                        functionalScore -= 1;
                    }
                }
            }
        }
        result.FunctionalLiteracyScore = functionalScore;

        //Mathematical Literacy
        int mathLitScore = 10;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (studentAnswers.MathematicalLiteracyAnswers.SingleChoiceAnswers[i, j] != testAnswers.MathematicalLiteracyAnswers.SingleChoiceAnswers[i, j])
                {
                    mathScore -= 1;
                }
            }
        }
        result.MathematicalLiteracyScore = mathLitScore;

        //Secondary Subject 1
        int sec1Score = 50;
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (studentAnswers.SecondarySubject1Answers.SingleChoiceAnswers[i, j] != testAnswers.SecondarySubject1Answers.SingleChoiceAnswers[i, j])
                {
                    sec1Score -= 1;
                }
            }
        }
        
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (studentAnswers.SecondarySubject1Answers.MultipleChoiceAnswers[i, j] != testAnswers.SecondarySubject1Answers.MultipleChoiceAnswers[i, j])
                {
                    sec1Score -= 1;
                }
            }
        }

        for (int j = 0; j < 5; j++)
        {
            for (int k = 0; k < 4; k++)
            {
                if (studentAnswers.SecondarySubject1Answers.ContextAnswers[j, k] != testAnswers.SecondarySubject1Answers.ContextAnswers[j, k])
                {
                    sec1Score -= 1;
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (studentAnswers.SecondarySubject1Answers.MatchAnswers[i][j, k] != testAnswers.SecondarySubject1Answers.MatchAnswers[i][j, k])
                    {
                        sec1Score -= 1;
                    }
                }
            }
        }
        result.SecondarySubject1 = SecondarySubject1;
        result.SecondarySubject1Score = sec1Score;

        //Secondary Subject 2
        int sec2Score = 50;
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (studentAnswers.SecondarySubject2Answers.SingleChoiceAnswers[i, j] != testAnswers.SecondarySubject2Answers.SingleChoiceAnswers[i, j])
                {
                    sec2Score -= 1;
                }
            }
        }
        
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (studentAnswers.SecondarySubject2Answers.MultipleChoiceAnswers[i, j] != testAnswers.SecondarySubject2Answers.MultipleChoiceAnswers[i, j])
                {
                    sec2Score -= 1;
                }
            }
        }

        for (int j = 0; j < 5; j++)
        {
            for (int k = 0; k < 4; k++)
            {
                if (studentAnswers.SecondarySubject2Answers.ContextAnswers[j, k] != testAnswers.SecondarySubject2Answers.ContextAnswers[j, k])
                {
                    sec2Score -= 1;
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (studentAnswers.SecondarySubject2Answers.MatchAnswers[i][j, k] != testAnswers.SecondarySubject2Answers.MatchAnswers[i][j, k])
                    {
                        sec2Score -= 1;
                    }
                }
            }
        }
        result.SecondarySubject2 = SecondarySubject2;
        result.SecondarySubject2Score = sec2Score;

        result.TotalScore = kazakhHistoryScore + functionalScore + mathLitScore + sec1Score + sec2Score;
        return result;
    }
}