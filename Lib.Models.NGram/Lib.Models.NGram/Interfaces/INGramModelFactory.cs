public interface INgramModelFactory
{
    ILanguageModel Create(string modelType, int vocabSize);
}