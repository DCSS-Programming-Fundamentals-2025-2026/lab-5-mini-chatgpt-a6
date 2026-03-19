using System.Text.Json;

public class NGramModel
{
    public float[][] _probs; 
    public int VocabSize;
    private NGramCounts counts;

    public NGramModel(int vocabSize) 
    {
        VocabSize = vocabSize;
        counts = new NGramCounts();
        _probs = new float[vocabSize][];

        for (int i = 0; i < vocabSize; i++)
        {
            _probs[i] = new float[vocabSize];
        }
    }

    public void Train(ReadOnlySpan<int> tokens)
    {
        if (tokens.Length < 2)
        {
            return;
        }

        counts.CountBigrams(_probs, tokens);

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
            return _probs[lastToken];
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

public class TrigramModel
{
    public Dictionary<(int, int), float[]> _trigramProbs = new Dictionary<(int, int), float[]>();
    public NGramModel bigramModel;
    public int VocabSize;
    private NGramCounts counts;

    public TrigramModel(int vocabSize)
    {
        VocabSize = vocabSize;
        counts = new NGramCounts();
        bigramModel = new NGramModel(VocabSize);

        for (int i = 0; i < vocabSize; i++)
        {
            for (int j = 0; j < vocabSize; j++)
            {
                _trigramProbs.Add((i, j), new float[vocabSize]);
            }
        }
    }

    public void Train(ReadOnlySpan<int> tokens)
    {
        bigramModel.Train(tokens);

        if (tokens.Length < 3)
        {
            return;
        }

        counts.CountTrigrams(_trigramProbs, tokens);

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
            return _trigramProbs[(beforeLastToken, lastToken)];
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

public class NGramPayloadMapper
{
    public JsonElement FromBigramToJson(NGramModel model)
    {
        Object obj = new
        {
            modelKind = "bigram",
            modelPayload = new
            {
                bigramProbs = model._probs
            }
        };

        string json = JsonSerializer.Serialize(obj);
        JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        return jsonElement;
    }

    public void FromJsonElementToBigram(JsonElement jsonElement, NGramModel model)
    {
        var payload = jsonElement.GetProperty("modelPayload");
        string probs = payload.GetProperty("bigramProbs").GetRawText();
        model._probs = JsonSerializer.Deserialize<float[][]>(probs);
        model.VocabSize = model._probs.Length;
    }

    public JsonElement FromTrigramToJson(TrigramModel model)
    {
        Dictionary<string, float[]> temp = new Dictionary<string, float[]>();
        foreach (var pair in model._trigramProbs)
        {
            string newKey = $"{pair.Key.Item1}, {pair.Key.Item2}";
            temp.Add(newKey, pair.Value);
        }
   
        Object obj = new
        {
            modelKind = "trigram",
            modelPayload = new
            {
                bigramProbs = model.bigramModel._probs,
                trigramProbs = temp
            }
        };

        string json = JsonSerializer.Serialize(obj);
        JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        return jsonElement;
    }

    public void FromJsonElementToTrigram(JsonElement jsonElement, TrigramModel model)
    {
        var payload = jsonElement.GetProperty("modelPayload");
        string bigramProbs = payload.GetProperty("bigramProbs").GetRawText();
        string trigramProbs = payload.GetProperty("trigramProbs").GetRawText();    
        
        model.bigramModel._probs = JsonSerializer.Deserialize<float[][]>(bigramProbs);
        model.bigramModel.VocabSize = model.bigramModel._probs.Length;

        model.VocabSize = model.bigramModel.VocabSize;
        Dictionary<string, float[]> temp = JsonSerializer.Deserialize<Dictionary<string, float[]>>(trigramProbs);
        Dictionary<(int, int), float[]> oldDictionary = new Dictionary<(int, int), float[]>();

        foreach(var pair in temp)
        {
            string[] keys = pair.Key.Split(',');
            oldDictionary[(int.Parse(keys[0].Trim()), int.Parse(keys[1].Trim()))] = pair.Value;
        }

        model._trigramProbs = oldDictionary;
    }
}

public class NGramCounts
{
    public void CountBigrams(float[][] probs, ReadOnlySpan<int> tokens)
    {
        for (int i = 1; i < tokens.Length; i++)
        {
            probs[tokens[i - 1]][tokens[i]]++;
        }
    }

    public void CountTrigrams(Dictionary<(int, int), float[]> probs, ReadOnlySpan<int> tokens)
    {
        for (int i = 2; i < tokens.Length; i++)
        {
            probs[(tokens[i - 2], tokens[i - 1])][tokens[i]]++;
        }
    }

    public int GetBigramPrevTotal(float[][] probs, int vocabSize, int prev)
    {
        int sum = 0;

        for (int i = 0; i < vocabSize; i++)
        {
            sum += (int)probs[prev][i];
        }

        return sum;
    }

    public int GetTrigramPrevsTotal(Dictionary<(int, int), float[]> probs, (int, int) pair)
    {
        int sum = 0;

        for (int j = 0; j < probs[pair].Length; j++)
        {
            sum += (int)probs[pair][j];
        }

        return sum;
    }
}

public class PerplexityCalculator
{
    public float ComputePerplexityBigram(NGramModel model, ReadOnlySpan<int> tokens)
    {
        if (tokens.Length < 2)
        {
            return float.PositiveInfinity;
        }

        double logSum = 0;
        int count = 0;

        for (int i = 1; i < tokens.Length; i++)
        {
            int[] context = { tokens[i - 1] };
            float[] probs = model.NextTokenScores(context);

            float prob = probs[tokens[i]];

            if (prob <= 0)
            {
                prob = 0.0000000001f;
            }

            logSum += Math.Log(prob);
            count++;
        }

        double average = logSum / count;
        return (float)Math.Exp(-average);
    }


    public float ComputePerplexityTrigram(TrigramModel model, ReadOnlySpan<int> tokens)
    {
        if (tokens.Length < 2)
        {
            return float.PositiveInfinity;
        }

        double logSum = 0;
        int count = 0;

        for (int i = 1; i < tokens.Length; i++)
        {
            float[] probs;
            
            if (i >= 2)
            {
                int[] context = { tokens[i - 2], tokens[i - 1] };
                probs = model.NextTokenScores(context);
            }
            else
            {
                int[] context = { tokens[i - 1] };
                probs = model.bigramModel.NextTokenScores(context);
            }                

            float prob = probs[tokens[i]];

            if (prob <= 0)
            {
                prob = 0.0000000001f;
            }

            logSum += Math.Log(prob);
            count++;
        }

        double average = logSum / count;
        return (float)Math.Exp(-average);
    }
}

