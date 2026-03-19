using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lib.Models.NGram.Serialization;

public static class NGramPayloadMapper
{
    public static object SerializeBigram(float[,] probs, int vocabSize)
    {
        var rows = new float[vocabSize][];

        for (int i = 0; i < vocabSize; i++)
        {
            rows[i] = new float[vocabSize];

            for (int j = 0; j < vocabSize; j++)
            {
                rows[i][j] = probs[i, j];
            }
        }

        return new BigramPayload { VocabSize = vocabSize, Probs = rows };
    }

    public static float[,] DeserializeBigram(JsonElement payload)
    {
        int vocabSize = payload.GetProperty("vocabSize").GetInt32();
        var probsJson = payload.GetProperty("probs");
        var probs = new float[vocabSize, vocabSize];

        int i = 0;

        foreach (var row in probsJson.EnumerateArray())
        {
            int j = 0;

            foreach (var cell in row.EnumerateArray())
            {
                probs[i, j] = cell.GetSingle();
                j++;
            }

            i++;
        }

        return probs;
    }

    public static object SerializeTrigram(
        float[,] bigramProbs,
        int vocabSize,
        Dictionary<(int P2, int P1), float[]> trigramProbs)
    {
        var bigramRows = new float[vocabSize][];

        for (int i = 0; i < vocabSize; i++)
        {
            bigramRows[i] = new float[vocabSize];

            for (int j = 0; j < vocabSize; j++)
            {
                bigramRows[i][j] = bigramProbs[i, j];
            }
        }

        var entries = trigramProbs
            .Select(kv => new TrigramEntry
            {
                P2 = kv.Key.P2,
                P1 = kv.Key.P1,
                Probs = kv.Value
            })
            .ToList();

        return new TrigramPayload
        {
            VocabSize = vocabSize,
            BigramProbs = bigramRows,
            TrigramProbs = entries
        };
    }

    public static (float[,] BigramProbs, Dictionary<(int, int), float[]> TrigramProbs)
        DeserializeTrigram(JsonElement payload)
    {
        int vocabSize = payload.GetProperty("vocabSize").GetInt32();

        var bigramJson = payload.GetProperty("bigramProbs");
        var bigramProbs = new float[vocabSize, vocabSize];

        int i = 0;

        foreach (var row in bigramJson.EnumerateArray())
        {
            int j = 0;

            foreach (var cell in row.EnumerateArray())
            {
                bigramProbs[i, j] = cell.GetSingle();
                j++;
            }

            i++;
        }

        var trigramJson = payload.GetProperty("trigramProbs");
        var trigramProbs = new Dictionary<(int, int), float[]>();

        foreach (var entry in trigramJson.EnumerateArray())
        {
            int p2 = entry.GetProperty("p2").GetInt32();
            int p1 = entry.GetProperty("p1").GetInt32();
            float[] probs = entry.GetProperty("probs")
                .EnumerateArray()
                .Select(v => v.GetSingle())
                .ToArray();
            trigramProbs[(p2, p1)] = probs;
        }

        return (bigramProbs, trigramProbs);
    }

    internal sealed class BigramPayload
    {
        [JsonPropertyName("vocabSize")]
        public int VocabSize { get; set; }

        [JsonPropertyName("probs")]
        public float[][] Probs { get; set; } = Array.Empty<float[]>();
    }

    internal sealed class TrigramPayload
    {
        [JsonPropertyName("vocabSize")]
        public int VocabSize { get; set; }

        [JsonPropertyName("bigramProbs")]
        public float[][] BigramProbs { get; set; } = Array.Empty<float[]>();

        [JsonPropertyName("trigramProbs")]
        public List<TrigramEntry> TrigramProbs { get; set; } = new();
    }

    internal sealed class TrigramEntry
    {
        [JsonPropertyName("p2")]
        public int P2 { get; set; }

        [JsonPropertyName("p1")]
        public int P1 { get; set; }

        [JsonPropertyName("probs")]
        public float[] Probs { get; set; } = Array.Empty<float>();
    }
}