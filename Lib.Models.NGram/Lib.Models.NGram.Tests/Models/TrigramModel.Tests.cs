public class TrigramModelTests
{
    [Test]
    public void Train_NormalTokens()
    {
        // Arrange
        TrigramModel model = new TrigramModel(3);
        int[] tokens = { 2, 1, 0, 1, 0, 2, 1, 0, 1, 2 };

        // Act
        model.Train(tokens);
        Dictionary<(int, int), float[]> trigramProbs = model._trigramProbs;
        float[][] bigramProbs = model.bigramModel._probs;

        float trigramSum2 = trigramProbs[(0, 1)][0] + trigramProbs[(0, 1)][1] + trigramProbs[(0, 1)][2];
        float trigramSum3 = trigramProbs[(0, 2)][0] + trigramProbs[(0, 2)][1] + trigramProbs[(0, 2)][2];
        float trigramSum4 = trigramProbs[(1, 0)][0] + trigramProbs[(1, 0)][1] + trigramProbs[(1, 0)][2];
        float trigramSum6 = trigramProbs[(1, 2)][0] + trigramProbs[(1, 2)][1] + trigramProbs[(1, 2)][2];
        float trigramSum8 = trigramProbs[(2, 1)][0] + trigramProbs[(2, 1)][1] + trigramProbs[(2, 1)][2];

        float bigramSum1 = bigramProbs[0][0] + bigramProbs[0][1] + bigramProbs[0][2];
        float bigramSum2 = bigramProbs[1][0] + bigramProbs[1][1] + bigramProbs[1][2];
        float bigramSum3 = bigramProbs[2][0] + bigramProbs[2][1] + bigramProbs[2][2];

        // Assert
        Assert.That(bigramProbs[0][1], Is.EqualTo((float)2 / 3).Within(1e-5));
        Assert.That(bigramProbs[0][2], Is.EqualTo((float)1 / 3).Within(1e-5));
        Assert.That(bigramProbs[1][0], Is.EqualTo((float)3/4).Within(1e-5));
        Assert.That(bigramProbs[1][2], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(bigramProbs[2][1], Is.EqualTo((float)1).Within(1e-5));
        
        Assert.That(trigramProbs[(2, 1)][0], Is.EqualTo((float)1).Within(1e-5));
        Assert.That(trigramProbs[(1, 0)][1], Is.EqualTo((float)2/3).Within(1e-5));
        Assert.That(trigramProbs[(1, 0)][2], Is.EqualTo((float)1/3).Within(1e-5));
        Assert.That(trigramProbs[(0, 1)][0], Is.EqualTo((float)1/2).Within(1e-5));
        Assert.That(trigramProbs[(0, 1)][2], Is.EqualTo((float)1/2).Within(1e-5));


        Assert.That(trigramSum2, Is.EqualTo(1).Within(1e-5));
        Assert.That(trigramSum3, Is.EqualTo(1).Within(1e-5));
        Assert.That(trigramSum4, Is.EqualTo(1).Within(1e-5));
        Assert.That(trigramSum6, Is.EqualTo(0).Within(1e-5));
        Assert.That(trigramSum8, Is.EqualTo(1).Within(1e-5));
        
        Assert.That(bigramSum1, Is.EqualTo(1).Within(1e-5));
        Assert.That(bigramSum2, Is.EqualTo(1).Within(1e-5));
        Assert.That(bigramSum3, Is.EqualTo(1).Within(1e-5));
    }

    [Test]
    public void Train_TwoTokens()
    {
        // Arrange
        TrigramModel model = new TrigramModel(3);
        int[] tokens = { 2, 0 };

        // Act
        model.Train(tokens);
        Dictionary<(int, int), float[]> trigramProbs = model._trigramProbs;

        // Assert
        Assert.That(trigramProbs[(0, 1)][2], Is.EqualTo(0).Within(1e-5));
        Assert.That(trigramProbs[(0, 2)][1], Is.EqualTo(0).Within(1e-5));
        Assert.That(trigramProbs[(1, 0)][2], Is.EqualTo(0).Within(1e-5));
        Assert.That(trigramProbs[(1, 2)][0], Is.EqualTo(0).Within(1e-5));
        Assert.That(trigramProbs[(2, 0)][1], Is.EqualTo(0).Within(1e-5));
        Assert.That(trigramProbs[(2, 1)][0], Is.EqualTo(0).Within(1e-5));
    }

    [Test]
    public void Train_ToBigToken()
    {
        // Arrange
        TrigramModel model = new TrigramModel(4);
        int[] tokens = { 1, 0, 2, 1, 3, 2, 4, 2, 1};

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => model.Train(tokens));
    }

    [Test]
    public void NextTokenScores_Normal()
    {
        // Arrange
        TrigramModel model = new TrigramModel(3);
        int[] tokens = { 2, 1, 0, 1, 0, 2, 1, 0, 1, 2 };
        model.Train(tokens);

        int[] context = { 0, 2, 1, 0 };

        // Act
        float[] resultProbs = model.NextTokenScores(context);

        // Assert
        Assert.That(resultProbs[0], Is.EqualTo(0).Within(1e-5));
        Assert.That(resultProbs[1], Is.EqualTo((float)2 / 3).Within(1e-5));
        Assert.That(resultProbs[2], Is.EqualTo((float)1/3).Within(1e-5));
    }

    [Test]
    public void NextTokenScores_OnlyZeros()
    {
        // Arrange
        TrigramModel model = new TrigramModel(4);
        int[] tokens = { 2, 1, 0, 1, 0, 2, 1, 0, 1, 2, 3 };
        model.Train(tokens);

        int[] context = { 2, 3 };

        // Act
        float[] resultProbs = model.NextTokenScores(context);

        // Assert
        Assert.That(resultProbs[0], Is.EqualTo((float)1/4).Within(1e-5));
        Assert.That(resultProbs[1], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[2], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[3], Is.EqualTo((float)1 / 4).Within(1e-5));
    }

    [Test]
    public void NextTokenScores_NewToken()
    {
        // Arrange
        TrigramModel model = new TrigramModel(4);
        int[] tokens = { 2, 1, 0, 1, 0, 2, 1, 0, 1, 2 };
        model.Train(tokens);

        int[] context = { 2, 3 };

        // Act
        float[] resultProbs = model.NextTokenScores(context);

        // Assert
        Assert.That(resultProbs[0], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[1], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[2], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[3], Is.EqualTo((float)1 / 4).Within(1e-5));
    }

    [Test]
    public void NextTokenScores_BigramFall()
    {
        // Arrange
        TrigramModel model = new TrigramModel(3);
        int[] tokens = { 2, 1, 0, 1, 0, 2, 1, 0, 1, 2 };
        model.Train(tokens);

        int[] context = { 1, 1 };

        // Act
        float[] resultProbs = model.NextTokenScores(context);

        // Assert
        Assert.That(resultProbs[0], Is.EqualTo((float)3 / 4).Within(1e-5));
        Assert.That(resultProbs[1], Is.EqualTo(0).Within(1e-5));
        Assert.That(resultProbs[2], Is.EqualTo((float)1 / 4).Within(1e-5));
    }
}

