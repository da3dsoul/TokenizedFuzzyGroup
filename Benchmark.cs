using BenchmarkDotNet.Attributes;

namespace TokenizedFuzzyGroup;

public class Benchmark
{
    private List<string> _stringList = [];
    private double _similarityThreshold;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(69420);
        _similarityThreshold = 0.6;
        _stringList = DataGenerator.GetData(random);
    }

    [Benchmark]
    public List<List<string>> GroupSimilarStrings_Benchmark()
    {
        return Program.GroupSimilarStrings(_stringList, _similarityThreshold);
    }
}
