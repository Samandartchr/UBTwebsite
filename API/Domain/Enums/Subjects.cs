using Google.Cloud.Firestore.V1;
using Google.Cloud.Firestore;

namespace API.Domain.Enums.Subject;

public enum Subject
{
    NoSubject,
    MathematicalLiteracy,
    FunctionalLiteracy,
    KazakhstanHistory,
    Physics,
    Mathematics,
    Informatics,
    Chemistry,
    Biology,
    Geography,
    WorldHistory,
    Laws,
    English,
    Russian,
    RussianLiterature,
    Kazakh,
    KazakhLiterature,
}

public class SubjectConverter : IFirestoreConverter<Subject>
{
    public object ToFirestore(Subject value) => value.ToString();

    public Subject FromFirestore(object value)
    {
        if (value is string s && Enum.TryParse<Subject>(s, true, out var result))
            return result;
        return Subject.NoSubject; // fallback instead of throwing
    }
}