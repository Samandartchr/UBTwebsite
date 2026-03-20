using API.Domain.Enums.Subject;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Cloud.Firestore.V1;
using Google.Cloud.Firestore;

namespace API.Domain.Entities.QuestionTypes;

[FirestoreData]
public class Block
{
    [FirestoreProperty]
    public string? ImageLink { get; set; }
    [FirestoreProperty]
    public string? Text { get; set; }
}

public class AlwaysFalseConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetBoolean(); // deserialize normally

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteBooleanValue(false); // always send false
}

[FirestoreData]
public class Option: Block
{
    [FirestoreProperty]
    [JsonConverter(typeof(AlwaysFalseConverter))]
    public bool IsCorrect { get; set; } = false;
}
[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(SingleChoiceQuestion), "SingleChoise")]
//[JsonDerivedType(typeof(MultipleChoiceQuestion), "multiple")]
//[JsonDerivedType(typeof(MatchQuestion), "match")]
//[JsonDerivedType(typeof(ContextQuestion), "match")]
[FirestoreData]
public class Question: Block
{
    [FirestoreProperty]
    public required string Id { get; set; }
    [FirestoreProperty(ConverterType = typeof(SubjectConverter))]
    public required Subject Subject { get; set; }
}

[FirestoreData]
public class SingleChoiceQuestion: Question
{
    [FirestoreProperty]
    public string Type { get; set; } = "SingleChoice";
    [FirestoreProperty]
    public required List<Option> Options { get; set; }
}
[FirestoreData]
public class MultipleChoiceQuestion: Question
{
    [FirestoreProperty]
    public string Type { get; set; } = "MultipleChoice";
    [FirestoreProperty]
    public required List<Option> Options { get; set; }
}
[FirestoreData]
public class ContextQuestion
{
    [FirestoreProperty]
    public string Type { get; set; } = "Context";
    [FirestoreProperty]
    public required string Id { get; set; }
    [FirestoreProperty(ConverterType = typeof(SubjectConverter))]
    public required Subject Subject { get; set; }
    [FirestoreProperty]
    public string? ContextImageLink { get; set; }
    [FirestoreProperty]
    public string? ContextText { get; set; }
    [FirestoreProperty]
    public required List<SingleChoiceQuestion> Questions { get; set; }
}
[FirestoreData]
public class MatchQuestion: Question
{
    [FirestoreProperty]
    public string Type { get; set; } = "Match";
    [FirestoreProperty]
    public required List<Block> LeftSide { get; set; }
    [FirestoreProperty]
    public required List<Block> RightSide { get; set; }
    //public required bool[,] CorrectMatches { get; set; } = new bool[2,4];
    [FirestoreProperty]
    [JsonConverter(typeof(AlwaysFalseListConverter))]
    public List<bool> CorrectMatches { get; set; }
    public bool[][] GetCorrectMatches()
    {
        int L = LeftSide.Count;
        int R = RightSide.Count;
        var result = new bool[L][];
        for (int r = 0; r < L; r++)
        {
            result[r] = new bool[R];
            for (int c = 0; c < R; c++)
                result[r][c] = CorrectMatches[r * R + c];
        }
        return result;
    }
}

public class AlwaysFalseListConverter : JsonConverter<List<bool>>
{
    public override List<bool> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<bool>();
        reader.Read(); // [
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            list.Add(reader.GetBoolean()); // deserialize normally
            reader.Read();
        }
        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<bool> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var _ in value)
            writer.WriteBooleanValue(false); // always write false
        writer.WriteEndArray();
    }
}