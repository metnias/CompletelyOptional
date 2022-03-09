using System.Text;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Static class that aids generating Lorem Ipsum, placeholder text. See <see cref="Generate(int, int, int)"/> to use.
    /// <para>Source: https://stackoverflow.com/questions/4286487/is-there-any-lorem-ipsum-generator-in-c </para>
    /// </summary>
    public static class LoremIpsum
    {
        private static readonly string[] words = new string[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
                "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
                "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};

        /// <summary>
        /// Average character count per sentence. See <see cref="Generate(int, int, int)"/>
        /// </summary>
        public const int meanCharPerSentence = 60;

        /// <summary>
        /// Generate rough Lorem Ipsum. See also <seealso cref="meanCharPerSentence"/>.
        /// </summary>
        /// <param name="minSentences">Minimum sentences per each paragraph</param>
        /// <param name="maxSentences">Maximum sentences per each paragraph</param>
        /// <param name="numParagraphs">Number of paragraphs divided by linebreak</param>
        /// <returns></returns>
        public static string Generate(int minSentences, int maxSentences, int numParagraphs = 1)
        {
            StringBuilder result = new StringBuilder();

            for (int p = 0; p < numParagraphs; p++)
            {
                int numSentences = Random.Range(minSentences, maxSentences) + 1;
                if (p > 0) { result.Append("\n"); }
                for (int s = 0; s < numSentences; s++)
                {
                    int numWords = Random.Range(4, 12);
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0 || s > 0) { result.Append(" "); }
                        string t = words[Random.Range(0, words.Length - 1)];
                        if (w == 0) { t = t.Substring(0, 1).ToUpper() + t.Substring(1); }
                        result.Append(t);
                    }
                    result.Append(".");
                }
            }
            //ComOptPlugin.LogInfo($"minSen: {minSentences}, maxSen: {maxSentences}, numPar: {numParagraphs}");
            //ComOptPlugin.LogInfo(result.ToString());

            return result.ToString();
        }
    }
}