using API.Domain.Entities.User;
using API.Domain.Entities.Test;
using API.Domain.Enums.UserRole;
using API.Application.Interfaces.Users.IStudent;
using API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using API.Domain.Entities;
using API.Domain.Enums.Subject;
using Google.Cloud.Firestore;
using API.Domain.Entities.QuestionTypes;

namespace API.Infrastructure.Implementations.StudentRepository;

public class StudentRepo: IStudentReader, IStudentWriter

{
    private readonly AppDbContext _context;
    private readonly FirestoreDb _db;
    public StudentRepo(AppDbContext context, FirestoreDb db)
    {
        _context = context;
        _db = db;
    }

    //Reader methods
    public async Task<List<GroupPublic>> GetGroupInvitesAsync(string studentId)
    {
        List<string> invites = new List<string>();
        invites = await _context.GroupJoinOrders
                    .Where(gi => gi.AcceptorId == studentId)
                    .Select(gi => gi.GroupId)
                    .ToListAsync();
        List<GroupPublic> groupInvites = new List<GroupPublic>();
        foreach (var groupId in invites)
        {
            var group = await _context.Groups.FindAsync(groupId)?? throw new Exception("Group not found");
            groupInvites.Add(new GroupPublic
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                CreatedAt = group.CreatedAt,
                GroupDescription = group.GroupDescription,
                GroupImageLink = group.GroupImageLink,
                TeacherUsername = group.TeacherUsername
            });
        }
        return groupInvites;
    }

    public async Task<List<TestResultClient>> GetTestResultsAsync(string studentId)
    {
        List<TestResultClient> results = new List<TestResultClient>();
        results = await _context.TestResults
                    .Where(tr => tr.StudentId == studentId)
                    .Select(tr => new TestResultClient
                    {
                        TakenAt = tr.TakenAt,
                        KazakhHistoryScore = tr.KazakhHistoryScore,
                        FunctionalLiteracyScore = tr.FunctionalLiteracyScore,
                        MathematicalLiteracyScore = tr.MathematicalLiteracyScore,
                        SecondarySubject1 = tr.SecondarySubject1,
                        SecondarySubject1Score = tr.SecondarySubject1Score,
                        SecondarySubject2 = tr.SecondarySubject2,
                        SecondarySubject2Score = tr.SecondarySubject2Score,
                        TotalScore = tr.TotalScore
                    })
                    .ToListAsync();
        return results;
    }

    public async Task<bool> isStudent(string Id)
    {
        var user = await _context.Users.FindAsync(Id);
        if (user != null && user.Role.ToString() == UserRole.Student.ToString())
            return true;
        return false;
    }

    public async Task<bool> IsInGroupAsync(string studentId, string groupId)
    {
        var doc = _db.Collection("Groups").Document(groupId);
        var snapshot = await doc.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            var students = snapshot.GetValue<List<string>>("StudentID");
            return students.Contains(studentId);
        }
        else
        {
            throw new Exception("Group not found");
        }

    }

    public async Task<Test> GetTestAsync(string studentId, Subject sub1, Subject sub2)
    {
        int KazakhHistoryBaseSize = _db.Collection("KazakhHistory").Document("Metadata").GetSnapshotAsync().Result.GetValue<int>("BaseSize");
        int FunctionalLiteracyBaseSize = _db.Collection("FunctionalLiteracy").Document("Metadata").GetSnapshotAsync().Result.GetValue<int>("BaseSize");
        int MathematicalLiteracyBaseSize = _db.Collection("MathematicalLiteracy").Document("Metadata").GetSnapshotAsync().Result.GetValue<int>("BaseSize");
        int Sub1BaseSize = _db.Collection(sub1.ToString()).Document("Metadata").GetSnapshotAsync().Result.GetValue<int>("BaseSize");
        int Sub2BaseSize = _db.Collection(sub2.ToString()).Document("Metadata").GetSnapshotAsync().Result.GetValue<int>("BaseSize");

        int KzHisBase = new Random().Next(1, KazakhHistoryBaseSize + 1);
        int FuncLitBase = new Random().Next(1, FunctionalLiteracyBaseSize + 1);
        int MathLitBase = new Random().Next(1, MathematicalLiteracyBaseSize + 1);
        int Sub1Base = new Random().Next(1, Sub1BaseSize + 1);
        int Sub2Base = new Random().Next(1, Sub2BaseSize + 1);

        int KzHisSingleChoiceSize = _db.Collection("KazakhHistory").Document("Single-"+KzHisBase.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int KzHisContextSize = _db.Collection("KazakhHistory").Document("Context-"+KzHisBase.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int FuncLitContextSize = _db.Collection("FunctionalLiteracy").Document("Context-"+FuncLitBase.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int MathLitSingleChoiceSize = _db.Collection("MathematicalLiteracy").Document("Single-"+MathLitBase.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int Sub1SingleChoiceSize = _db.Collection(sub1.ToString()).Document("Single-"+Sub1Base.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int Sub1MultipleChoiceSize = _db.Collection(sub1.ToString()).Document("Multiple-"+Sub1Base.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int Sub1ContextSize = _db.Collection(sub1.ToString()).Document("Context-"+Sub1Base.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int Sub1MatchSize = _db.Collection(sub1.ToString()).Document("Match-"+Sub1Base.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int Sub2SingleChoiceSize = _db.Collection(sub2.ToString()).Document("Single-"+Sub2Base.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int Sub2MultipleChoiceSize = _db.Collection(sub2.ToString()).Document("Multiple-"+Sub2Base.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int Sub2ContextSize = _db.Collection(sub2.ToString()).Document("Context-"+Sub2Base.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");
        int Sub2MatchSize = _db.Collection(sub2.ToString()).Document("Match-"+Sub2Base.ToString()).GetSnapshotAsync().Result.GetValue<int>("Count");

        int[] KzHisSingleChoiceIDs = Enumerable.Range(1, KzHisSingleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(10).ToArray();
        int[] KzHisContextIDs = Enumerable.Range(1, KzHisContextSize).OrderBy(_ => Random.Shared.Next()).Take(2).ToArray();
        int[] FuncLitContextIDs = Enumerable.Range(1, FuncLitContextSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();
        int[] MathLitSingleChoiceIDs = Enumerable.Range(1, MathLitSingleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(10).ToArray();
        int[] Sub1SingleChoiceIDs = Enumerable.Range(1, Sub1SingleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(25).ToArray();
        int[] Sub1MultipleChoiceIDs = Enumerable.Range(1, Sub1MultipleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();
        int Sub1ContextID = new Random().Next(1, Sub1ContextSize + 1);
        int[] Sub1MatchIDs = Enumerable.Range(1, Sub1MatchSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();
        int[] Sub2SingleChoiceIDs = Enumerable.Range(1, Sub2SingleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(25).ToArray();
        int[] Sub2MultipleChoiceIDs = Enumerable.Range(1, Sub2MultipleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();
        int Sub2ContextID = new Random().Next(1, Sub2ContextSize + 1);
        int[] Sub2MatchIDs = Enumerable.Range(1, Sub2MatchSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();

        var KzHisSingleChoiceDoc = _db.Collection("KazakhHistory").Document("Single-"+KzHisBase.ToString());
        var KzHisContextDoc = _db.Collection("KazakhHistory").Document("Context-"+KzHisBase.ToString());
        var FuncLitContextDoc = _db.Collection("FunctionalLiteracy").Document("Context-"+FuncLitBase.ToString());
        var MathLitSingleChoiceDoc = _db.Collection("MathematicalLiteracy").Document("Single-"+MathLitBase.ToString());
        var Sub1SingleChoiceDoc = _db.Collection(sub1.ToString()).Document("Single-"+Sub1Base.ToString());
        var Sub1MultipleChoiceDoc = _db.Collection(sub1.ToString()).Document("Multiple-"+Sub1Base.ToString());
        var Sub1ContextDoc = _db.Collection(sub1.ToString()).Document("Context-"+Sub1Base.ToString());
        var Sub1MatchDoc = _db.Collection(sub1.ToString()).Document("Match-"+Sub1Base.ToString());
        var Sub2SingleChoiceDoc = _db.Collection(sub2.ToString()).Document("Single-"+Sub2Base.ToString());
        var Sub2MultipleChoiceDoc = _db.Collection(sub2.ToString()).Document("Multiple-"+Sub2Base.ToString());
        var Sub2ContextDoc = _db.Collection(sub2.ToString()).Document("Context-"+Sub2Base.ToString());
        var Sub2MatchDoc = _db.Collection(sub2.ToString()).Document("Match-"+Sub2Base.ToString());

        List<SingleChoiceQuestion> KzHisSingleChoiceQuestions = new List<SingleChoiceQuestion>();
        foreach (var n in KzHisSingleChoiceIDs)
        {
            string q = KzHisSingleChoiceDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            KzHisSingleChoiceQuestions.Add(KzHisSingleChoiceDoc.GetSnapshotAsync().Result.GetValue<SingleChoiceQuestion>(q));
        }

        List<ContextQuestion> KzHisContextQuestions = new List<ContextQuestion>();
        foreach (var n in KzHisContextIDs)
        {
            string q = KzHisContextDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            KzHisContextQuestions.Add(KzHisContextDoc.GetSnapshotAsync().Result.GetValue<ContextQuestion>(q));
        }

        List<ContextQuestion> FuncLitContextQuestions = new List<ContextQuestion>();
        int number = 0;
        foreach (var n in FuncLitContextIDs)
        {
            if (number >= 10) break;

            string q = FuncLitContextDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            ContextQuestion context = FuncLitContextDoc.GetSnapshotAsync().Result.GetValue<ContextQuestion>(q);

            if (number + context.Questions.Count > 10)
            {
                ContextQuestion trimmedContext = new ContextQuestion
                {
                    Id = context.Id,
                    Subject = context.Subject,
                    ContextImageLink = context.ContextImageLink,
                    ContextText = context.ContextText,
                    Questions = context.Questions.Take(10 - n).ToList()
                };
                FuncLitContextQuestions.Add(trimmedContext);
            }
            else
            {
                FuncLitContextQuestions.Add(context);
                number += context.Questions.Count;
            }
        }

        List<SingleChoiceQuestion> MathLitSingleChoiceQuestions = new List<SingleChoiceQuestion>();
        foreach (var n in MathLitSingleChoiceIDs)
        {
            string q = MathLitSingleChoiceDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            MathLitSingleChoiceQuestions.Add(MathLitSingleChoiceDoc.GetSnapshotAsync().Result.GetValue<SingleChoiceQuestion>(q));
        }

        List<SingleChoiceQuestion> Sub1SingleChoiceQuestions = new List<SingleChoiceQuestion>();
        foreach (var n in Sub1SingleChoiceIDs)
        {
            string q = Sub1SingleChoiceDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            Sub1SingleChoiceQuestions.Add(Sub1SingleChoiceDoc.GetSnapshotAsync().Result.GetValue<SingleChoiceQuestion>(q));
        }

        List<MultipleChoiceQuestion> Sub1MultipleChoiceQuestions = new List<MultipleChoiceQuestion>();
        foreach (var n in Sub1MultipleChoiceIDs)
        {
            string q = Sub1MultipleChoiceDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            Sub1MultipleChoiceQuestions.Add(Sub1MultipleChoiceDoc.GetSnapshotAsync().Result.GetValue<MultipleChoiceQuestion>(q));
        }

        ContextQuestion Sub1ContextQuestion = Sub1ContextDoc.GetSnapshotAsync().Result.GetValue<ContextQuestion>(Sub1ContextDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[Sub1ContextID]);

        List<MatchQuestion> Sub1MatchQuestions = new List<MatchQuestion>();
        foreach (var n in Sub1MatchIDs)
        {
            string q = Sub1MatchDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            Sub1MatchQuestions.Add(Sub1MatchDoc.GetSnapshotAsync().Result.GetValue<MatchQuestion>(q));
        }

        List<SingleChoiceQuestion> Sub2SingleChoiceQuestions = new List<SingleChoiceQuestion>();
        foreach (var n in Sub2SingleChoiceIDs)
        {
            string q = Sub2SingleChoiceDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            Sub2SingleChoiceQuestions.Add(Sub2SingleChoiceDoc.GetSnapshotAsync().Result.GetValue<SingleChoiceQuestion>(q));
        }

        List<MultipleChoiceQuestion> Sub2MultipleChoiceQuestions = new List<MultipleChoiceQuestion>();
        foreach (var n in Sub2MultipleChoiceIDs)
        {
            string q = Sub2MultipleChoiceDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            Sub2MultipleChoiceQuestions.Add(Sub2MultipleChoiceDoc.GetSnapshotAsync().Result.GetValue<MultipleChoiceQuestion>(q));
        }

        ContextQuestion Sub2ContextQuestion = Sub2ContextDoc.GetSnapshotAsync().Result.GetValue<ContextQuestion>(Sub2ContextDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[Sub2ContextID]);

        List<MatchQuestion> Sub2MatchQuestions = new List<MatchQuestion>();
        foreach (var n in Sub2MatchIDs)
        {
            string q = Sub2MatchDoc.GetSnapshotAsync().Result.GetValue<List<string>>("IDs")[n];
            Sub2MatchQuestions.Add(Sub2MatchDoc.GetSnapshotAsync().Result.GetValue<MatchQuestion>(q));
        }
        
        Test test = new Test
        {
            KazakhHistory = new KazakhHistoryTest
            {
                SingleChoiceQuestions = KzHisSingleChoiceQuestions,
                ContextQuestions = KzHisContextQuestions
            },
            FunctionalLiteracy = new FunctionalLiteracyTest
            {
                ContextQuestions = FuncLitContextQuestions
            },
            MathematicalLiteracy = new MathematicalLiteracyTest
            {
                SingleChoiceQuestions = MathLitSingleChoiceQuestions
            },
            SecondarySubject1 = new SecondarySubjectTest
            {
                Subject = sub1,
                SingleChoiceQuestions = Sub1SingleChoiceQuestions,
                MultipleChoiceQuestions = Sub1MultipleChoiceQuestions,
                ContextQuestion = Sub1ContextQuestion,
                MatchQuestions = Sub1MatchQuestions
            },
            SecondarySubject2 = new SecondarySubjectTest
            {
                Subject = sub2,
                SingleChoiceQuestions = Sub2SingleChoiceQuestions,
                MultipleChoiceQuestions = Sub2MultipleChoiceQuestions,
                ContextQuestion = Sub2ContextQuestion,
                MatchQuestions = Sub2MatchQuestions
            }
            
        };

        return test;
    }

    public async Task<TestAnswers> GetTestAnswersAsync(string studentId, Test test)
    {
        //Get correct answers from Firesore for the given test
        //Question id is in format Base-"Something random"

        bool[,] KzHisSingleChoiceAnswers = new bool[10,4];
        for (int i = 0; i < 10; i++)
        {
            string qId = test.KazakhHistory.SingleChoiceQuestions[i].Id;
            //parse base number and question name in document
            var doc = _db.Collection("KazakhHistory").Document("Single-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<SingleChoiceQuestion>(qId);
            for (int j = 0; j < 4; j++)
            {
                KzHisSingleChoiceAnswers[i, j] = question.Options[j].IsCorrect;
            }
        }

        List<bool[,]> KzHisContextAnswers = new List<bool[,]>();
        for (int i = 0; i < 2; i++)
        {
            string qId = test.KazakhHistory.ContextQuestions[i].Id;
            var doc = _db.Collection("KazakhHistory").Document("Context-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<ContextQuestion>(qId);
            bool[,] contextAnswers = new bool[5,4];
            for (int j = 0; j < question.Questions.Count; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    contextAnswers[j, k] = question.Questions[j].Options[k].IsCorrect;
                }
            }
            KzHisContextAnswers.Add(contextAnswers);
        }

        List<bool[,]> FuncLitContextAnswers = new List<bool[,]>();
        for (int i = 0; i < test.FunctionalLiteracy.ContextQuestions.Count; i++)
        {
            string qId = test.FunctionalLiteracy.ContextQuestions[i].Id;
            var doc = _db.Collection("FunctionalLiteracy").Document("Context-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<ContextQuestion>(qId);
            bool[,] contextAnswers = new bool[question.Questions.Count,4];
            for (int j = 0; j < question.Questions.Count; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    contextAnswers[j, k] = question.Questions[j].Options[k].IsCorrect;
                }
            }
            FuncLitContextAnswers.Add(contextAnswers);
        }

        bool[,] MathLitSingleChoiceAnswers = new bool[10,4];
        for (int i = 0; i < 10; i++)
        {
            string qId = test.MathematicalLiteracy.SingleChoiceQuestions[i].Id;
            var doc = _db.Collection("MathematicalLiteracy").Document("Single-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<SingleChoiceQuestion>(qId);
            for (int j = 0; j < 4; j++)
            {
                MathLitSingleChoiceAnswers[i, j] = question.Options[j].IsCorrect;
            }
        }

        bool[,] Sub1SingleChoiceAnswers = new bool[25,4];
        for (int i = 0; i < 25; i++)
        {
            string qId = test.SecondarySubject1.SingleChoiceQuestions[i].Id;
            var doc = _db.Collection(test.SecondarySubject1.Subject.ToString()).Document("Single-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<SingleChoiceQuestion>(qId);
            for (int j = 0; j < 4; j++)
            {
                Sub1SingleChoiceAnswers[i, j] = question.Options[j].IsCorrect;
            }
        }

        bool[,] Sub1MultipleChoiceAnswers = new bool[5,6];
        for (int i = 0; i < 5; i++)
        {
            string qId = test.SecondarySubject1.MultipleChoiceQuestions[i].Id;
            var doc = _db.Collection(test.SecondarySubject1.Subject.ToString()).Document("Multiple-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<MultipleChoiceQuestion>(qId);
            for (int j = 0; j < 6; j++)
            {
                Sub1MultipleChoiceAnswers[i, j] = question.Options[j].IsCorrect;
            }
        }

        bool[,] Sub1ContextAnswers = new bool[5,4];
        {
            string qId = test.SecondarySubject1.ContextQuestion.Id;
            var doc = _db.Collection(test.SecondarySubject1.Subject.ToString()).Document("Context-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<ContextQuestion>(qId);
            for (int j = 0; j < question.Questions.Count; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    Sub1ContextAnswers[j, k] = question.Questions[j].Options[k].IsCorrect;
                }
            }
        }

        List<bool[,]> Sub1MatchAnswers = new List<bool[,]>();
        for (int i = 0; i < 5; i++)
        {
            string qId = test.SecondarySubject1.MatchQuestions[i].Id;
            var doc = _db.Collection(test.SecondarySubject1.Subject.ToString()).Document("Match-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<MatchQuestion>(qId);
            
            Sub1MatchAnswers.Add(question.CorrectMatches);
        }

        bool[,] Sub2SingleChoiceAnswers = new bool[25,4];
        for (int i = 0; i < 25; i++)
        {
            string qId = test.SecondarySubject2.SingleChoiceQuestions[i].Id;
            var doc = _db.Collection(test.SecondarySubject2.Subject.ToString()).Document("Single-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<SingleChoiceQuestion>(qId);
            for (int j = 0; j < 4; j++)
            {
                Sub2SingleChoiceAnswers[i, j] = question.Options[j].IsCorrect;
            }
        }

        bool[,] Sub2MultipleChoiceAnswers = new bool[5,6];
        for (int i = 0; i < 5; i++)
        {
            string qId = test.SecondarySubject2.MultipleChoiceQuestions[i].Id;
            var doc = _db.Collection(test.SecondarySubject2.Subject.ToString()).Document("Multiple-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<MultipleChoiceQuestion>(qId);
            for (int j = 0; j < 6; j++)
            {
                Sub2MultipleChoiceAnswers[i, j] = question.Options[j].IsCorrect;
            }
        }

        bool[,] Sub2ContextAnswers = new bool[5,4];
        {
            string qId = test.SecondarySubject2.ContextQuestion.Id;
            var doc = _db.Collection(test.SecondarySubject2.Subject.ToString()).Document("Context-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<ContextQuestion>(qId);
            for (int j = 0; j < question.Questions.Count; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    Sub2ContextAnswers[j, k] = question.Questions[j].Options[k].IsCorrect;
                }
            }
        }

        List<bool[,]> Sub2MatchAnswers = new List<bool[,]>();
        for (int i = 0; i < 5; i++)
        {
            string qId = test.SecondarySubject2.MatchQuestions[i].Id;
            var doc = _db.Collection(test.SecondarySubject2.Subject.ToString()).Document("Match-" + qId.Split("-")[0]);
            var question = doc.GetSnapshotAsync().Result.GetValue<MatchQuestion>(qId);
            
            Sub2MatchAnswers.Add(question.CorrectMatches);
        }

        TestAnswers answers = new TestAnswers
        {
            KazakhHistoryAnswers = new KazakhHistoryTestAnswers
            {
                SingleChoiceAnswers = KzHisSingleChoiceAnswers,
                ContextAnswers = KzHisContextAnswers
            },
            FunctionalLiteracyAnswers = new FunctionalLiteracyTestAnswers
            {
                ContextAnswers = FuncLitContextAnswers
            },
            MathematicalLiteracyAnswers = new MathematicalLiteracyTestAnswers
            {
                SingleChoiceAnswers = MathLitSingleChoiceAnswers
            },
            SecondarySubject1Answers = new SecondarySubjectTestAnswers
            {
                SingleChoiceAnswers = Sub1SingleChoiceAnswers,
                MultipleChoiceAnswers = Sub1MultipleChoiceAnswers,
                ContextAnswers = Sub1ContextAnswers,
                MatchAnswers = Sub1MatchAnswers
            },
            SecondarySubject2Answers = new SecondarySubjectTestAnswers
            {
                SingleChoiceAnswers = Sub2SingleChoiceAnswers,
                MultipleChoiceAnswers = Sub2MultipleChoiceAnswers,
                ContextAnswers = Sub2ContextAnswers,
                MatchAnswers = Sub2MatchAnswers
            }
        };

        return answers;
    }

    public async Task<List<GroupPublic>> GetGroups(string studentId)
    {
        var snap = _db.Collection("Students").Document(studentId).GetSnapshotAsync().Result;
        var groups = snap.GetValue<List<string>>("Groups");
        List<GroupPublic> groupPublics = new List<GroupPublic>();
        foreach (var groupId in groups)
        {
            var groupSnap = _db.Collection("Groups").Document(groupId).GetSnapshotAsync().Result;
            groupPublics.Add(new GroupPublic
            {
                GroupId = groupSnap.Id,
                GroupName = groupSnap.GetValue<string>("GroupName"),
                CreatedAt = groupSnap.GetValue<DateTime>("CreatedAt"),
                GroupDescription = groupSnap.GetValue<string>("GroupDescription"),
                GroupImageLink = groupSnap.GetValue<string>("GroupImageLink"),
                TeacherUsername = groupSnap.GetValue<string>("TeacherUsername")
            });
        }
        return groupPublics;
    }
    //Writer methods
    public async Task JoinGroupAsync(string studentId, string groupId)
    {
        Group group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId)?? throw new Exception("Group not found");
        string TeacherId = group.TeacherId;
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
        _context.GroupJoinOrders.Remove(invite);
        await _context.SaveChangesAsync();
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
        TestAnswers answers = new TestAnswers
        {
            KazakhHistoryAnswers = new KazakhHistoryTestAnswers
            {
                SingleChoiceAnswers = new bool[10, 4],
                ContextAnswers = new List<bool[,]>()
            },
            FunctionalLiteracyAnswers = new FunctionalLiteracyTestAnswers
            {
                ContextAnswers = new List<bool[,]>()
            },
            MathematicalLiteracyAnswers = new MathematicalLiteracyTestAnswers
            {
                SingleChoiceAnswers = new bool[10, 4]
            },
            SecondarySubject1Answers = new SecondarySubjectTestAnswers
            {
                SingleChoiceAnswers = new bool[25, 4],
                MultipleChoiceAnswers = new bool[5, 6],
                ContextAnswers = new bool[5, 4],
                MatchAnswers = new List<bool[,]>()
            },
            SecondarySubject2Answers = new SecondarySubjectTestAnswers
            {
                SingleChoiceAnswers = new bool[25, 4],
                MultipleChoiceAnswers = new bool[5, 6],
                ContextAnswers = new bool[5, 4],
                MatchAnswers = new List<bool[,]>()
            }
        };
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
        //Calculation logic: Compare booleans and assign scores
        //Kazakh History
        int kazakhHistoryScore = 20;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (studentAnswers.KazakhHistoryAnswers.SingleChoiceAnswers[i, j] != testAnswers.KazakhHistoryAnswers.SingleChoiceAnswers[i, j])
                {
                    kazakhHistoryScore -= 1;
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
                        kazakhHistoryScore -= 1;
                    }
                }
            }
        }

        //Functional Literacy
        int functionalScore = 10;
        for (int i = 0; i < testAnswers.FunctionalLiteracyAnswers.ContextAnswers.Count; i++)
        {
            for (int j = 0; j < testAnswers.FunctionalLiteracyAnswers.ContextAnswers[i].GetLength(0); j++)
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

        //Mathematical Literacy
        int mathLitScore = 10;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (studentAnswers.MathematicalLiteracyAnswers.SingleChoiceAnswers[i, j] != testAnswers.MathematicalLiteracyAnswers.SingleChoiceAnswers[i, j])
                {
                    mathLitScore -= 1;
                }
            }
        }

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

        TestResult result = new TestResult
        {
            StudentId = studentId,
            TakenAt = DateTime.UtcNow,
            KazakhHistoryScore = kazakhHistoryScore,
            FunctionalLiteracyScore = functionalScore,
            MathematicalLiteracyScore = mathLitScore,
            SecondarySubject1 = SecondarySubject1,
            SecondarySubject1Score = sec1Score,
            SecondarySubject2 = SecondarySubject2,
            SecondarySubject2Score = sec2Score,
            TotalScore = kazakhHistoryScore + functionalScore + mathLitScore + sec1Score + sec2Score
        };
        return result;
    }
}