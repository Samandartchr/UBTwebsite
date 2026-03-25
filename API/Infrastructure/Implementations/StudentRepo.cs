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
        var snapshot = await _db.Collection("Groups").Document(groupId).GetSnapshotAsync();

        if (!snapshot.Exists)
            throw new Exception("Group not found");

        if (!snapshot.TryGetValue<List<string>>("Students", out var students) || students == null)
            return false;

        return students.Contains(studentId);
    }

    public async Task<Test> GetTestAsync(string studentId, Subject sub1, Subject sub2)
    {
        // ── 1. Получаем BaseSize для каждого предмета ─────────────────────────────
        var metaSnapshots = await Task.WhenAll(
            _db.Collection("KazakhstanHistory").Document("Metadata").GetSnapshotAsync(),
            _db.Collection("FunctionalLiteracy").Document("Metadata").GetSnapshotAsync(),
            _db.Collection("MathematicalLiteracy").Document("Metadata").GetSnapshotAsync(),
            _db.Collection(sub1.ToString()).Document("Metadata").GetSnapshotAsync(),
            _db.Collection(sub2.ToString()).Document("Metadata").GetSnapshotAsync()
        );

        int kazakhHistoryBaseSize = metaSnapshots[0].GetValue<int>("BaseSize");
        int functionalLiteracyBaseSize = metaSnapshots[1].GetValue<int>("BaseSize");
        int mathematicalLiteracyBase = metaSnapshots[2].GetValue<int>("BaseSize");
        int sub1BaseSize = metaSnapshots[3].GetValue<int>("BaseSize");
        int sub2BaseSize = metaSnapshots[4].GetValue<int>("BaseSize");

        // ── 2. Случайно выбираем номер базы для каждого предмета ─────────────────
        int kzHisBase = Random.Shared.Next(1, kazakhHistoryBaseSize + 1);
        int funcLitBase = Random.Shared.Next(1, functionalLiteracyBaseSize + 1);
        int mathLitBase = Random.Shared.Next(1, mathematicalLiteracyBase + 1);
        int sub1Base = Random.Shared.Next(1, sub1BaseSize + 1);
        int sub2Base = Random.Shared.Next(1, sub2BaseSize + 1);

        // ── 3. Получаем все нужные документы параллельно ─────────────────────────
        var docSnapshots = await Task.WhenAll(
            _db.Collection("KazakhstanHistory").Document("Single-" + kzHisBase).GetSnapshotAsync(),
            _db.Collection("KazakhstanHistory").Document("Context-" + kzHisBase).GetSnapshotAsync(),
            _db.Collection("FunctionalLiteracy").Document("Context-" + funcLitBase).GetSnapshotAsync(),
            _db.Collection("MathematicalLiteracy").Document("Single-" + mathLitBase).GetSnapshotAsync(),
            _db.Collection(sub1.ToString()).Document("Single-" + sub1Base).GetSnapshotAsync(),
            _db.Collection(sub1.ToString()).Document("Multiple-" + sub1Base).GetSnapshotAsync(),
            _db.Collection(sub1.ToString()).Document("Context-" + sub1Base).GetSnapshotAsync(),
            _db.Collection(sub1.ToString()).Document("Match-" + sub1Base).GetSnapshotAsync(),
            _db.Collection(sub2.ToString()).Document("Single-" + sub2Base).GetSnapshotAsync(),
            _db.Collection(sub2.ToString()).Document("Multiple-" + sub2Base).GetSnapshotAsync(),
            _db.Collection(sub2.ToString()).Document("Context-" + sub2Base).GetSnapshotAsync(),
            _db.Collection(sub2.ToString()).Document("Match-" + sub2Base).GetSnapshotAsync()
        );

        var kzHisSingleSnap = docSnapshots[0];
        var kzHisContextSnap = docSnapshots[1];
        var funcLitContextSnap = docSnapshots[2];
        var mathLitSingleSnap = docSnapshots[3];
        var sub1SingleSnap = docSnapshots[4];
        var sub1MultipleSnap = docSnapshots[5];
        var sub1ContextSnap = docSnapshots[6];
        var sub1MatchSnap = docSnapshots[7];
        var sub2SingleSnap = docSnapshots[8];
        var sub2MultipleSnap = docSnapshots[9];
        var sub2ContextSnap = docSnapshots[10];
        var sub2MatchSnap = docSnapshots[11];

        // ── 4. Считываем Count из каждого документа ───────────────────────────────
        int kzHisSingleChoiceSize = kzHisSingleSnap.GetValue<int>("Count");
        int kzHisContextSize = kzHisContextSnap.GetValue<int>("Count");
        int funcLitContextSize = funcLitContextSnap.GetValue<int>("Count");
        int mathLitSingleChoiceSize = mathLitSingleSnap.GetValue<int>("Count");
        int sub1SingleChoiceSize = sub1SingleSnap.GetValue<int>("Count");
        int sub1MultipleChoiceSize = sub1MultipleSnap.GetValue<int>("Count");
        int sub1ContextSize = sub1ContextSnap.GetValue<int>("Count");
        int sub1MatchSize = sub1MatchSnap.GetValue<int>("Count");
        int sub2SingleChoiceSize = sub2SingleSnap.GetValue<int>("Count");
        int sub2MultipleChoiceSize = sub2MultipleSnap.GetValue<int>("Count");
        int sub2ContextSize = sub2ContextSnap.GetValue<int>("Count");
        int sub2MatchSize = sub2MatchSnap.GetValue<int>("Count");

        // ── 5. Случайные индексы (FIX: Range(0, Size), не Range(1, Size)) ─────────
        int[] kzHisSingleChoiceIDs = Enumerable.Range(0, kzHisSingleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(10).ToArray();
        int[] kzHisContextIDs = Enumerable.Range(0, kzHisContextSize).OrderBy(_ => Random.Shared.Next()).Take(2).ToArray();
        int[] funcLitContextIDs = Enumerable.Range(0, funcLitContextSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();
        int[] mathLitSingleChoiceIDs = Enumerable.Range(0, mathLitSingleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(10).ToArray();
        int[] sub1SingleChoiceIDs = Enumerable.Range(0, sub1SingleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(25).ToArray();
        int[] sub1MultipleChoiceIDs = Enumerable.Range(0, sub1MultipleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();
        int sub1ContextID = Random.Shared.Next(0, sub1ContextSize);
        int[] sub1MatchIDs = Enumerable.Range(0, sub1MatchSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();
        int[] sub2SingleChoiceIDs = Enumerable.Range(0, sub2SingleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(25).ToArray();
        int[] sub2MultipleChoiceIDs = Enumerable.Range(0, sub2MultipleChoiceSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();
        int sub2ContextID = Random.Shared.Next(0, sub2ContextSize);
        int[] sub2MatchIDs = Enumerable.Range(0, sub2MatchSize).OrderBy(_ => Random.Shared.Next()).Take(5).ToArray();

        // ── 6. Формируем списки вопросов (снэпшот берётся один раз, не в цикле) ──

        // KazakhHistory — Single Choice
        var kzHisSingleIDs = kzHisSingleSnap.GetValue<List<string>>("IDs");
        var kzHisSingleChoiceQuestions = kzHisSingleChoiceIDs
            .Select(n => kzHisSingleSnap.GetValue<SingleChoiceQuestion>(kzHisSingleIDs[n]))
            .ToList();

        // KazakhHistory — Context
        var kzHisContextIDs_list = kzHisContextSnap.GetValue<List<string>>("IDs");
        var kzHisContextQuestions = kzHisContextIDs
            .Select(n => kzHisContextSnap.GetValue<ContextQuestion>(kzHisContextIDs_list[n]))
            .ToList();

        // FunctionalLiteracy — Context (набираем ровно 10 подвопросов)
        var funcLitIDs = funcLitContextSnap.GetValue<List<string>>("IDs");
        var funcLitContextQuestions = new List<ContextQuestion>();
        int totalFuncLitSubQs = 0;

        foreach (var n in funcLitContextIDs)
        {
            if (totalFuncLitSubQs >= 10) break;

            var context = funcLitContextSnap.GetValue<ContextQuestion>(funcLitIDs[n]);
            int remaining = 10 - totalFuncLitSubQs;

            if (context.Questions.Count > remaining)
            {
                // FIX: Take(remaining) — не Take(10 - n)
                funcLitContextQuestions.Add(new ContextQuestion
                {
                    Id = context.Id,
                    Subject = context.Subject,
                    ContextText = context.ContextText,
                    ContextImageLink = context.ContextImageLink,
                    Questions = context.Questions.Take(remaining).ToList()
                });
                totalFuncLitSubQs += remaining;
            }
            else
            {
                funcLitContextQuestions.Add(context);
                totalFuncLitSubQs += context.Questions.Count;
            }
        }

        // MathematicalLiteracy — Single Choice
        var mathLitIDs = mathLitSingleSnap.GetValue<List<string>>("IDs");
        var mathLitSingleChoiceQuestions = mathLitSingleChoiceIDs
            .Select(n => mathLitSingleSnap.GetValue<SingleChoiceQuestion>(mathLitIDs[n]))
            .ToList();

        // Sub1
        var sub1SingleIDs = sub1SingleSnap.GetValue<List<string>>("IDs");
        var sub1MultipleIDs = sub1MultipleSnap.GetValue<List<string>>("IDs");
        var sub1ContextIDs = sub1ContextSnap.GetValue<List<string>>("IDs");
        var sub1MatchIDs_list = sub1MatchSnap.GetValue<List<string>>("IDs");

        var sub1SingleChoiceQuestions = sub1SingleChoiceIDs
            .Select(n => sub1SingleSnap.GetValue<SingleChoiceQuestion>(sub1SingleIDs[n]))
            .ToList();

        var sub1MultipleChoiceQuestions = sub1MultipleChoiceIDs
            .Select(n => sub1MultipleSnap.GetValue<MultipleChoiceQuestion>(sub1MultipleIDs[n]))
            .ToList();

        var sub1ContextQuestion = sub1ContextSnap.GetValue<ContextQuestion>(sub1ContextIDs[sub1ContextID]);

        var sub1MatchQuestions = sub1MatchIDs
            .Select(n => sub1MatchSnap.GetValue<MatchQuestion>(sub1MatchIDs_list[n]))
            .ToList();

        // Sub2
        var sub2SingleIDs = sub2SingleSnap.GetValue<List<string>>("IDs");
        var sub2MultipleIDs = sub2MultipleSnap.GetValue<List<string>>("IDs");
        var sub2ContextIDs = sub2ContextSnap.GetValue<List<string>>("IDs");
        var sub2MatchIDs_list = sub2MatchSnap.GetValue<List<string>>("IDs");

        var sub2SingleChoiceQuestions = sub2SingleChoiceIDs
            .Select(n => sub2SingleSnap.GetValue<SingleChoiceQuestion>(sub2SingleIDs[n]))
            .ToList();

        var sub2MultipleChoiceQuestions = sub2MultipleChoiceIDs
            .Select(n => sub2MultipleSnap.GetValue<MultipleChoiceQuestion>(sub2MultipleIDs[n]))
            .ToList();

        var sub2ContextQuestion = sub2ContextSnap.GetValue<ContextQuestion>(sub2ContextIDs[sub2ContextID]);

        var sub2MatchQuestions = sub2MatchIDs
            .Select(n => sub2MatchSnap.GetValue<MatchQuestion>(sub2MatchIDs_list[n]))
            .ToList();

        // ── 7. Собираем тест ──────────────────────────────────────────────────────
        return new Test
        {
            KazakhHistory = new KazakhHistoryTest
            {
                SingleChoiceQuestions = kzHisSingleChoiceQuestions,
                ContextQuestions = kzHisContextQuestions
            },
            FunctionalLiteracy = new FunctionalLiteracyTest
            {
                ContextQuestions = funcLitContextQuestions
            },
            MathematicalLiteracy = new MathematicalLiteracyTest
            {
                SingleChoiceQuestions = mathLitSingleChoiceQuestions
            },
            SecondarySubject1 = new SecondarySubjectTest
            {
                Subject = sub1,
                SingleChoiceQuestions = sub1SingleChoiceQuestions,
                MultipleChoiceQuestions = sub1MultipleChoiceQuestions,
                ContextQuestion = sub1ContextQuestion,
                MatchQuestions = sub1MatchQuestions
            },
            SecondarySubject2 = new SecondarySubjectTest
            {
                Subject = sub2,
                SingleChoiceQuestions = sub2SingleChoiceQuestions,
                MultipleChoiceQuestions = sub2MultipleChoiceQuestions,
                ContextQuestion = sub2ContextQuestion,
                MatchQuestions = sub2MatchQuestions
            }
        };
    }
    public async Task<TestAnswers> GetTestAnswersAsync(string studentId, Test test)
    {
        static bool[,] FlatToMatrix(MatchQuestion q)
        {
            int L = q.LeftSide.Count, R = q.RightSide.Count;
            var matrix = new bool[L, R];
            for (int r = 0; r < L; r++)
                for (int c = 0; c < R; c++)
                    matrix[r, c] = q.CorrectMatches[r * R + c];
            return matrix;
        }

        static bool[,] SingleChoiceMatrix(List<SingleChoiceQuestion> questions)
        {
            var result = new bool[questions.Count, 4];
            for (int i = 0; i < questions.Count; i++)
                for (int j = 0; j < 4; j++)
                    result[i, j] = questions[i].Options[j].IsCorrect;
            return result;
        }

        static bool[,] MultipleChoiceMatrix(List<MultipleChoiceQuestion> questions)
        {
            var result = new bool[questions.Count, 6];
            for (int i = 0; i < questions.Count; i++)
                for (int j = 0; j < 6; j++)
                    result[i, j] = questions[i].Options[j].IsCorrect;
            return result;
        }

        static List<bool[,]> ContextMatrixList(List<ContextQuestion> questions)
        {
            var result = new List<bool[,]>();
            foreach (var ctx in questions)
            {
                var matrix = new bool[ctx.Questions.Count, 4];
                for (int j = 0; j < ctx.Questions.Count; j++)
                    for (int k = 0; k < 4; k++)
                        matrix[j, k] = ctx.Questions[j].Options[k].IsCorrect;
                result.Add(matrix);
            }
            return result;
        }

        static bool[,] ContextMatrix(ContextQuestion ctx)
        {
            var matrix = new bool[ctx.Questions.Count, 4];
            for (int j = 0; j < ctx.Questions.Count; j++)
                for (int k = 0; k < 4; k++)
                    matrix[j, k] = ctx.Questions[j].Options[k].IsCorrect;
            return matrix;
        }

        string sub1 = test.SecondarySubject1.Subject.ToString();
        string sub2 = test.SecondarySubject2.Subject.ToString();

        // ── 1. Fetch all Metadata in parallel ────────────────────────────────────
        var metaSnaps = await Task.WhenAll(
            _db.Collection("KazakhstanHistory").Document("Metadata").GetSnapshotAsync(),
            _db.Collection("FunctionalLiteracy").Document("Metadata").GetSnapshotAsync(),
            _db.Collection("MathematicalLiteracy").Document("Metadata").GetSnapshotAsync(),
            _db.Collection(sub1).Document("Metadata").GetSnapshotAsync(),
            _db.Collection(sub2).Document("Metadata").GetSnapshotAsync()
        );

        int kzHisBaseSize = metaSnaps[0].GetValue<int>("BaseSize");
        int funcLitBaseSize = metaSnaps[1].GetValue<int>("BaseSize");
        int mathLitBaseSize = metaSnaps[2].GetValue<int>("BaseSize");
        int sub1BaseSize = metaSnaps[3].GetValue<int>("BaseSize");
        int sub2BaseSize = metaSnaps[4].GetValue<int>("BaseSize");

        // ── 2. Helper: fetch all docs of a type and find the one containing our ID ─
        async Task<DocumentSnapshot> FindBaseDoc(string collection, string type, int baseSize, string targetId)
        {
            var tasks = Enumerable.Range(1, baseSize)
                .Select(i => _db.Collection(collection).Document($"{type}-{i}").GetSnapshotAsync());
            var snaps = await Task.WhenAll(tasks);
            return snaps.First(s =>
            {
                var ids = s.GetValue<List<string>>("IDs");
                return ids != null && ids.Contains(targetId);
            });
        }

        // ── 3. Find base documents for each section in parallel ───────────────────
        var findTasks = await Task.WhenAll(
            FindBaseDoc("KazakhstanHistory", "Single", kzHisBaseSize, test.KazakhHistory.SingleChoiceQuestions[0].Id),
            FindBaseDoc("KazakhstanHistory", "Context", kzHisBaseSize, test.KazakhHistory.ContextQuestions[0].Id),
            FindBaseDoc("FunctionalLiteracy", "Context", funcLitBaseSize, test.FunctionalLiteracy.ContextQuestions[0].Id),
            FindBaseDoc("MathematicalLiteracy", "Single", mathLitBaseSize, test.MathematicalLiteracy.SingleChoiceQuestions[0].Id),
            FindBaseDoc(sub1, "Single", sub1BaseSize, test.SecondarySubject1.SingleChoiceQuestions[0].Id),
            FindBaseDoc(sub1, "Multiple", sub1BaseSize, test.SecondarySubject1.MultipleChoiceQuestions[0].Id),
            FindBaseDoc(sub1, "Context", sub1BaseSize, test.SecondarySubject1.ContextQuestion.Id),
            FindBaseDoc(sub1, "Match", sub1BaseSize, test.SecondarySubject1.MatchQuestions[0].Id),
            FindBaseDoc(sub2, "Single", sub2BaseSize, test.SecondarySubject2.SingleChoiceQuestions[0].Id),
            FindBaseDoc(sub2, "Multiple", sub2BaseSize, test.SecondarySubject2.MultipleChoiceQuestions[0].Id),
            FindBaseDoc(sub2, "Context", sub2BaseSize, test.SecondarySubject2.ContextQuestion.Id),
            FindBaseDoc(sub2, "Match", sub2BaseSize, test.SecondarySubject2.MatchQuestions[0].Id)
        );

        var kzHisSingleSnap = findTasks[0];
        var kzHisContextSnap = findTasks[1];
        var funcLitContextSnap = findTasks[2];
        var mathLitSingleSnap = findTasks[3];
        var sub1SingleSnap = findTasks[4];
        var sub1MultipleSnap = findTasks[5];
        var sub1ContextSnap = findTasks[6];
        var sub1MatchSnap = findTasks[7];
        var sub2SingleSnap = findTasks[8];
        var sub2MultipleSnap = findTasks[9];
        var sub2ContextSnap = findTasks[10];
        var sub2MatchSnap = findTasks[11];

        // ── 4. Read real answers from Firestore by question ID ────────────────────
        var kzHisSingleQuestions = test.KazakhHistory.SingleChoiceQuestions
            .Select(q => kzHisSingleSnap.GetValue<SingleChoiceQuestion>(q.Id)).ToList();

        var kzHisContextQuestions = test.KazakhHistory.ContextQuestions
            .Select(q => kzHisContextSnap.GetValue<ContextQuestion>(q.Id)).ToList();

        var funcLitContextQuestions = test.FunctionalLiteracy.ContextQuestions
    .Select(q =>
    {
        var full = funcLitContextSnap.GetValue<ContextQuestion>(q.Id);
        return new ContextQuestion
        {
            Id = full.Id,
            Subject = full.Subject,
            ContextText = full.ContextText,
            ContextImageLink = full.ContextImageLink,
            Questions = full.Questions.Take(q.Questions.Count).ToList() // trim to match submitted
        };
    }).ToList();

        var mathLitSingleQuestions = test.MathematicalLiteracy.SingleChoiceQuestions
            .Select(q => mathLitSingleSnap.GetValue<SingleChoiceQuestion>(q.Id)).ToList();

        var sub1SingleQuestions = test.SecondarySubject1.SingleChoiceQuestions
            .Select(q => sub1SingleSnap.GetValue<SingleChoiceQuestion>(q.Id)).ToList();

        var sub1MultipleQuestions = test.SecondarySubject1.MultipleChoiceQuestions
            .Select(q => sub1MultipleSnap.GetValue<MultipleChoiceQuestion>(q.Id)).ToList();

        var sub1ContextQuestion = sub1ContextSnap
            .GetValue<ContextQuestion>(test.SecondarySubject1.ContextQuestion.Id);

        var sub1MatchQuestions = test.SecondarySubject1.MatchQuestions
            .Select(q => sub1MatchSnap.GetValue<MatchQuestion>(q.Id)).ToList();

        var sub2SingleQuestions = test.SecondarySubject2.SingleChoiceQuestions
            .Select(q => sub2SingleSnap.GetValue<SingleChoiceQuestion>(q.Id)).ToList();

        var sub2MultipleQuestions = test.SecondarySubject2.MultipleChoiceQuestions
            .Select(q => sub2MultipleSnap.GetValue<MultipleChoiceQuestion>(q.Id)).ToList();

        var sub2ContextQuestion = sub2ContextSnap
            .GetValue<ContextQuestion>(test.SecondarySubject2.ContextQuestion.Id);

        var sub2MatchQuestions = test.SecondarySubject2.MatchQuestions
            .Select(q => sub2MatchSnap.GetValue<MatchQuestion>(q.Id)).ToList();

        // ── 5. Build answer matrices ──────────────────────────────────────────────
        return new TestAnswers
        {
            KazakhHistoryAnswers = new KazakhHistoryTestAnswers
            {
                SingleChoiceAnswers = SingleChoiceMatrix(kzHisSingleQuestions),
                ContextAnswers = ContextMatrixList(kzHisContextQuestions)
            },
            FunctionalLiteracyAnswers = new FunctionalLiteracyTestAnswers
            {
                ContextAnswers = ContextMatrixList(funcLitContextQuestions)
            },
            MathematicalLiteracyAnswers = new MathematicalLiteracyTestAnswers
            {
                SingleChoiceAnswers = SingleChoiceMatrix(mathLitSingleQuestions)
            },
            SecondarySubject1Answers = new SecondarySubjectTestAnswers
            {
                SingleChoiceAnswers = SingleChoiceMatrix(sub1SingleQuestions),
                MultipleChoiceAnswers = MultipleChoiceMatrix(sub1MultipleQuestions),
                ContextAnswers = ContextMatrix(sub1ContextQuestion),
                MatchAnswers = sub1MatchQuestions.Select(q => FlatToMatrix(q)).ToList()
            },
            SecondarySubject2Answers = new SecondarySubjectTestAnswers
            {
                SingleChoiceAnswers = SingleChoiceMatrix(sub2SingleQuestions),
                MultipleChoiceAnswers = MultipleChoiceMatrix(sub2MultipleQuestions),
                ContextAnswers = ContextMatrix(sub2ContextQuestion),
                MatchAnswers = sub2MatchQuestions.Select(q => FlatToMatrix(q)).ToList()
            }
        };
    }
    public async Task<List<GroupPublic>> GetStudentGroups(string studentId)
    {
        var snap = await _db.Collection("Students")
            .Document(studentId)
            .GetSnapshotAsync();

        if (!snap.Exists) return new List<GroupPublic>();

        var groups = snap.GetValue<List<string>>("Groups") ?? new List<string>();

        var tasks = groups.Select(async groupId =>
        {
            var groupSnap = await _db.Collection("Groups")
                .Document(groupId)
                .GetSnapshotAsync();

            return new GroupPublic
            {
                GroupId = groupSnap.Id,
                GroupName = groupSnap.GetValue<string>("GroupName"),
                CreatedAt = groupSnap.GetValue<DateTime>("CreatedAt"),
                GroupDescription = groupSnap.GetValue<string>("GroupDescription"),
                GroupImageLink = groupSnap.GetValue<string>("GroupImageLink"),
                TeacherUsername = groupSnap.GetValue<string>("TeacherUsername")
            };
        });

        return (await Task.WhenAll(tasks)).ToList();
    }
    //Writer methods
    public async Task JoinGroupAsync(string studentId, string groupId)
    {
        Group group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId)?? throw new Exception("Group not found");
        string TeacherId = group.TeacherId;
        if(await _context.GroupJoinOrders.AnyAsync(o => o.GroupId == groupId && o.SenderId == TeacherId && o.AcceptorId == studentId))
        {
            throw new Exception("Teacher already sent invitation to student");
        }
        if(await _context.GroupJoinOrders.AnyAsync(o => o.GroupId == groupId && o.SenderId == studentId && o.AcceptorId == TeacherId))
        {
            throw new Exception("Student already sent invitation to teacher");
        }
        await _context.GroupJoinOrders.AddAsync(new GroupJoinOrder
        {
            GroupId = groupId,
            AcceptorId = TeacherId,
            SenderId = studentId,
        });
        await _context.SaveChangesAsync();
    }

    public async Task AcceptGroupInviteAsync(string studentId, string groupId)
    {
        var invite = await _context.GroupJoinOrders.FirstOrDefaultAsync(gi => gi.GroupId == groupId && gi.AcceptorId == studentId);
        if (invite == null) throw new Exception("Invite not found");

        _context.GroupJoinOrders.Remove(invite);
        await _context.SaveChangesAsync();

        bool isInGroup = await IsInGroupAsync(studentId, groupId);
        if (isInGroup) throw new Exception("Student is already in the group");

        var doc = _db.Collection("Groups").Document(groupId);
        var docc = _db.Collection("Students").Document(studentId);

        var snapshot = await doc.GetSnapshotAsync();
        var snap = await docc.GetSnapshotAsync();

        if (!snapshot.Exists || !snap.Exists)
            throw new Exception("Group or Student document not found");

        // SetAsync with merge creates the field if it doesn't exist
        await doc.SetAsync(new { Students = FieldValue.ArrayUnion(studentId) }, SetOptions.MergeAll);
        await docc.SetAsync(new { Groups = FieldValue.ArrayUnion(groupId) }, SetOptions.MergeAll);
    }

    public async Task SubmitTestAsync(string studentId, TestResult testResult)
    {
        await _context.TestResults.AddAsync(testResult);
        await _context.SaveChangesAsync();
        await _db.Collection("Students").Document(studentId).UpdateAsync("LastTimeTest", DateTime.UtcNow);
        //in document there is TestResults array, add new test result to it
        await _db.Collection("Students").Document(studentId).UpdateAsync("TestResults", FieldValue.ArrayUnion(testResult));
    }
}

public class StudentCalculator: IStudentCalculator
{
    public async Task<TestAnswers> GetStudentAnswersAsync(string studentId, Test test)
    {
        // ── Вспомогательный метод: CorrectMatchesFlat → bool[,] ──────────────────
        // FIX: CorrectMatches удалено, читаем CorrectMatchesFlat
        static bool[,] FlatToMatrix(MatchQuestion q)
        {
            int L = q.LeftSide.Count;
            int R = q.RightSide.Count;
            var matrix = new bool[L, R];
            for (int r = 0; r < L; r++)
                for (int c = 0; c < R; c++)
                    matrix[r, c] = q.CorrectMatches[r * R + c];
            return matrix;
        }

        // ── KazakhHistory — SingleChoice ──────────────────────────────────────────
        var kzHisSingle = new bool[test.KazakhHistory.SingleChoiceQuestions.Count, 4];
        for (int i = 0; i < test.KazakhHistory.SingleChoiceQuestions.Count; i++)
            for (int j = 0; j < 4; j++)
                kzHisSingle[i, j] = test.KazakhHistory.SingleChoiceQuestions[i].Options[j].IsCorrect;

        // ── KazakhHistory — Context ───────────────────────────────────────────────
        // FIX: в оригинале List инициализирован пустым, но затем индексируется [i] —
        //      IndexOutOfRangeException. Нужно добавлять матрицу в список через Add().
        var kzHisContext = new List<bool[,]>();
        for (int i = 0; i < test.KazakhHistory.ContextQuestions.Count; i++)
        {
            var ctx = test.KazakhHistory.ContextQuestions[i];
            var matrix = new bool[ctx.Questions.Count, 4];
            for (int j = 0; j < ctx.Questions.Count; j++)
                for (int k = 0; k < 4; k++)
                    matrix[j, k] = ctx.Questions[j].Options[k].IsCorrect;
            kzHisContext.Add(matrix);
        }

        // ── FunctionalLiteracy — Context ──────────────────────────────────────────
        // FIX: та же проблема — Add() вместо прямого индексирования
        var funcLitContext = new List<bool[,]>();
        for (int i = 0; i < test.FunctionalLiteracy.ContextQuestions.Count; i++)
        {
            var ctx = test.FunctionalLiteracy.ContextQuestions[i];
            var matrix = new bool[ctx.Questions.Count, 4];
            for (int j = 0; j < ctx.Questions.Count; j++)
                for (int k = 0; k < 4; k++)
                    matrix[j, k] = ctx.Questions[j].Options[k].IsCorrect;
            funcLitContext.Add(matrix);
        }

        // ── MathematicalLiteracy — SingleChoice ───────────────────────────────────
        var mathLitSingle = new bool[test.MathematicalLiteracy.SingleChoiceQuestions.Count, 4];
        for (int i = 0; i < test.MathematicalLiteracy.SingleChoiceQuestions.Count; i++)
            for (int j = 0; j < 4; j++)
                mathLitSingle[i, j] = test.MathematicalLiteracy.SingleChoiceQuestions[i].Options[j].IsCorrect;

        // ── SecondarySubject1 ─────────────────────────────────────────────────────
        var sub1Single = new bool[test.SecondarySubject1.SingleChoiceQuestions.Count, 4];
        for (int i = 0; i < test.SecondarySubject1.SingleChoiceQuestions.Count; i++)
            for (int j = 0; j < 4; j++)
                sub1Single[i, j] = test.SecondarySubject1.SingleChoiceQuestions[i].Options[j].IsCorrect;

        var sub1Multiple = new bool[test.SecondarySubject1.MultipleChoiceQuestions.Count, 6];
        for (int i = 0; i < test.SecondarySubject1.MultipleChoiceQuestions.Count; i++)
            for (int j = 0; j < 6; j++)
                sub1Multiple[i, j] = test.SecondarySubject1.MultipleChoiceQuestions[i].Options[j].IsCorrect;

        var sub1CtxQ = test.SecondarySubject1.ContextQuestion;
        var sub1Context = new bool[sub1CtxQ.Questions.Count, 4];
        for (int j = 0; j < sub1CtxQ.Questions.Count; j++)
            for (int k = 0; k < 4; k++)
                sub1Context[j, k] = sub1CtxQ.Questions[j].Options[k].IsCorrect;

        // FIX: CorrectMatches → FlatToMatrix; Add() вместо индексирования пустого списка;
        //      размеры матрицы берутся из реальных данных, не захардкожены как [2,4]
        var sub1Match = test.SecondarySubject1.MatchQuestions
            .Select(q => FlatToMatrix(q))
            .ToList();

        // ── SecondarySubject2 ─────────────────────────────────────────────────────
        var sub2Single = new bool[test.SecondarySubject2.SingleChoiceQuestions.Count, 4];
        for (int i = 0; i < test.SecondarySubject2.SingleChoiceQuestions.Count; i++)
            for (int j = 0; j < 4; j++)
                sub2Single[i, j] = test.SecondarySubject2.SingleChoiceQuestions[i].Options[j].IsCorrect;

        var sub2Multiple = new bool[test.SecondarySubject2.MultipleChoiceQuestions.Count, 6];
        for (int i = 0; i < test.SecondarySubject2.MultipleChoiceQuestions.Count; i++)
            for (int j = 0; j < 6; j++)
                sub2Multiple[i, j] = test.SecondarySubject2.MultipleChoiceQuestions[i].Options[j].IsCorrect;

        var sub2CtxQ = test.SecondarySubject2.ContextQuestion;
        var sub2Context = new bool[sub2CtxQ.Questions.Count, 4];
        for (int j = 0; j < sub2CtxQ.Questions.Count; j++)
            for (int k = 0; k < 4; k++)
                sub2Context[j, k] = sub2CtxQ.Questions[j].Options[k].IsCorrect;

        var sub2Match = test.SecondarySubject2.MatchQuestions
            .Select(q => FlatToMatrix(q))
            .ToList();

        // ── Сборка ────────────────────────────────────────────────────────────────
        return new TestAnswers
        {
            KazakhHistoryAnswers = new KazakhHistoryTestAnswers
            {
                SingleChoiceAnswers = kzHisSingle,
                ContextAnswers = kzHisContext
            },
            FunctionalLiteracyAnswers = new FunctionalLiteracyTestAnswers
            {
                ContextAnswers = funcLitContext
            },
            MathematicalLiteracyAnswers = new MathematicalLiteracyTestAnswers
            {
                SingleChoiceAnswers = mathLitSingle
            },
            SecondarySubject1Answers = new SecondarySubjectTestAnswers
            {
                SingleChoiceAnswers = sub1Single,
                MultipleChoiceAnswers = sub1Multiple,
                ContextAnswers = sub1Context,
                MatchAnswers = sub1Match
            },
            SecondarySubject2Answers = new SecondarySubjectTestAnswers
            {
                SingleChoiceAnswers = sub2Single,
                MultipleChoiceAnswers = sub2Multiple,
                ContextAnswers = sub2Context,
                MatchAnswers = sub2Match
            }
        };
    }
    public async Task<TestResult> CalculateTestResultAsync(string studentId,
                                        TestAnswers studentAnswers,
                                        TestAnswers testAnswers,
                                        Subject SecondarySubject1,
                                        Subject SecondarySubject2)
    {
        int kazakhHistoryScore = 20;
        for (int i = 0; i < testAnswers.KazakhHistoryAnswers.SingleChoiceAnswers.GetLength(0); i++)
            for (int j = 0; j < testAnswers.KazakhHistoryAnswers.SingleChoiceAnswers.GetLength(1); j++)
                if (studentAnswers.KazakhHistoryAnswers.SingleChoiceAnswers[i, j] != testAnswers.KazakhHistoryAnswers.SingleChoiceAnswers[i, j])
                    kazakhHistoryScore--;

        for (int i = 0; i < testAnswers.KazakhHistoryAnswers.ContextAnswers.Count; i++)
            for (int j = 0; j < testAnswers.KazakhHistoryAnswers.ContextAnswers[i].GetLength(0); j++)
                for (int k = 0; k < testAnswers.KazakhHistoryAnswers.ContextAnswers[i].GetLength(1); k++)
                    if (studentAnswers.KazakhHistoryAnswers.ContextAnswers[i][j, k] != testAnswers.KazakhHistoryAnswers.ContextAnswers[i][j, k])
                        kazakhHistoryScore--;

        int functionalScore = 10;
        for (int i = 0; i < testAnswers.FunctionalLiteracyAnswers.ContextAnswers.Count; i++)
            for (int j = 0; j < testAnswers.FunctionalLiteracyAnswers.ContextAnswers[i].GetLength(0); j++)
                for (int k = 0; k < testAnswers.FunctionalLiteracyAnswers.ContextAnswers[i].GetLength(1); k++)
                    if (studentAnswers.FunctionalLiteracyAnswers.ContextAnswers[i][j, k] != testAnswers.FunctionalLiteracyAnswers.ContextAnswers[i][j, k])
                        functionalScore--;

        int mathLitScore = 10;
        for (int i = 0; i < testAnswers.MathematicalLiteracyAnswers.SingleChoiceAnswers.GetLength(0); i++)
            for (int j = 0; j < testAnswers.MathematicalLiteracyAnswers.SingleChoiceAnswers.GetLength(1); j++)
                if (studentAnswers.MathematicalLiteracyAnswers.SingleChoiceAnswers[i, j] != testAnswers.MathematicalLiteracyAnswers.SingleChoiceAnswers[i, j])
                    mathLitScore--;

        int sec1Score = 50;
        for (int i = 0; i < testAnswers.SecondarySubject1Answers.SingleChoiceAnswers.GetLength(0); i++)
            for (int j = 0; j < testAnswers.SecondarySubject1Answers.SingleChoiceAnswers.GetLength(1); j++)
                if (studentAnswers.SecondarySubject1Answers.SingleChoiceAnswers[i, j] != testAnswers.SecondarySubject1Answers.SingleChoiceAnswers[i, j])
                    sec1Score--;

        for (int i = 0; i < testAnswers.SecondarySubject1Answers.MultipleChoiceAnswers.GetLength(0); i++)
        {
            int mismatches = 0;
            int correctCount = 0;
            bool studentFoundCorrect = false;

            for (int j = 0; j < testAnswers.SecondarySubject1Answers.MultipleChoiceAnswers.GetLength(1); j++)
            {
                bool isCorrect = testAnswers.SecondarySubject1Answers.MultipleChoiceAnswers[i, j];
                bool studentSelected = studentAnswers.SecondarySubject1Answers.MultipleChoiceAnswers[i, j];

                if (isCorrect) correctCount++;
                if (isCorrect && studentSelected) studentFoundCorrect = true;
                if (isCorrect != studentSelected) mismatches++;
            }

            if (mismatches == 0) continue;

            if (correctCount == 1 && !studentFoundCorrect)
                sec1Score -= 2;
            else if (mismatches == 1)
                sec1Score -= 1;
            else
                sec1Score -= 2;
        }

        for (int j = 0; j < testAnswers.SecondarySubject1Answers.ContextAnswers.GetLength(0); j++)
            for (int k = 0; k < testAnswers.SecondarySubject1Answers.ContextAnswers.GetLength(1); k++)
                if (studentAnswers.SecondarySubject1Answers.ContextAnswers[j, k] != testAnswers.SecondarySubject1Answers.ContextAnswers[j, k])
                    sec1Score--;

        for (int i = 0; i < testAnswers.SecondarySubject1Answers.MatchAnswers.Count; i++)
            for (int j = 0; j < testAnswers.SecondarySubject1Answers.MatchAnswers[i].GetLength(0); j++)
                for (int k = 0; k < testAnswers.SecondarySubject1Answers.MatchAnswers[i].GetLength(1); k++)
                    if (studentAnswers.SecondarySubject1Answers.MatchAnswers[i][j, k] != testAnswers.SecondarySubject1Answers.MatchAnswers[i][j, k])
                        sec1Score--;

        int sec2Score = 50;
        for (int i = 0; i < testAnswers.SecondarySubject2Answers.SingleChoiceAnswers.GetLength(0); i++)
            for (int j = 0; j < testAnswers.SecondarySubject2Answers.SingleChoiceAnswers.GetLength(1); j++)
                if (studentAnswers.SecondarySubject2Answers.SingleChoiceAnswers[i, j] != testAnswers.SecondarySubject2Answers.SingleChoiceAnswers[i, j])
                    sec2Score--;

        for (int i = 0; i < testAnswers.SecondarySubject2Answers.MultipleChoiceAnswers.GetLength(0); i++)
        {
            int mismatches = 0;
            int correctCount = 0;
            bool studentFoundCorrect = false;

            for (int j = 0; j < testAnswers.SecondarySubject2Answers.MultipleChoiceAnswers.GetLength(1); j++)
            {
                bool isCorrect = testAnswers.SecondarySubject2Answers.MultipleChoiceAnswers[i, j];
                bool studentSelected = studentAnswers.SecondarySubject2Answers.MultipleChoiceAnswers[i, j];

                if (isCorrect) correctCount++;
                if (isCorrect && studentSelected) studentFoundCorrect = true;
                if (isCorrect != studentSelected) mismatches++;
            }

            if (mismatches == 0) continue;

            if (correctCount == 1 && !studentFoundCorrect)
                sec2Score -= 2;
            else if (mismatches == 1)
                sec2Score -= 1;
            else
                sec2Score -= 2;
        }

        for (int j = 0; j < testAnswers.SecondarySubject2Answers.ContextAnswers.GetLength(0); j++)
            for (int k = 0; k < testAnswers.SecondarySubject2Answers.ContextAnswers.GetLength(1); k++)
                if (studentAnswers.SecondarySubject2Answers.ContextAnswers[j, k] != testAnswers.SecondarySubject2Answers.ContextAnswers[j, k])
                    sec2Score--;

        for (int i = 0; i < testAnswers.SecondarySubject2Answers.MatchAnswers.Count; i++)
            for (int j = 0; j < testAnswers.SecondarySubject2Answers.MatchAnswers[i].GetLength(0); j++)
                for (int k = 0; k < testAnswers.SecondarySubject2Answers.MatchAnswers[i].GetLength(1); k++)
                    if (studentAnswers.SecondarySubject2Answers.MatchAnswers[i][j, k] != testAnswers.SecondarySubject2Answers.MatchAnswers[i][j, k])
                        sec2Score--;

        return new TestResult
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
    }
}