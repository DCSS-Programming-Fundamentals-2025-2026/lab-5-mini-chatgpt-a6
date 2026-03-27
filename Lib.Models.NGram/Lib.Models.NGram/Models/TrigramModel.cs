using System.Text.Json;

public class TrigramModel : ILanguageModel
{
    public Dictionary<(int, int), float[]> _trigramProbs = new Dictionary<(int, int), float[]>();
    public NGramModel bigramModel;
    private NGramCounts counts = new NGramCounts();
    public int VocabSize { get; set; }
    public string ModelKind { get; }

    public TrigramModel(int vocabSize)
    {
        VocabSize = vocabSize;
        counts = new NGramCounts();
        bigramModel = new NGramModel(VocabSize);
        ModelKind = "trigram";

        for (int i = 0; i < vocabSize; i++)
        {
            for (int j = 0; j < vocabSize; j++)
            {
                _trigramProbs.Add((i, j), new float[vocabSize]);
            }
        }
    }

    public string GetContractFingerprint()
    {
        return $"V1_{ModelKind}:vocabSize={VocabSize}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is not TrigramModel model)
        {
            return false;
        }

        if (model.ModelKind != this.ModelKind 
            || model.VocabSize != this.VocabSize 
            || !model.bigramModel.Equals(this.bigramModel))
        {
            return false;
        }

        foreach (var pair in _trigramProbs)
        {
            for (int i = 0; i < VocabSize; i++)
            {
                if (model._trigramProbs[pair.Key][i] != this._trigramProbs[pair.Key][i])
                {
                    return false;
                }
            }
        }

        return true;
    }
    public void ResetTrainingResults()
    {
        counts = new NGramCounts();

        if (bigramModel != null)
        {
            bigramModel.ResetTrainingResults();
        }
        else
        {
            bigramModel = new NGramModel(VocabSize);
        }

        _trigramProbs.Clear();
        for (int i = 0; i < VocabSize; i++)
        {
            for (int j = 0; j < VocabSize; j++)
            {
                _trigramProbs.Add((i, j), new float[VocabSize]);
            }
        }
    }

    public void Train(ReadOnlySpan<int> tokens)
    {
        ResetTrainingResults();

        try
        {
            bigramModel.Train(tokens);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw;
        }

        if (tokens.Length < 3)
        {
            return;
        }

        try
        {
            counts.CountTrigrams(_trigramProbs, tokens);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw;
        }
        

        foreach (var bigram in _trigramProbs)
        {
            int countForPair = counts.GetTrigramPrevsTotal(_trigramProbs, bigram.Key);

            for (int j = 0; j < bigram.Value.Length; j++)
            {
                if (countForPair != 0)
                {
                    bigram.Value[j] = bigram.Value[j] / countForPair;
                }
            }
        }
    }

    public float[] NextTokenScores(ReadOnlySpan<int> context)
    {
        if (context.Length < 2)
        {
            return bigramModel.NextTokenScores(context);
        }

        int lastToken = context[context.Length - 1];
        int beforeLastToken = context[context.Length - 2];

        if (lastToken < 0 || lastToken >= VocabSize
            || beforeLastToken < 0 || beforeLastToken >= VocabSize)
        {
            return bigramModel.NextTokenScores(context);
        }

        bool isNull = true;

        for (int i = 0; i < VocabSize; i++)
        {
            if (_trigramProbs[(beforeLastToken, lastToken)][i] != 0)
            {
                isNull = false;
                break;
            }
        }

        if (!isNull)
        {
            return (float[])_trigramProbs[(beforeLastToken, lastToken)].Clone();
        }

        return bigramModel.NextTokenScores(context);
    }

    public void FromPayload(JsonElement json)
    {
        NGramPayloadMapper mapper = new NGramPayloadMapper();
        mapper.FromJsonElementToTrigram(json, this);
    }

    public JsonElement GetPayloadForCheckpoint()
    {
        NGramPayloadMapper mapper = new NGramPayloadMapper();
        return mapper.FromTrigramToJson(this);
    }
}