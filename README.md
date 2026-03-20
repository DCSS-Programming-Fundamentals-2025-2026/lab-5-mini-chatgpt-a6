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

## Інтерфейси

### `ILanguageModel`

Контракт для всіх мовних моделей проекту.

| Член | Підпис | Опис |
|---|---|---|
| `ModelKind` | `string { get; }` | Ідентифікатор типу: `"bigram"`, `"trigram"` |
| `VocabSize` | `int { get; }` | Розмір словника |
| `Train` | `(ReadOnlySpan<int> tokens) → void` | Навчання на масиві токенів |
| `NextTokenScores` | `(ReadOnlySpan<int> context) → float[]` | Ймовірності для наступного токена |
| `FromPayload` | `(JsonElement json) → void` | Відновлення стану з checkpoint |
| `GetPayloadForCheckpoint` | `() → JsonElement` | Серіалізація стану для збереження |

### `INgramModelFactory`

| Метод | Підпис | Опис |
|---|---|---|
| `Create` | `(string modelType, int vocabSize) → ILanguageModel` | `"bigram"` → `NGramModel`, `"trigram"` → `TrigramModel` |

---

## Класи

### `NGramCounts`

Utility клас для підрахунку статистики під час навчання. Отримує матриці даних як параметри — не зберігає стан.

| Метод | Підпис | Опис |
|---|---|---|
| `CountBigrams` | `(float[][] probs, ReadOnlySpan<int> tokens) → void` | Підраховує пари токенів у матрицю |
| `CountTrigrams` | `(Dictionary<(int,int),float[]> probs, ReadOnlySpan<int> tokens) → void` | Підраховує трійки токенів |
| `GetBigramPrevTotal` | `(float[][] probs, int vocabSize, int prev) → int` | Сума рядка для нормалізації bigram |
| `GetTrigramPrevsTotal` | `(Dictionary<(int,int),float[]> probs, (int,int) pair) → int` | Сума лічильників для пари trigram |

Кидає `ArgumentOutOfRangeException` якщо токен виходить за межі `[0, VocabSize)`.

---

### `NGramModel` — реалізує `ILanguageModel`

Bigram модель. `ModelKind` = `"bigram"`.

| Метод | Підпис | Опис |
|---|---|---|
| `Train` | `(ReadOnlySpan<int> tokens) → void` | Навчання: підрахунок пар через `NGramCounts`, нормалізація |
| `NextTokenScores` | `(ReadOnlySpan<int> context) → float[]` | Повертає рядок матриці для останнього токена контексту |
| `GetPayloadForCheckpoint` | `() → JsonElement` | Серіалізація через `NGramPayloadMapper` |
| `FromPayload` | `(JsonElement json) → void` | Відновлення через `NGramPayloadMapper` |
| `Equals` | `(object? obj) → bool` | Порівняння за `ModelKind`, `VocabSize` і матрицею ймовірностей |

**Поведінка `NextTokenScores` при крайніх випадках** — повертає рівномірний розподіл `1/VocabSize`:
- порожній контекст
- токен поза межами `[0, VocabSize)`
- токен не зустрічався у тренуванні (весь рядок нульовий)

---

### `TrigramModel` — реалізує `ILanguageModel`

Trigram модель з fallback на Bigram. `ModelKind` = `"trigram"`. Містить `NGramModel bigramModel` як внутрішнє поле.

| Метод | Підпис | Опис |
|---|---|---|
| `Train` | `(ReadOnlySpan<int> tokens) → void` | Спочатку тренує bigram, потім trigram |
| `NextTokenScores` | `(ReadOnlySpan<int> context) → float[]` | Trigram якщо пара відома і ненульова, інакше fallback на bigram |
| `GetPayloadForCheckpoint` | `() → JsonElement` | Серіалізація через `NGramPayloadMapper` |
| `FromPayload` | `(JsonElement json) → void` | Відновлення через `NGramPayloadMapper` |
| `Equals` | `(object? obj) → bool` | Порівняння включаючи `bigramModel.Equals` |

Конструктор ініціалізує `_trigramProbs` для всіх `VocabSize²` пар одразу.

---

### `NGramPayloadMapper`

Серіалізація і десеріалізація стану моделей. Instance клас.

| Метод | Підпис | Опис |
|---|---|---|
| `FromBigramToJson` | `(NGramModel model) → JsonElement` | Серіалізує `_probs` у JSON |
| `FromJsonElementToBigram` | `(JsonElement json, NGramModel model) → void` | Відновлює `_probs` і `VocabSize` |
| `FromTrigramToJson` | `(TrigramModel model) → JsonElement` | Серіалізує bigram + trigram |
| `FromJsonElementToTrigram` | `(JsonElement json, TrigramModel model) → void` | Відновлює bigram і trigram |

Ключі trigram словника серіалізуються як рядки `"p2, p1"` і розбираються назад при десеріалізації.

---

### `PerplexityCalculator`

Обчислює Perplexity на валідаційному тексті. Менше значення — краща модель.

| Метод | Підпис | Опис |
|---|---|---|
| `ComputePerplexityBigram` | `(NGramModel model, ReadOnlySpan<int> tokens) → float` | Perplexity для bigram |
| `ComputePerplexityTrigram` | `(TrigramModel model, ReadOnlySpan<int> tokens) → float` | Perplexity для trigram |

Повертає `float.PositiveInfinity` якщо `tokens.Length < 2`. При нульовій ймовірності підставляє `1e-10` замість `0` щоб уникнути `log(0)`.

---

### `NGramModelFactory` — реалізує `INgramModelFactory`

| Метод | Підпис | Опис |
|---|---|---|
| `Create` | `(string modelType, int vocabSize) → ILanguageModel` | `"bigram"` → `NGramModel`, `"trigram"` → `TrigramModel`, інше → `ArgumentException` |

---

## Тести

| Клас | К-сть | Що покривають |
|---|---|---|
| `NGramModelTests` | 4 | Train нормальний, один токен, невалідний; NextTokenScores нормальний, порожній, поза межами |
| `TrigramModelTests` | 6 | Train нормальний, два токени, невалідний; NextTokenScores нормальний, нулі, новий токен, bigram fallback |
| `NGramCountsTests` | 7 | CountBigrams, CountTrigrams — нормальний, невідомий токен, від'ємний; GetTotals |
| `NGramModelFactoryTests` | 3 | Create bigram, trigram, невідомий тип |

---

## Очікувана інтеграція для Етапу 2

### Data Pipeline (B1) та Training Data (B2)

```csharp
var factory = new NGramModelFactory();
var model = factory.Create("bigram", tokenizer.VocabSize);
model.Train(tokenizer.Encode(corpus.TrainText));
```

### Runtime (B3)

```csharp
ILanguageModel model = checkpoint.ModelKind switch
{
    "bigram"  => CreateAndLoad<NGramModel>(checkpoint),
    "trigram" => CreateAndLoad<TrigramModel>(checkpoint),
    _         => throw new ArgumentException("Unknown model kind")
};
// де CreateAndLoad викликає model.FromPayload(checkpoint.ModelPayload)
```

### Baseline (B4)

```csharp
var calc = new PerplexityCalculator();
float perplexity = calc.ComputePerplexityBigram(
    (NGramModel)model,
    tokenizer.Encode(corpus.ValText)
);
```

### Neural (B5)

`NextTokenScores` має однаковий підпис у всіх моделей через `ILanguageModel` — семплер може працювати з будь-якою моделлю без змін.

---

## Збірка і тести

```bash
dotnet build Lib.Models.NGram/Lib.Models.NGram.csproj
dotnet test
```
