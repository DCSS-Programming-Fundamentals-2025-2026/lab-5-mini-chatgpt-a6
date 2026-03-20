public class PerplexityCalculatorTests
{
    [Test]
    public void ComputePerplexityBigram_NotEnoughTokens()
    {
        // Arrange
        PerplexityCalculator calculator = new PerplexityCalculator();
        NGramModel model = new NGramModel(3);
        int[] tokens = { 2 };

        // Act
        float result = calculator.ComputePerplexityBigram(model, tokens);

        // Assert
        Assert.That(result, Is.EqualTo(float.PositiveInfinity));
    }

    [Test]
    public void ComputePerplexityTrigram_NotEnoughTokens()
    {
        // Arrange
        PerplexityCalculator calculator = new PerplexityCalculator();
        TrigramModel model = new TrigramModel(3);
        int[] tokens = { };

        // Act
        float result = calculator.ComputePerplexityTrigram(model, tokens);

        // Assert
        Assert.That(result, Is.EqualTo(float.PositiveInfinity));
    }

    [Test]
    public void ComputePerplexityBigram_ZeroProbs()
    {
        // Arrange
        PerplexityCalculator calculator = new PerplexityCalculator();
        NGramModel model = new NGramModel(3);
        int[] tokens = { 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 2 };
        model.Train(tokens);

        tokens = new int[] { 1, 2}; 

        // Act
        float result = calculator.ComputePerplexityBigram(model, tokens);

        // Assert
        Assert.IsTrue(result > 0);
    }

    [Test]
    public void ComputePerplexityTrigram_ZeroProbs()
    {
        // Arrange
        PerplexityCalculator calculator = new PerplexityCalculator();
        TrigramModel model = new TrigramModel(4);
        int[] tokens = { 1, 3, 0, 1, 3, 1, 0 , 3, 0, 3, 2};
        model.Train(tokens);

        tokens = new int[] { 3, 2 };

        // Act
        float result = calculator.ComputePerplexityTrigram(model, tokens);

        // Assert
        Assert.IsTrue(result > 0);
    }
}

