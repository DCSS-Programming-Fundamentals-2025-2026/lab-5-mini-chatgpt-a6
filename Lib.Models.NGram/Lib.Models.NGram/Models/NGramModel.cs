using System.Text.Json;

public class NGramModel : ILanguageModel
{
    public float[][] _probs; 
    public int VocabSize { get; set; }
    private NGramCounts counts = new NGramCounts();
    public string ModelKind { get; }

    public NGramModel(int vocabSize) 
    {
        VocabSize = vocabSize;
        _probs = new float[vocabSize][];
        ModelKind = "bigram";

        for (int i = 0; i < vocabSize; i++)
        {
            _probs[i] = new float[vocabSize];
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

        if (obj is not NGramModel model)
        {
            return false;
        }

        if (model.ModelKind != this.ModelKind || model.VocabSize != this.VocabSize)
        {
            return false;
        }

        for (int i = 0; i < VocabSize; i++)
        {
            for (int j = 0; j < VocabSize; j++)
            {
                if (model._probs[i][j] != this._probs[i][j])
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

        _probs = new float[VocabSize][];
        for (int i = 0; i < VocabSize; i++)
        {
            _probs[i] = new float[VocabSize];
        }
    }

    public void Train(ReadOnlySpan<int> tokens)
    {
        if (tokens.Length < 2)
        {
            return;
        }

        ResetTrainingResults();

        try
        {
            counts.CountBigrams(_probs, tokens);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw;
        }

        for (int i = 0; i < VocabSize; i++)
        {
            int countForRow = counts.GetBigramPrevTotal(_probs, VocabSize, i);

            if (countForRow != 0)
            {
                for (int j = 0; j < VocabSize; j++)
                {
                    _probs[i][j] = _probs[i][j] / countForRow;
                }
            }
        }
    }

    public float[] NextTokenScores(ReadOnlySpan<int> context)
    {
        float[] alternative = new float[VocabSize];
        for (int i = 0; i < VocabSize; i++)
        {
            alternative[i] = (float)1 / VocabSize;
        }

        if (context.Length < 1)
        {
            return alternative;
        }

        int lastToken = context[context.Length - 1];       

        if (lastToken < 0 || lastToken >= VocabSize)
        {
            return alternative;
        }

        bool isNull = true;

        for (int i = 0; i < VocabSize; i++)
        {
            if (_probs[lastToken][i] != 0)
            {
                isNull = false;
                break;
            }
        }

        if (!isNull)
        {
            return (float[])_probs[lastToken].Clone();
        }

        return alternative;
    }

    public void FromPayload(JsonElement json)
    {
        NGramPayloadMapper mapper = new NGramPayloadMapper();
        mapper.FromJsonElementToBigram(json, this);
    }

    public JsonElement GetPayloadForCheckpoint()
    {
        NGramPayloadMapper mapper = new NGramPayloadMapper();      
        return mapper.FromBigramToJson(this);
    }
}