namespace Contracts;

public interface ILanguageModel
{
    string ModelKind { get; }

    int VocabSize { get; }

    float[] NextTokenScores(ReadOnlySpan<int> context);

    object GetPayloadForCheckpoint();
}