using Contracts;

namespace Lib.Models.NGram;

public interface INGramModelFactory
{
    ILanguageModel Create(string modelType, int vocabSize);
}