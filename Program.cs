namespace TokenizedFuzzyGroup;

/// <summary>
/// Provides functionality to group similar strings together.
/// </summary>
public static class Program
{
    /// <summary>
    /// Groups a list of strings based on the similarity of their token sets.
    /// It attempts to preserve the original order of the strings. The first string in each group
    /// acts as the representative for comparison.
    /// </summary>
    /// <param name="strings">The list of strings to group.</param>
    /// <param name="similarityThreshold">
    /// A value between 0.0 and 1.0. Strings with a token set similarity score greater than or
    /// equal to this threshold will be considered part of the same group. This threshold is also
    /// used for fuzzy matching individual tokens.
    /// </param>
    /// <returns>A list of lists, where each inner list is a group of similar strings.</returns>
    public static List<List<string>> GroupSimilarStrings(IEnumerable<string> strings, double similarityThreshold)
    {
        if (strings == null)
        {
            throw new ArgumentNullException(nameof(strings));
        }
        if (similarityThreshold < 0.0 || similarityThreshold > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(similarityThreshold), "Threshold must be between 0.0 and 1.0.");
        }

        var stringList = strings.ToList();
        var groups = new List<List<string>>();
        // Using a HashSet for faster lookups of indices that have already been grouped.
        var groupedIndices = new HashSet<int>();

        for (var i = 0; i < stringList.Count; i++)
        {
            if (groupedIndices.Contains(i))
            {
                continue;
            }

            var currentString = stringList[i];
            var foundGroup = false;
            foreach (var group in groups)
            {
                // Compare strings based on their token set similarity.
                if (CalculateTokenSetSimilarity(currentString, group[0], similarityThreshold) >= similarityThreshold)
                {
                    group.Add(currentString);
                    groupedIndices.Add(i);
                    foundGroup = true;
                    break;
                }
            }

            if (!foundGroup)
            {
                // If no suitable group was found, create a new one with the current string.
                var newGroup = new List<string> { currentString };
                groups.Add(newGroup);
                groupedIndices.Add(i);
            }
        }

        return groups;
    }

    /// <summary>
    /// Calculates a similarity score between two strings based on their tokens (words).
    /// The score is determined by finding the number of fuzzy-matched tokens between the two strings.
    /// </summary>
    /// <param name="s1">The first string.</param>
    /// <param name="s2">The second string.</param>
    /// <param name="tokenSimilarityThreshold">The threshold for considering two tokens as a match.</param>
    /// <returns>The token set similarity score between 0.0 and 1.0.</returns>
    private static double CalculateTokenSetSimilarity(string s1, string s2, double tokenSimilarityThreshold)
    {
        var tokens1 = Tokenize(s1);
        var tokens2 = Tokenize(s2);

        if (tokens1.Length == 0 || tokens2.Length == 0)
        {
            return 0.0;
        }
        
        if (s1.Equals(s2, StringComparison.OrdinalIgnoreCase))
        {
            return 1.0;
        }

        var matches = 0;
        var availableTokens2 = tokens2.ToList();

        foreach (var token1 in tokens1)
        {
            var bestScore = 0.0;
            var bestMatchIndex = -1;

            // Find the best fuzzy match for the current token in the second string's token list.
            for (var i = 0; i < availableTokens2.Count; i++)
            {
                var currentScore = CalculateSorensenDice(token1, availableTokens2[i]);
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestMatchIndex = i;
                }
            }

            // If a sufficiently good match was found, count it and remove the token to prevent re-matching.
            if (bestMatchIndex != -1 && bestScore >= tokenSimilarityThreshold)
            {
                matches++;
                availableTokens2.RemoveAt(bestMatchIndex);
            }
        }

        // Use the Sørensen-Dice formula on the token counts to get the final similarity score.
        return (2.0 * matches) / (tokens1.Length + tokens2.Length);
    }

    /// <summary>
    /// Splits a string into an array of lower-case tokens based on spaces.
    /// </summary>
    /// <param name="str">The string to tokenize.</param>
    /// <returns>An array of string tokens.</returns>
    private static string[] Tokenize(string str)
    {
        return str.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Calculates the Sørensen-Dice coefficient between two strings (used here for token-level comparison).
    /// The coefficient is a value between 0 and 1, where 1 indicates identical strings.
    /// The comparison is case-sensitive.
    /// </summary>
    /// <param name="s1">The first string.</param>
    /// <param name="s2">The second string.</param>
    /// <returns>The similarity score between 0.0 and 1.0.</returns>
    private static double CalculateSorensenDice(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
        {
            return 0.0;
        }

        if (s1 == s2)
        {
            return 1.0;
        }
        
        // Get the set of bigrams (pairs of adjacent characters) for each string.
        var s1Bigrams = GetBigrams(s1);
        var s2Bigrams = GetBigrams(s2);
        
        // Count the number of intersecting bigrams.
        var intersection = s1Bigrams.Intersect(s2Bigrams).Count();
        
        // The formula for the Sørensen-Dice coefficient.
        return (2.0 * intersection) / (s1Bigrams.Count + s2Bigrams.Count);
    }

    /// <summary>
    /// Generates a set of character bigrams from a string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>A HashSet of bigrams.</returns>
    private static HashSet<string> GetBigrams(string str)
    {
        var bigrams = new HashSet<string>();
        for (var i = 0; i < str.Length - 1; i++)
        {
            bigrams.Add(str.Substring(i, 2));
        }
        return bigrams;
    }
    
    /// <summary>
    /// The main entry point for the application to demonstrate the string grouping functionality.
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "benchmark")
        {
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<Benchmark>();
            return;
        }

        var stringList = new List<string>
        {
            "the quick brown fox",
            "a quick brown fox",    // Similar to the first
            "the quiet brown cat",  // Somewhat similar to the first
            "jumps over the lazy dog",
            "jumped over a lazy dog", // Similar to the one above
            "the lazy dog sleeps",
            "the five boxing wizards jump quickly", // Unique
            "a quick brown fox"     // Duplicate, should be grouped
        };

        Console.WriteLine("Original List of Strings:");
        stringList.ForEach(s => Console.WriteLine($"- \"{s}\""));
        Console.WriteLine(new string('=', 30));

        // --- Configurable Threshold ---
        // A higher threshold (e.g., 0.8) means strings must be very similar to be grouped.
        // A lower threshold (e.g., 0.5) will create broader, less similar groups.
        var threshold = 0.6; 
        
        Console.WriteLine($"Grouping with a similarity threshold of: {threshold}\n");

        var groupedResults = GroupSimilarStrings(stringList, threshold);

        var groupNumber = 1;
        foreach (var group in groupedResults)
        {
            Console.WriteLine($"--- Group {groupNumber++} ---");
            foreach (var item in group)
            {
                Console.WriteLine($"  - \"{item}\"");
            }
            Console.WriteLine();
        }
    }
}