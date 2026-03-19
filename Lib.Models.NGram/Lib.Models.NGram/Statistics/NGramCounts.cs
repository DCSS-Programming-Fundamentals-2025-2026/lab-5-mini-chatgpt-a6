public class NGramCounts
{
    public void CountBigrams(float[][] probs, ReadOnlySpan<int> tokens)
    {
        for (int i = 1; i < tokens.Length; i++)
        {
            probs[tokens[i - 1]][tokens[i]]++;
        }
    }

    public void CountTrigrams(Dictionary<(int, int), float[]> probs, ReadOnlySpan<int> tokens)
    {
        for (int i = 2; i < tokens.Length; i++)
        {
            probs[(tokens[i - 2], tokens[i - 1])][tokens[i]]++;
        }
    }

    public int GetBigramPrevTotal(float[][] probs, int vocabSize, int prev)
    {
        int sum = 0;

        for (int i = 0; i < vocabSize; i++)
        {
            sum += (int)probs[prev][i];
        }

        return sum;
    }

    public int GetTrigramPrevsTotal(Dictionary<(int, int), float[]> probs, (int, int) pair)
    {
        int sum = 0;

        for (int j = 0; j < probs[pair].Length; j++)
        {
            sum += (int)probs[pair][j];
        }

        return sum;
    }
}