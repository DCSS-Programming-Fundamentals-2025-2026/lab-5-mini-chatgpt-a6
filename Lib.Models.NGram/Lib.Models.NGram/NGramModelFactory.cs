public class NGramModelFactory : INGramModelFactory
{
    public ILanguageModel Create(string modelType, int vocabSize)
    {
        if (modelType == "bigram")
        {
            return new NGramModel(vocabSize);
        }

        if (modelType == "trigram")
        {
            return new TrigramModel(vocabSize);
        }

        throw new ArgumentException($"Unknown model type: {modelType}");
    }
}