public class NGramCounts
{
    public void CountBigrams(float[][] probs, ReadOnlySpan<int> tokens)
    {
        
        for (int i = 1; i < tokens.Length; i++)
        {
            if (tokens[i] < 0 || tokens[i] >= probs.Length ||
                tokens[i - 1] < 0 || tokens[i - 1] >= probs.Length)
            {
                throw new ArgumentOutOfRangeException("Token out of vocabulary range");
            }

            probs[tokens[i - 1]][tokens[i]]++;
        }
    }

    public void CountTrigrams(Dictionary<(int, int), float[]> probs, ReadOnlySpan<int> tokens)
    {
        for (int i = 2; i < tokens.Length; i++)
        {
            if (tokens[i] < 0 || tokens[i] >= probs[(0, 0)].Length ||
                tokens[i - 1] < 0 || tokens[i - 1] >= probs[(0, 0)].Length ||
                tokens[i - 2] < 0 || tokens[i - 2] >= probs[(0, 0)].Length
                )
            {
                throw new ArgumentOutOfRangeException("Token out of vocabulary range");
            }

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