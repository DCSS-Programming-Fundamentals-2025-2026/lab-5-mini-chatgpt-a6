# Lib.Models.NGram

**Група A6** — Рекун Катерина, Глушкова Марина
Етап 1 · Mini-ChatGPT · Основи програмування

---

## Що це

Бібліотека реалізує N-грамні мовні моделі для проекту Mini-ChatGPT. Містить Bigram і Trigram моделі, підрахунок статистики, серіалізацію checkpoint та фабрику моделей.

Залежить виключно від `Contracts`. Не знає про токенізатор, корпус чи семплер.

---

## Структура

```
Lib.Models.NGram.sln
│
├── Lib.Models.NGram
│   ├── Interfaces
│   │   ├── ILanguageModel.cs
│   │   └── INGramModelFactory.cs
│   │
│   ├── Metrics
│   │   └── PerplexityCalculator.cs
│   │
│   ├── Models
│   │   ├── NGramModel.cs
│   │   └── TrigramModel.cs
│   │
│   ├── Serialization
│   │   └── NGramPayloadMapper.cs
│   │
│   ├── Statistics
│   │   └── NGramCounts.cs
│   │
│   ├── NGramModelFactory.cs
│   └── Lib.Models.NGram.csproj
│
├── Lib.Models.NGram.Tests
│   ├── Metrics
│   │   └── PerplexityCalculator.Tests.cs
│   │
│   ├── Models
│   │   ├── NGramModel.Tests.cs
│   │   └── TrigramModel.Tests.cs
│   │
│   ├── Statistics
│   │   └── NGramCounts.Tests.cs
│   │
│   ├── NGramModelFactory.Tests.cs
│   └── Lib.Models.NGram.Tests.csproj
│
├── .gitignore
└── README.md

```

---

Інтерфейси
ILanguageModel
Контракт для всіх мовних моделей проекту.
ЧленПідписОписModelKindstring { get; }Ідентифікатор типу: "bigram", "trigram"VocabSizeint { get; }Розмір словникаTrain(ReadOnlySpan<int> tokens) → voidНавчання на масиві токенівNextTokenScores(ReadOnlySpan<int> context) → float[]Ймовірності для наступного токенаFromPayload(JsonElement json) → voidВідновлення стану з checkpointGetPayloadForCheckpoint() → JsonElementСеріалізація стану для збереження
INgramModelFactory
МетодПідписОписCreate(string modelType, int vocabSize) → ILanguageModel"bigram" → NGramModel, "trigram" → TrigramModel

Класи
NGramCounts
Utility клас для підрахунку статистики під час навчання. Отримує матриці даних як параметри — не зберігає стан.
МетодПідписОписCountBigrams(float[][] probs, ReadOnlySpan<int> tokens) → voidПідраховує пари токенів у матрицюCountTrigrams(Dictionary<(int,int),float[]> probs, ReadOnlySpan<int> tokens) → voidПідраховує трійки токенівGetBigramPrevTotal(float[][] probs, int vocabSize, int prev) → intСума рядка для нормалізації bigramGetTrigramPrevsTotal(Dictionary<(int,int),float[]> probs, (int,int) pair) → intСума лічильників для пари trigram
Кидає ArgumentOutOfRangeException якщо токен виходить за межі [0, VocabSize).

NGramModel — реалізує ILanguageModel
Bigram модель. ModelKind = "bigram".
МетодПідписОписTrain(ReadOnlySpan<int> tokens) → voidНавчання: підрахунок пар через NGramCounts, нормалізаціяNextTokenScores(ReadOnlySpan<int> context) → float[]Повертає рядок матриці для останнього токена контекстуGetPayloadForCheckpoint() → JsonElementСеріалізація через NGramPayloadMapperFromPayload(JsonElement json) → voidВідновлення через NGramPayloadMapperEquals(object? obj) → boolПорівняння за ModelKind, VocabSize і матрицею ймовірностей
Поведінка NextTokenScores при крайніх випадках — повертає рівномірний розподіл 1/VocabSize:

порожній контекст
токен поза межами [0, VocabSize)
токен не зустрічався у тренуванні (весь рядок нульовий)


TrigramModel — реалізує ILanguageModel
Trigram модель з fallback на Bigram. ModelKind = "trigram". Містить NGramModel bigramModel як внутрішнє поле.
МетодПідписОписTrain(ReadOnlySpan<int> tokens) → voidСпочатку тренує bigram, потім trigramNextTokenScores(ReadOnlySpan<int> context) → float[]Trigram якщо пара відома і ненульова, інакше fallback на bigramGetPayloadForCheckpoint() → JsonElementСеріалізація через NGramPayloadMapperFromPayload(JsonElement json) → voidВідновлення через NGramPayloadMapperEquals(object? obj) → boolПорівняння включаючи bigramModel.Equals
Конструктор ініціалізує _trigramProbs для всіх VocabSize² пар одразу.

NGramPayloadMapper
Серіалізація і десеріалізація стану моделей. Instance клас.
МетодПідписОписFromBigramToJson(NGramModel model) → JsonElementСеріалізує _probs у JSONFromJsonElementToBigram(JsonElement json, NGramModel model) → voidВідновлює _probs і VocabSizeFromTrigramToJson(TrigramModel model) → JsonElementСеріалізує bigram + trigramFromJsonElementToTrigram(JsonElement json, TrigramModel model) → voidВідновлює bigram і trigram
Ключі trigram словника серіалізуються як рядки "p2, p1" і розбираються назад при десеріалізації.

PerplexityCalculator
Обчислює Perplexity на валідаційному тексті. Менше значення — краща модель.
МетодПідписОписComputePerplexityBigram(NGramModel model, ReadOnlySpan<int> tokens) → floatPerplexity для bigramComputePerplexityTrigram(TrigramModel model, ReadOnlySpan<int> tokens) → floatPerplexity для trigram
Повертає float.PositiveInfinity якщо tokens.Length < 2. При нульовій ймовірності підставляє 1e-10 замість 0 щоб уникнути log(0).

NGramModelFactory — реалізує INgramModelFactory
МетодПідписОписCreate(string modelType, int vocabSize) → ILanguageModel"bigram" → NGramModel, "trigram" → TrigramModel, інше → ArgumentException

Тести
КласК-стьЩо покриваютьNGramModelTests4Train нормальний, один токен, невалідний; NextTokenScores нормальний, порожній, поза межамиTrigramModelTests6Train нормальний, два токени, невалідний; NextTokenScores нормальний, нулі, новий токен, bigram fallbackNGramCountsTests7CountBigrams, CountTrigrams — нормальний, невідомий токен, від'ємний; GetTotalsNGramModelFactoryTests3Create bigram, trigram, невідомий тип

Очікувана інтеграція для Етапу 2
Data Pipeline (B1) та Training Data (B2)
csharpvar factory = new NGramModelFactory();
var model = factory.Create("bigram", tokenizer.VocabSize);
model.Train(tokenizer.Encode(corpus.TrainText));
Runtime (B3)
csharpILanguageModel model = checkpoint.ModelKind switch
{
    "bigram"  => CreateAndLoad<NGramModel>(checkpoint),
    "trigram" => CreateAndLoad<TrigramModel>(checkpoint),
    _         => throw new ArgumentException("Unknown model kind")
};
// де CreateAndLoad викликає model.FromPayload(checkpoint.ModelPayload)
Baseline (B4)
csharpvar calc = new PerplexityCalculator();
float perplexity = calc.ComputePerplexityBigram(
    (NGramModel)model,
    tokenizer.Encode(corpus.ValText)
);
Neural (B5)
NextTokenScores має однаковий підпис у всіх моделей через ILanguageModel — семплер може працювати з будь-якою моделлю без змін.

Збірка і тести
bashdotnet build Lib.Models.NGram/Lib.Models.NGram.csproj
dotnet test