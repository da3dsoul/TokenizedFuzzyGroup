using BenchmarkDotNet.Attributes;

namespace TokenizedFuzzyGroup;

public class Benchmark
{
    private readonly List<string> _stringList = [];
    private double _similarityThreshold;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random();
        _similarityThreshold = 0.6;

        var basePhrases = new List<string>
        {
            "the quick brown fox",
            "jumps over the lazy dog",
            "a wizard cast a spell",
            "clouds gather before storm",
            "programmers love clean code",
            "music soothes the soul",
            "reading expands the mind",
            "dogs bark at strangers",
            "cats sleep all day",
            "robots are taking over"
        };

        const int groupCount = 100;

        for (var i = 0; i < groupCount; i++)
        {
            // Pick a base phrase
            var basePhrase = basePhrases[random.Next(basePhrases.Count)];

            // Generate 1 to 10 similar strings
            var groupSize = random.Next(1, 11);
            for (var j = 0; j < groupSize; j++)
            {
                var variation = GenerateVariation(basePhrase, random);
                _stringList.Add(variation);
            }
        }
    }

    private string GenerateVariation(string basePhrase, Random random)
    {
        var words = basePhrase.Split(' ');
        var operation = random.Next(3); // 0: replace, 1: insert, 2: delete

        switch (operation)
        {
            case 0: // Replace a word
                if (words.Length > 0)
                    words[random.Next(words.Length)] = GetRandomWord(random);
                break;
            case 1: // Insert a word
                var wordList = new List<string>(words);
                wordList.Insert(random.Next(wordList.Count + 1), GetRandomWord(random));
                words = wordList.ToArray();
                break;
            case 2: // Delete a word
                if (words.Length > 1)
                {
                    var wordList2 = new List<string>(words);
                    wordList2.RemoveAt(random.Next(wordList2.Count));
                    words = wordList2.ToArray();
                }
                break;
        }

        return string.Join(' ', words);
    }

    private string GetRandomWord(Random random)
    {
        string[] randomWords =
        [
            "fast", "silent", "bright", "dark", "code", "spell", "storm", "love", "robot", "cat", "dog", "jump",
            "wizard", "fox", "lazy", "quick", "mind", "soul", "music", "book"
        ];
        return randomWords[random.Next(randomWords.Length)];
    }


    [Benchmark]
    public List<List<string>> GroupSimilarStrings_Benchmark()
    {
        return Program.GroupSimilarStrings(_stringList, _similarityThreshold);
    }
}
