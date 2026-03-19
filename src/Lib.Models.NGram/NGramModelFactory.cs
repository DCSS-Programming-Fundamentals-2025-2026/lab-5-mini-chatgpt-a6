using Contracts;

namespace Lib.Models.NGram;

public sealed class NGramModelFactory : INGramModelFactory
{
    public ILanguageModel Create(string modelType, int vocabSize)
    {
        return modelType switch
        {
            "ngram"   => new NGramModel(vocabSize),
            "trigram" => new TrigramModel(vocabSize),
            _ => throw new ArgumentException($"Unknown model type: {modelType}")
        };
    }
}