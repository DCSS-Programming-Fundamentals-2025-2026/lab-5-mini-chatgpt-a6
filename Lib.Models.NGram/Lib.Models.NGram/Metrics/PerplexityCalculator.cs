public class PerplexityCalculator
{
    public float ComputePerplexityBigram(NGramModel model, ReadOnlySpan<int> tokens)
    {
        if (tokens.Length < 2)
        {
            return float.PositiveInfinity;
        }

        double logSum = 0;
        int count = 0;

        for (int i = 1; i < tokens.Length; i++)
        {
            int[] context = { tokens[i - 1] };
            float[] probs = model.NextTokenScores(context);

            float prob = probs[tokens[i]];

            if (prob <= 0)
            {
                prob = 0.0000000001f;
            }

            logSum += Math.Log(prob);
            count++;
        }

        double average = logSum / count;
        return (float)Math.Exp(-average);
    }


    public float ComputePerplexityTrigram(TrigramModel model, ReadOnlySpan<int> tokens)
    {
        if (tokens.Length < 2)
        {
            return float.PositiveInfinity;
        }

        double logSum = 0;
        int count = 0;

        for (int i = 1; i < tokens.Length; i++)
        {
            float[] probs;

            if (i >= 2)
            {
                int[] context = { tokens[i - 2], tokens[i - 1] };
                probs = model.NextTokenScores(context);
            }
            else
            {
                int[] context = { tokens[i - 1] };
                probs = model.bigramModel.NextTokenScores(context);
            }

            float prob = probs[tokens[i]];

            if (prob <= 0)
            {
                prob = 0.0000000001f;
            }

            logSum += Math.Log(prob);
            count++;
        }

        double average = logSum / count;
        return (float)Math.Exp(-average);
    }
}