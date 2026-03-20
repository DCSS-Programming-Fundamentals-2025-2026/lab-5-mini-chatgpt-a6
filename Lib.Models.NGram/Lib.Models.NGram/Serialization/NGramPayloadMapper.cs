using System.Text.Json;

public class NGramPayloadMapper
{
    public JsonElement FromBigramToJson(NGramModel model)
    {
        Object obj = new
        {
            modelKind = model.ModelKind,
            modelPayload = new
            {
                bigramProbs = model._probs
            },
            contractFingerprintChain = $"|Lib.Models.NGram: {model.GetContractFingerprint()}|"
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
            modelKind = model.ModelKind,
            modelPayload = new
            {
                bigramProbs = model.bigramModel._probs,
                trigramProbs = temp
            },
            contractFingerprintChain = $"|Lib.Models.NGram: {model.GetContractFingerprint()}|"
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

        foreach (var pair in temp)
        {
            string[] keys = pair.Key.Split(',');
            oldDictionary[(int.Parse(keys[0].Trim()), int.Parse(keys[1].Trim()))] = pair.Value;
        }

        model._trigramProbs = oldDictionary;
    }
}