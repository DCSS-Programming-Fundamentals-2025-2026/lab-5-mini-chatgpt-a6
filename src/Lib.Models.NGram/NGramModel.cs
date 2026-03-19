using System.Text.Json;
using Contracts;
using Lib.Models.NGram.Statistics;
using Lib.Models.NGram.Serialization;

namespace Lib.Models.NGram;

public sealed class NGramModel : ILanguageModel
{
    private readonly float[,] _probs;

    public string ModelKind => "ngram";
    public int VocabSize { get; }

    public NGramModel(int vocabSize)
    {
        if (vocabSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vocabSize), "VocabSize must be positive.");
        }

        VocabSize = vocabSize;
        _probs = new float[vocabSize, vocabSize];
    }

    private NGramModel(float[,] probs)
    {
        _probs = probs;
        VocabSize = probs.GetLength(0);
    }

    public void Train(ReadOnlySpan<int> tokens)
    {
        if (tokens.Length < 2)
        {
            return;
        }

        var counts = new NGramCounts(VocabSize);

        for (int i = 0; i < tokens.Length - 1; i++)
        {
            counts.RecordBigram(tokens[i], tokens[i + 1]);
        }

        for (int prev = 0; prev < VocabSize; prev++)
        {
            int total = counts.GetBigramPrevTotal(prev);

            if (total == 0)
            {
                continue;
            }

            for (int next = 0; next < VocabSize; next++)
            {
                _probs[prev, next] = (float)counts.GetBigramCount(prev, next) / total;
            }
        }
    }

    public float[] NextTokenScores(ReadOnlySpan<int> context)
    {
        if (context.Length == 0)
        {
            return Uniform();
        }

        int last = context[^1];

        if (last < 0 || last >= VocabSize)
        {
            return Uniform();
        }

        float rowSum = 0f;

        for (int i = 0; i < VocabSize; i++)
        {
            rowSum += _probs[last, i];
        }

        if (rowSum == 0f)
        {
            return Uniform();
        }

        var result = new float[VocabSize];

        for (int i = 0; i < VocabSize; i++)
        {
            result[i] = _probs[last, i];
        }

        return result;
    }

    public object GetPayloadForCheckpoint()
    {
        return NGramPayloadMapper.SerializeBigram(_probs, VocabSize);
    }

    public static NGramModel FromPayload(JsonElement payload)
    {
        float[,] probs = NGramPayloadMapper.DeserializeBigram(payload);
        return new NGramModel(probs);
    }

    public static string GetContractFingerprint()
    {
        return "Lib.Models.NGram:v1.0.0:bigram";
    }

    internal float[,] Probs => _probs;

    private float[] Uniform()
    {
        var result = new float[VocabSize];
        float val = 1.0f / VocabSize;

        for (int i = 0; i < VocabSize; i++)
        {
            result[i] = val;
        }

        return result;
    }
}