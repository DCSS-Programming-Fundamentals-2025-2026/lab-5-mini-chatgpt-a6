namespace Lib.Models.NGram.Statistics;

public sealed class NGramCounts
{
    public int VocabSize { get; }

    private readonly int[,] _bigramCounts;
    private readonly Dictionary<(int P2, int P1), int[]> _trigramCounts;

    public NGramCounts(int vocabSize)
    {
        if (vocabSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vocabSize), "VocabSize must be positive.");
        }

        VocabSize = vocabSize;
        _bigramCounts = new int[vocabSize, vocabSize];
        _trigramCounts = new Dictionary<(int, int), int[]>();
    }

    public void RecordBigram(int prev, int next)
    {
        if (prev < 0 || prev >= VocabSize || next < 0 || next >= VocabSize)
        {
            return;
        }

        _bigramCounts[prev, next]++;
    }

    public int GetBigramCount(int prev, int next)
    {
        return _bigramCounts[prev, next];
    }

    public int GetBigramPrevTotal(int prev)
    {
        int sum = 0;

        for (int i = 0; i < VocabSize; i++)
        {
            sum += _bigramCounts[prev, i];
        }

        return sum;
    }

    public int[,] BigramCounts => _bigramCounts;

    public void RecordTrigram(int p2, int p1, int next)
    {
        if (p2 < 0 || p2 >= VocabSize || p1 < 0 || p1 >= VocabSize || next < 0 || next >= VocabSize)
        {
            return;
        }

        var key = (p2, p1);

        if (!_trigramCounts.TryGetValue(key, out var arr))
        {
            arr = new int[VocabSize];
            _trigramCounts[key] = arr;
        }

        arr[next]++;
    }

    public bool HasTrigram(int p2, int p1)
    {
        return _trigramCounts.ContainsKey((p2, p1));
    }

    public int[] GetTrigramNextCounts(int p2, int p1)
    {
        if (_trigramCounts.TryGetValue((p2, p1), out var arr))
        {
            return arr;
        }

        return Array.Empty<int>();
    }

    public int GetTrigramTotal(int p2, int p1)
    {
        if (!_trigramCounts.TryGetValue((p2, p1), out var arr))
        {
            return 0;
        }

        int sum = 0;

        for (int i = 0; i < arr.Length; i++)
        {
            sum += arr[i];
        }

        return sum;
    }

    public IReadOnlyDictionary<(int P2, int P1), int[]> TrigramCounts => _trigramCounts;
}