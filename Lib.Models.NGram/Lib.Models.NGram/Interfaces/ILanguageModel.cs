using System.Text.Json;

public interface ILanguageModel
{
    string ModelKind { get; }
    int VocabSize { get; }
    void Train(ReadOnlySpan<int> tokens);
    float[] NextTokenScores(ReadOnlySpan<int> context);
    void FromPayload(JsonElement json);
    JsonElement GetPayloadForCheckpoint();
}