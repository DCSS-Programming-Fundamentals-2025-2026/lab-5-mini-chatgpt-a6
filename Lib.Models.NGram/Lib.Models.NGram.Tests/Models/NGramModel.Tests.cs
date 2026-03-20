public class NGramModelTests
{
    [Test]
    public void Train_NormalTokens()
    {
        // Arrange
        NGramModel model = new NGramModel(4);
        int[] tokens = { 2, 3, 0, 3, 1, 2, 0, 2, 3, 1};

        // Act
        model.Train(tokens);
        float[][] probs = model._probs;

        float sum1 = probs[0][0] + probs[0][1] + probs[0][2] + probs[0][3];
        float sum2 = probs[1][0] + probs[1][1] + probs[1][2] + probs[1][3];
        float sum3 = probs[2][0] + probs[2][1] + probs[2][2] + probs[2][3];
        float sum4 = probs[3][0] + probs[3][1] + probs[3][2] + probs[3][3];

        // Assert
        Assert.That(probs[0][2], Is.EqualTo((float)1/2).Within(1e-5));
        Assert.That(probs[0][3], Is.EqualTo((float)1/2).Within(1e-5));
        Assert.That(probs[1][2], Is.EqualTo((float)1).Within(1e-5));
        Assert.That(probs[2][0], Is.EqualTo((float)1/3).Within(1e-5));
        Assert.That(probs[2][3], Is.EqualTo((float)2/3).Within(1e-5));
        Assert.That(probs[3][0], Is.EqualTo((float)1/3).Within(1e-5));
        Assert.That(probs[3][1], Is.EqualTo((float)2/3).Within(1e-5));
        
        Assert.That(sum1, Is.EqualTo(1).Within(1e-5));
        Assert.That(sum2, Is.EqualTo(1).Within(1e-5));
        Assert.That(sum3, Is.EqualTo(1).Within(1e-5));
        Assert.That(sum4, Is.EqualTo(1).Within(1e-5));
    }

    [Test]
    public void Train_OneToken()
    {
        // Arrange
        NGramModel model = new NGramModel(3);
        int[] tokens = { 2 };

        // Act
        model.Train(tokens);
        float[][] probs = model._probs;

        // Assert
        Assert.That(probs[0][0], Is.EqualTo(0).Within(1e-5));
        Assert.That(probs[0][1], Is.EqualTo(0).Within(1e-5));
        Assert.That(probs[0][2], Is.EqualTo(0).Within(1e-5));
        Assert.That(probs[1][0], Is.EqualTo(0).Within(1e-5));
        Assert.That(probs[1][1], Is.EqualTo(0).Within(1e-5));
        Assert.That(probs[1][2], Is.EqualTo(0).Within(1e-5));
        Assert.That(probs[2][0], Is.EqualTo(0).Within(1e-5));
        Assert.That(probs[2][1], Is.EqualTo(0).Within(1e-5));
        Assert.That(probs[2][2], Is.EqualTo(0).Within(1e-5));
    }

    [Test]
    public void Train_ToBigToken()
    {
        // Arrange
        NGramModel model = new NGramModel(3);
        int[] tokens = { 1, 0, 2, 1, 3, 2 };

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => model.Train(tokens));
    }

    [Test]
    public void NextTokenScores_Normal()
    {
        // Arrange
        NGramModel model = new NGramModel(4);
        int[] tokens = { 2, 3, 0, 3, 1, 2, 0, 2, 3, 1 };
        model.Train(tokens);

        int[] context = { 2, 3 };

        // Act
        float[] resultProbs = model.NextTokenScores(context);

        // Assert
        Assert.That(resultProbs[0], Is.EqualTo((float)1 / 3).Within(1e-5));
        Assert.That(resultProbs[1], Is.EqualTo((float)2 / 3).Within(1e-5));
        Assert.That(resultProbs[2], Is.EqualTo((float)0).Within(1e-5));
        Assert.That(resultProbs[3], Is.EqualTo((float)0).Within(1e-5));
    }

    [Test]
    public void NextTokenScores_EmptyContext()
    {
        // Arrange
        NGramModel model = new NGramModel(4);
        int[] tokens = { 2, 3, 0, 3, 1, 2, 0, 2, 3, 1 };
        model.Train(tokens);

        int[] context = { };

        // Act
        float[] resultProbs = model.NextTokenScores(context);

        // Assert
        Assert.That(resultProbs[0], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[1], Is.EqualTo((float)1/4).Within(1e-5));
        Assert.That(resultProbs[2], Is.EqualTo((float)1/4).Within(1e-5));
        Assert.That(resultProbs[3], Is.EqualTo((float)1/4).Within(1e-5));
    }

    [Test]
    public void NextTokenScores_NewToken()
    {
        // Arrange
        NGramModel model = new NGramModel(4);
        int[] tokens = { 2, 3, 0, 3, 1, 2, 0, 2, 3, 1 };
        model.Train(tokens);

        int[] context = { 1, 0, 4 };

        // Act
        float[] resultProbs = model.NextTokenScores(context);

        // Assert
        Assert.That(resultProbs[0], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[1], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[2], Is.EqualTo((float)1 / 4).Within(1e-5));
        Assert.That(resultProbs[3], Is.EqualTo((float)1 / 4).Within(1e-5));
    }
}

