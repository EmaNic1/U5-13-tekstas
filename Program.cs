using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Tekstas_U5_13
{
    internal class Program
    {
        const string PD = "Duomenys.txt";
        const string RZ = "Rezultatai.txt";
        const string AN = "Analize.txt";

        static void Main(string[] args)
        {
            // separators
            string separators = " ,.();:'-{}!?><&%^$%*";

            if (File.Exists(RZ) || File.Exists(AN))
            {
                File.Delete(RZ);
                File.Delete(AN);
            }
            Process(PD, RZ, AN, separators);
        }

        static bool Contains(char[] array, char character)
        {
            foreach (char item in array)
            {
                if (item == character)
                    return true;
            }
            return false;
        }

        static string FindCharacters(string line, string sep)
        {
            StringBuilder result = new StringBuilder();
            char[] separators = sep.ToCharArray();
            string[] words = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                bool[] isDuplicate = new bool[256];
                foreach (char character in word)
                {
                    int index = (int)character;
                    if (!Contains(separators, character))
                    {
                        if (!isDuplicate[index])
                            isDuplicate[index] = true;
                        else
                        {
                            bool containsCharacter = false;

                            for (int i = 0; i < result.Length; i++)
                            {
                                if (result[i] == character)
                                {
                                    containsCharacter = true;
                                    break;
                                }
                            }
                            if (!containsCharacter)
                                result.Append(character);
                        }
                    }
                }
            }
            return result.ToString();
        }

        static string ShortWord(string line, string sep, out int shortestWordIndex, out string shortestWordSeparators)
        {
            char[] separators = sep.ToCharArray();
            string[] words = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string shortestWord = "";
            shortestWordIndex = -1;
            int shortestLength = int.MaxValue;

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length < shortestLength)
                {
                    shortestWord = words[i];
                    shortestWordIndex = i;
                    shortestLength = words[i].Length;
                }
            }

            shortestWordSeparators = "";
            int len = 0;
            int start = 0;
            WordsWithSep(line, sep, shortestWordIndex, ref start, ref len);
            shortestWordSeparators = line.Substring(start, len);

            return shortestWord;
        }

        static void WordsWithSep(string line, string sep, int nr,
            ref int start, ref int len)
        {
            len = -1;
            int s = 0, start1 = -1;
            start = 0;
            char[] separators = sep.ToCharArray();
            string[] words = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i <= nr; i++)
            {
                start = line.IndexOf(words[i], s);
                s = start + words[i].Length;
            }
            if (nr != words.Length - 1)
            {
                start1 = line.IndexOf(words[nr + 1], s);
                len = start1 - start;
            }
            else
                len = line.Length - start;
        }

        static void RemoveWithSep(ref string line, string sep, int len)
        {
            char[] separators = sep.ToCharArray();
            string[] words = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length > 0)
            {
                int i = 0;
                foreach (string word in words)
                {
                    if (word.Length == len)
                    {
                        int Len = 0;
                        int fstart = 0;
                        WordsWithSep(line, sep, i, ref fstart, ref Len);
                        line = line.Remove(fstart, Len);
                        i--;
                    }
                    i++;
                }
            }
        }

        static void Process(string fvd, string fvr, string fva, string sep)
        {
            string[] lines = File.ReadAllLines(fvd, Encoding.UTF8);

            using (var fr = File.AppendText(fvr))
            {
                using (var fa = File.AppendText(fva))
                {
                    fa.WriteLine("| Line number | Repeated characters in words | Shortest word with his separators |");
                    fa.WriteLine("|---------------------------------------------------------------------------|");

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string characters = FindCharacters(lines[i], sep);
                        string newLine = lines[i];
                        int shortestWordIndex;
                        string shortestWordSeparators;
                        string shortestWord = ShortWord(lines[i], sep, out shortestWordIndex, out shortestWordSeparators);

                        if (!string.IsNullOrEmpty(shortestWord) && characters.Length > 0)
                        {
                            bool isShortestRemoved = false;

                            foreach (char character in characters)
                            {
                                fa.WriteLine("|{0,13:d}|{1,-30}|{2,-40}|", i + 1, character, shortestWordSeparators);

                                // Check if the current character is in the shortest word
                                if (shortestWord.Contains(character))
                                {
                                    RemoveWithSep(ref newLine, sep, shortestWord.Length);
                                    isShortestRemoved = true;
                                    break; // Stop processing if the word is removed
                                }
                            }

                            // If the shortest word is not removed, check for other words with the same length and character
                            if (!isShortestRemoved)
                            {
                                for (int j = 0; j < lines[i].Length; j++)
                                {
                                    int otherWordIndex;
                                    string otherWordSeparators;
                                    string otherWord = ShortWord(lines[i], sep, out otherWordIndex, out otherWordSeparators);

                                    if (otherWord.Length == shortestWord.Length && otherWordIndex != shortestWordIndex && otherWord.Contains(characters[0]))
                                    {
                                        fa.WriteLine("|{0,13:d}|{1,-30}|{2,-40}|", i + 1, characters[0], otherWordSeparators);
                                        RemoveWithSep(ref newLine, sep, otherWord.Length);
                                    }
                                }
                            }
                        }
                        else
                        {
                            fa.WriteLine("|{0,13:d}|{1,-30}|{2,-40}|", i + 1, "NO CHARACTERS", "NO SHORTEST WORD");
                        }

                        fr.WriteLine(newLine);
                    }

                    fa.WriteLine("|---------------------------------------------------------------------------|");
                }
            }
        }
    }
}