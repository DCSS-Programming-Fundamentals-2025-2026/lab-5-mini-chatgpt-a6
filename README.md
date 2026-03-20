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
Lib.Models.NGram/
├── Statistics/
│   └── NGramCounts.cs
├── Serialization/
│   └── NGramPayloadMapper.cs
├── Metrics/
│   └── PerplexityCalculator.cs
├── NGramModel.cs
├── TrigramModel.cs
├── INGramModelFactory.cs
└── NGramModelFactory.cs
```

---

## Класи та методи

### `NGramCounts`

Зберігає сирі лічильники під час навчання. Використовується як `NGramModel`, так і `TrigramModel`.

| Метод | Підпис | Опис |
|---|---|---|
| `RecordBigram` | `(int prev, int next) → void` | Записує факт що після `prev` йшов `next` |
| `GetBigramCount` | `(int prev, int next) → int` | Повертає лічильник конкретної пари |
| `GetBigramPrevTotal` | `(int prev) → int` | Сума рядка — потрібна для нормалізації |
| `RecordTrigram` | `(int p2, int p1, int next) → void` | Записує трійку токенів |
| `HasTrigram` | `(int p2, int p1) → bool` | Чи зустрічалась пара у тренуванні |
| `GetTrigramNextCounts` | `(int p2, int p1) → int[]` | Масив лічильників для пари |
| `GetTrigramTotal` | `(int p2, int p1) → int` | Сума лічильників для пари |

---

### `NGramModel` — реалізує `ILanguageModel`

Bigram модель. `ModelKind` = `"ngram"`.

| Метод | Підпис | Опис |
|---|---|---|
| `Train` | `(ReadOnlySpan<int> tokens) → void` | Навчання: підрахунок пар і нормалізація |
| `NextTokenScores` | `(ReadOnlySpan<int> context) → float[]` | Повертає ймовірності для наступного токена |
| `GetPayloadForCheckpoint` | `() → object` | Серіалізує матрицю ймовірностей |
| `FromPayload` | `(JsonElement) → NGramModel` | Відновлює модель з checkpoint |
| `GetContractFingerprint` | `() → string` | Хеш версії контракту |

**Поведінка `NextTokenScores` при крайніх випадках** — у всіх випадках нижче повертається рівномірний розподіл `1/VocabSize`:
- порожній контекст
- токен поза межами словника
- токен не зустрічався у тренуванні

---

### `TrigramModel` — реалізує `ILanguageModel`

Trigram модель з fallback на Bigram. `ModelKind` = `"trigram"`.

| Метод | Підпис | Опис |
|---|---|---|
| `Train` | `(ReadOnlySpan<int> tokens) → void` | Навчання: підрахунок пар і трійок |
| `NextTokenScores` | `(ReadOnlySpan<int> context) → float[]` | Trigram якщо є, інакше Bigram fallback |
| `GetPayloadForCheckpoint` | `() → object` | Серіалізує bigram + trigram |
| `FromPayload` | `(JsonElement) → TrigramModel` | Відновлює з checkpoint |

---

### `NGramPayloadMapper`

Статичний клас. Серіалізація і десеріалізація стану моделей.

| Метод | Опис |
|---|---|
| `SerializeBigram` | `float[,]` → JSON-сумісний об'єкт |
| `DeserializeBigram` | `JsonElement` → `float[,]` |
| `SerializeTrigram` | bigram + trigram словник → JSON |
| `DeserializeTrigram` | `JsonElement` → `(float[,], Dictionary)` |

---

### `PerplexityCalculator`

Обчислює Perplexity моделі на валідаційному тексті.

| Метод | Підпис | Опис |
|---|---|---|
| `Compute` | `(ILanguageModel, ReadOnlySpan<int>) → double` | Повертає Perplexity. Менше — краще |

---

### `INGramModelFactory` / `NGramModelFactory`

Фабрика моделей.

| Метод | Підпис | Опис |
|---|---|---|
| `Create` | `(string modelType, int vocabSize) → ILanguageModel` | `"ngram"` → `NGramModel`, `"trigram"` → `TrigramModel` |

---

## Очікувана інтеграція для Етапу 2

### Data Pipeline (B1) та Training Data (B2)

Після побудови токенізатора і кодування корпусу передаєте масив токенів у `model.Train(tokens)`. Модель створюється через фабрику:

```csharp
var factory = new NGramModelFactory();
var model = factory.Create("trigram", tokenizer.VocabSize);
model.Train(tokenizer.Encode(corpus.TrainText));
```

### Runtime (B3)

При завантаженні checkpoint використовуйте `NGramModel.FromPayload` або `TrigramModel.FromPayload` залежно від значення `checkpoint.ModelKind`. Після відновлення модель готова до виклику `NextTokenScores`.

```csharp
ILanguageModel model = checkpoint.ModelKind switch
{
    "ngram"   => NGramModel.FromPayload(checkpoint.ModelPayload),
    "trigram" => TrigramModel.FromPayload(checkpoint.ModelPayload),
    _         => throw new ArgumentException("Unknown model kind")
};
```

### Baseline (B4)

Для оцінки якості використовуйте `PerplexityCalculator.Compute(model, valTokens)` де `valTokens = tokenizer.Encode(corpus.ValText)`.

### Neural (B5)

`NextTokenScores` має однаковий підпис у всіх моделей — Neural компонент може використовувати ту саму логіку семплінгу без змін.

---

## Збірка і тести

```bash
dotnet build src/Lib.Models.NGram
dotnet test tests/Lib.Models.NGram.Tests
```