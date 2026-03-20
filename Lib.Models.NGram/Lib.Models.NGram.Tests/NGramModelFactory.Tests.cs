public class NGramModelFactoryTests
{
    [Test]
    public void Create_Bigram()
    {
        // Arrange
        NGramModelFactory factory = new NGramModelFactory();
        NGramModel expectedModel = new NGramModel(4);

        // Act
        ILanguageModel actualModel = factory.Create("bigram", 4);

        // Assert
        Assert.IsTrue(actualModel.Equals(expectedModel));
    }

    [Test]
    public void Create_Trigram()
    {
        // Arrange
        NGramModelFactory factory = new NGramModelFactory();
        TrigramModel expectedModel = new TrigramModel(4);

        // Act
        ILanguageModel actualModel = factory.Create("trigram", 4);

        // Assert
        Assert.IsTrue(actualModel.Equals(expectedModel));
    }

    [Test]
    public void Create_Unknown()
    {
        // Arrange
        NGramModelFactory factory = new NGramModelFactory();

        // Act + Assert
        Assert.Throws<ArgumentException>(() => factory.Create("gram", 3));
    }
}

