public class NGramCountsTests
{
    [Test]
    public void CountBigrams_NormalTokens()
    {
        // Arrange
        NGramCounts counts = new NGramCounts();
        float[][] probs =
        {
            new float[3],
            new float[3],
            new float[3]
        };

        int[] tokens = { 0, 1, 1, 2, 0, 1, 2, 1, 2, 0, 1 };

        // Act
        counts.CountBigrams(probs, tokens);

        // Assert
        Assert.That(probs[0][0], Is.EqualTo(0));
        Assert.That(probs[0][1], Is.EqualTo(3));
        Assert.That(probs[0][2], Is.EqualTo(0));
        Assert.That(probs[1][0], Is.EqualTo(0));
        Assert.That(probs[1][1], Is.EqualTo(1));
        Assert.That(probs[1][2], Is.EqualTo(3));
        Assert.That(probs[2][0], Is.EqualTo(2));
        Assert.That(probs[2][1], Is.EqualTo(1));
        Assert.That(probs[2][2], Is.EqualTo(0));
    }

    [Test]
    public void CountBigrams_UnknownToken()
    {
        // Arrange
        NGramCounts counts = new NGramCounts();
        float[][] probs =
        {
            new float[3],
            new float[3],
            new float[3]
        };

        int[] tokens = { 0, 1, 1, 3, 0, 1, 2, 1, 2, 0, 1 };

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => counts.CountBigrams(probs, tokens));       
    }

    [Test]
    public void CountBigrams_NegativeToken()
    {
        // Arrange
        NGramCounts counts = new NGramCounts();
        float[][] probs =
        {
            new float[3],
            new float[3],
            new float[3]
        };

        int[] tokens = { 0, -1, 1, 0, 0, 1, 2, 1, 2, 0, 1 };

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => counts.CountBigrams(probs, tokens));
    }

    [Test]
    public void CountTrigrams_NormalTokens()
    {
        // Arrange
        NGramCounts counts = new NGramCounts();
        Dictionary<(int, int), float[]> probs = new Dictionary<(int, int), float[]>();
        probs[(0, 0)] = new float[3];
        probs[(0, 1)] = new float[3];
        probs[(0, 2)] = new float[3];
        probs[(1, 0)] = new float[3];
        probs[(1, 1)] = new float[3];
        probs[(1, 2)] = new float[3];
        probs[(2, 0)] = new float[3];
        probs[(2, 1)] = new float[3];
        probs[(2, 2)] = new float[3];

        int[] tokens = { 2, 1, 0, 1, 0, 2, 1, 0, 1, 2 };

        // Act
        counts.CountTrigrams(probs, tokens);

        // Assert
        Assert.That(probs[(0, 1)][0], Is.EqualTo(1));
        Assert.That(probs[(0, 1)][1], Is.EqualTo(0));
        Assert.That(probs[(0, 1)][2], Is.EqualTo(1));
        Assert.That(probs[(1, 0)][0], Is.EqualTo(0));
        Assert.That(probs[(1, 0)][1], Is.EqualTo(2));
        Assert.That(probs[(1, 0)][2], Is.EqualTo(1));
        Assert.That(probs[(2, 1)][0], Is.EqualTo(2));
        Assert.That(probs[(2, 1)][1], Is.EqualTo(0));
        Assert.That(probs[(2, 1)][2], Is.EqualTo(0));

    }

    [Test]
    public void CountTrigrams_UnknownToken()
    {
        // Arrange
        NGramCounts counts = new NGramCounts();
        Dictionary<(int, int), float[]> probs = new Dictionary<(int, int), float[]>();
        probs[(0, 0)] = new float[3];
        probs[(0, 1)] = new float[3];
        probs[(0, 2)] = new float[3];
        probs[(1, 0)] = new float[3];
        probs[(1, 1)] = new float[3];
        probs[(1, 2)] = new float[3];
        probs[(2, 0)] = new float[3];
        probs[(2, 1)] = new float[3];
        probs[(2, 2)] = new float[3];

        int[] tokens = { 0, 1, 1, 3, 0, 1, 2, 1, 2, 0, 1, 2 };

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => counts.CountTrigrams(probs, tokens));
    }

    [Test]
    public void CountTrigrams_NegativeToken()
    {
        // Arrange
        NGramCounts counts = new NGramCounts();
        Dictionary<(int, int), float[]> probs = new Dictionary<(int, int), float[]>();
        probs[(0, 0)] = new float[3];
        probs[(0, 1)] = new float[3];
        probs[(0, 2)] = new float[3];
        probs[(1, 0)] = new float[3];
        probs[(1, 1)] = new float[3];
        probs[(1, 2)] = new float[3];
        probs[(2, 0)] = new float[3];
        probs[(2, 1)] = new float[3];
        probs[(2, 2)] = new float[3];

        int[] tokens = { 0, 1, -1, 3, 0, 1, 2, 1, 2, 0, 1, 2 };

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => counts.CountTrigrams(probs, tokens));
    }

    [Test]
    public void CountBigramPrevTotal()
    {
        // Arrange
        NGramCounts counts = new NGramCounts();
        float[][] probs =
        {
            [0, 1, 2, 0],
            [1, 1, 0, 2],
            [1, 0, 1, 0],
            [1, 1, 1, 1]
        };

        int[] tokens = { 0, 1, 1, 2, 0, 1, 2, 1, 2, 0, 1 };

        // Act
        int sum = counts.GetBigramPrevTotal(probs, 4, 1);

        // Assert
        Assert.That(sum, Is.EqualTo(4));
    }

    [Test]
    public void GetTrigramPrevsTotal()
    {
        // Arrange
        NGramCounts counts = new NGramCounts();
        Dictionary<(int, int), float[]> probs = new Dictionary<(int, int), float[]>();
        probs[(0, 0)] = new float[] { 2, 1, 0};
        probs[(0, 1)] = new float[] { 1, 1, 1 };
        probs[(0, 2)] = new float[] { 0, 3, 0 };
        probs[(1, 0)] = new float[] { 2, 1, 3 };
        probs[(1, 1)] = new float[] { 0, 0, 0 };
        probs[(1, 2)] = new float[] { 0, 1, 4 };
        probs[(2, 0)] = new float[] { 4, 1, 4 };
        probs[(2, 1)] = new float[] { 2, 2, 0 };
        probs[(2, 2)] = new float[] { 0, 0, 0 };

        int[] tokens = { 0, 1, 1, 2, 0, 1, 2, 1, 2, 0, 1, 2, 1 };

        // Act
        int sum = counts.GetTrigramPrevsTotal(probs, (1, 0));

        // Assert
        Assert.That(sum, Is.EqualTo(6));
    }
}

