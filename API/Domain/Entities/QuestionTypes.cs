using API.Domain.Enums.Subject;

namespace API.Domain.Entities.QuestionTypes;

public class Block
{
    public string? ImageLink { get; set; }
    public string? Text { get; set; }
}

public class Option: Block
{
    public required bool IsCorrect { get; set; } = false;
}
public class Question: Block
{
    public required string Id { get; set; }
    public required Subject Subject { get; set; }
}

public class SingleChoiceQuestion: Question
{
    public string Type { get; set; } = "SingleChoice";
    public required List<Option> Options { get; set; }
}
public class MultipleChoiceQuestion: Question
{
    public string Type { get; set; } = "MultipleChoice";
    public required List<Option> Options { get; set; }
}
public class ContextQuestion
{
    public string Type { get; set; } = "Context";
    public required string Id { get; set; }
    public required Subject Subject { get; set; }
    public string? ContextImageLink { get; set; }
    public string? ContextText { get; set; }
    public required List<SingleChoiceQuestion> Questions { get; set; }
}
public class MatchQuestion: Question
{
    public string Type { get; set; } = "Match";
    public required List<Block> LeftSide { get; set; }
    public required List<Block> RightSide { get; set; }
    public required bool[,] CorrectMatches { get; set; } = new bool[2,4];
}