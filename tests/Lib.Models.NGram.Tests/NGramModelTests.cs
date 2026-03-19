using System.Text.Json;
using NUnit.Framework;
using Lib.Models.NGram;
using Lib.Models.NGram.Statistics;

namespace Lib.Models.NGram.Tests;

[TestFixture]
public class NGramModelTests
{
    [Test]
    public void Train_NormalCorpus_CorrectProbabilities()
    {
        var model = new NGramModel(3);
        model.Train(new[] { 0, 1, 0, 2 });

        float[] after0 = model.NextTokenScores(new[] { 0 });

        Assert.That(after0[0], Is.EqualTo(0.0f).Within(1e-6f));
        Assert.That(after0[1], Is.EqualTo(0.5f).Within(1e-6f));
        Assert.That(after0[2], Is.EqualTo(0.5f).Within(1e-6f));

        float[] after1 = model.NextTokenScores(new[] { 1 });

        Assert.That(after1[0], Is.EqualTo(1.0f).Within(1e-6f));
    }

    [Test]
    public void Train_RepeatedPairs_CorrectWeights()
    {
        var model = new NGramModel(3);
        model.Train(new[] { 0, 1, 0, 1, 0, 1 });

        float[] after0 = model.NextTokenScores(new[] { 0 });

        Assert.That(after0[1], Is.EqualTo(1.0f).Within(1e-6f));
    }

    [Test]
    public void NextTokenScores_LengthEqualsVocabSize()
    {
        var model = new NGramModel(10);
        model.Train(new[] { 0, 1, 2, 3, 4 });

        float[] scores = model.NextTokenScores(new[] { 0 });

        Assert.That(scores.Length, Is.EqualTo(10));
    }

    [Test]
    public void NextTokenScores_KnownToken_SumIsOne()
    {
        var model = new NGramModel(5);
        model.Train(new[] { 0, 1, 2, 0, 3, 1, 4 });

        float[] scores = model.NextTokenScores(new[] { 0 });

        Assert.That(scores.Sum(), Is.EqualTo(1.0f).Within(1e-5f));
    }

    [Test]
    public void NextTokenScores_UsesOnlyLastContextToken()
    {
        var model = new NGramModel(4);
        model.Train(new[] { 0, 1, 2, 0, 3 });

        float[] shortContext = model.NextTokenScores(new[] { 0 });
        float[] longContext  = model.NextTokenScores(new[] { 3, 3, 3, 0 });

        Assert.That(longContext[0], Is.EqualTo(shortContext[0]).Within(1e-6f));
        Assert.That(longContext[1], Is.EqualTo(shortContext[1]).Within(1e-6f));
        Assert.That(longContext[2], Is.EqualTo(shortContext[2]).Within(1e-6f));
        Assert.That(longContext[3], Is.EqualTo(shortContext[3]).Within(1e-6f));
    }

    [Test]
    public void ModelKind_ReturnsNgram()
    {
        var model = new NGramModel(5);

        Assert.That(model.ModelKind, Is.EqualTo("ngram"));
    }

    [Test]
    public void VocabSize_ReturnsCorrectValue()
    {
        var model = new NGramModel(42);

        Assert.That(model.VocabSize, Is.EqualTo(42));
    }

    [Test]
    public void Train_EmptyTokens_DoesNotThrow()
    {
        var model = new NGramModel(5);

        Assert.DoesNotThrow(() => model.Train(Array.Empty<int>()));
    }

    [Test]
    public void Train_SingleToken_DoesNotThrow()
    {
        var model = new NGramModel(5);

        Assert.DoesNotThrow(() => model.Train(new[] { 0 }));
    }

    [Test]
    public void Train_EmptyCorpus_ThenNextTokenScores_ReturnsUniform()
    {
        var model = new NGramModel(4);
        model.Train(Array.Empty<int>());

        float[] scores = model.NextTokenScores(new[] { 0 });
        float expected = 1.0f / 4;

        Assert.That(scores, Has.All.EqualTo(expected).Within(1e-6f));
    }

    [Test]
    public void NextTokenScores_EmptyContext_ReturnsUniform()
    {
        var model = new NGramModel(4);
        model.Train(new[] { 0, 1, 2, 3 });

        float[] scores = model.NextTokenScores(Array.Empty<int>());
        float expected = 1.0f / 4;

        Assert.That(scores, Has.All.EqualTo(expected).Within(1e-6f));
    }

    [Test]
    public void NextTokenScores_UnseenToken_ReturnsUniform()
    {
        var model = new NGramModel(5);
        model.Train(new[] { 0, 1, 2, 0 });

        float[] scores = model.NextTokenScores(new[] { 4 });
        float expected = 1.0f / 5;

        Assert.That(scores, Has.All.EqualTo(expected).Within(1e-6f));
    }

    [Test]
    public void NextTokenScores_UnseenToken_SumIsOne()
    {
        var model = new NGramModel(6);
        model.Train(new[] { 0, 1, 2 });

        float[] scores = model.NextTokenScores(new[] { 5 });

        Assert.That(scores.Sum(), Is.EqualTo(1.0f).Within(1e-5f));
    }

    [Test]
    public void NextTokenScores_OutOfRangeToken_ReturnsUniform()
    {
        var model = new NGramModel(5);
        model.Train(new[] { 0, 1, 2 });

        float[] scores = model.NextTokenScores(new[] { 999 });
        float expected = 1.0f / 5;

        Assert.That(scores, Has.All.EqualTo(expected).Within(1e-6f));
    }

    [Test]
    public void Checkpoint_RoundTrip_RestoresCorrectProbabilities()
    {
        var original = new NGramModel(3);
        original.Train(new[] { 0, 1, 2, 0, 1, 0, 2, 1 });

        object payload = original.GetPayloadForCheckpoint();
        string json = JsonSerializer.Serialize(payload);
        JsonElement element = JsonSerializer.Deserialize<JsonElement>(json);
        NGramModel restored = NGramModel.FromPayload(element);

        float[] origAfter0 = original.NextTokenScores(new[] { 0 });
        float[] restAfter0 = restored.NextTokenScores(new[] { 0 });
        Assert.That(restAfter0[0], Is.EqualTo(origAfter0[0]).Within(1e-6f));
        Assert.That(restAfter0[1], Is.EqualTo(origAfter0[1]).Within(1e-6f));
        Assert.That(restAfter0[2], Is.EqualTo(origAfter0[2]).Within(1e-6f));

        float[] origAfter1 = original.NextTokenScores(new[] { 1 });
        float[] restAfter1 = restored.NextTokenScores(new[] { 1 });
        Assert.That(restAfter1[0], Is.EqualTo(origAfter1[0]).Within(1e-6f));
        Assert.That(restAfter1[1], Is.EqualTo(origAfter1[1]).Within(1e-6f));
        Assert.That(restAfter1[2], Is.EqualTo(origAfter1[2]).Within(1e-6f));
    }

    [Test]
    public void Checkpoint_RoundTrip_PreservesVocabSize()
    {
        var original = new NGramModel(7);
        original.Train(new[] { 0, 1, 2, 3 });

        object payload = original.GetPayloadForCheckpoint();
        string json = JsonSerializer.Serialize(payload);
        JsonElement element = JsonSerializer.Deserialize<JsonElement>(json);
        NGramModel restored = NGramModel.FromPayload(element);

        Assert.That(restored.VocabSize, Is.EqualTo(7));
        Assert.That(restored.ModelKind, Is.EqualTo("ngram"));
    }

    [Test]
    public void NGramCounts_RecordBigram_CorrectCounts()
    {
        var counts = new NGramCounts(3);
        counts.RecordBigram(0, 1);
        counts.RecordBigram(0, 1);
        counts.RecordBigram(0, 2);

        Assert.That(counts.GetBigramCount(0, 1), Is.EqualTo(2));
        Assert.That(counts.GetBigramCount(0, 2), Is.EqualTo(1));
        Assert.That(counts.GetBigramCount(1, 2), Is.EqualTo(0));
    }

    [Test]
    public void NGramCounts_PrevTotal_CorrectSum()
    {
        var counts = new NGramCounts(3);
        counts.RecordBigram(0, 1);
        counts.RecordBigram(0, 2);
        counts.RecordBigram(0, 0);

        Assert.That(counts.GetBigramPrevTotal(0), Is.EqualTo(3));
        Assert.That(counts.GetBigramPrevTotal(1), Is.EqualTo(0));
    }

    [Test]
    public void NGramCounts_OutOfRangeTokens_Ignored()
    {
        var counts = new NGramCounts(3);

        Assert.DoesNotThrow(() => counts.RecordBigram(-1, 0));
        Assert.DoesNotThrow(() => counts.RecordBigram(0, 100));
        Assert.DoesNotThrow(() => counts.RecordBigram(99, 99));
        Assert.That(counts.GetBigramPrevTotal(0), Is.EqualTo(0));
    }
}