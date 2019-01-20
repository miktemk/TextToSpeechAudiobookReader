using Miktemk.Models;
using Miktemk.TextToSpeech.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextToSpeechAudiobookReader.Code
{
    public static class Utils
    {
        public static void FindParagraph(string text, int position, out int paraStart, out int paraLength)
        {
            paraStart = 0;
            paraLength = text.Length;

            if (position < 0 || position >= text.Length)
            {
                paraLength = 0;
                return;
            }
            
            // go back
            var i = position;
            while (i > 0)
            {
                if (text[i] == '\n') // NOTE: sequence is \r\n so you will never encounter \r on the way back
                {
                    paraStart = i + 1;
                    break;
                }
                i--;
            }

            // go forward
            i = position;
            while (i < text.Length)
            {
                if (text[i] == '\n' || text[i] == '\r')
                    break;
                i++;
            }
            paraLength = i - paraStart;
        }

        public static void FindWord(string text, int position, out int paraStart, out int paraLength)
        {
            paraStart = 0;
            paraLength = text.Length;

            if (position < 0)
            {
                paraLength = 0;
                return;
            }

            if (position >= text.Length)
            {
                // last word
                position = text.Length - 1;
            }

            // make sure we are not on whitespace
            var noWsCharPosition = position;
            while (noWsCharPosition >= 0 && char.IsWhiteSpace(text[noWsCharPosition]))
                noWsCharPosition--;

            // no chars above? search below
            if (noWsCharPosition == -1)
            {
                noWsCharPosition = position;
                while (noWsCharPosition < text.Length && char.IsWhiteSpace(text[noWsCharPosition]))
                    noWsCharPosition++;
            }

            if (noWsCharPosition < 0 || noWsCharPosition > text.Length - 1)
            {
                paraStart = -1;
                paraLength = 0;
                return;
            }

            // go back
            var i = noWsCharPosition;
            while (i > 0)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    paraStart = i + 1;
                    break;
                }
                i--;
            }

            // go forward
            i = noWsCharPosition;
            while (i < text.Length)
            {
                if (char.IsWhiteSpace(text[i]))
                    break;
                i++;
            }
            paraLength = i - paraStart;
        }

        public static void SplitTtsarHeader(string text, out string header, out string body)
        {
            // .... https://regex101.com/r/7wG3ME/3
            var regex = new Regex(@"^<!--\s*(.*?)\s*-->\s*(.*)$", RegexOptions.Singleline);
            var mo = regex.Match(text);
            if (!mo.Success)
            {
                header = null;
                body = text;
                return;
            }
            header = mo.Groups[1].Value;
            body = mo.Groups[2].Value;
        }

        private static bool IsCyrillic(this char c)
        {
            //[1040..1103], 1025, 1105
            // TODO: accented cyrillic chars
            return (c >= 1040 && c <= 1103)
                || c == 1025
                || c == 1105;
        }
        private static bool IsLatina(this char c)
        {
            return Char.IsLetter(c) && !c.IsCyrillic();
        }

        public static int[] FindAllNonCyrillics(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return new int[0];

            var result = new List<int>();
            var inLatina = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (!inLatina && s[i].IsLatina())
                {
                    inLatina = true;
                    result.Add(i);
                }
                if (inLatina && s[i].IsCyrillic())
                {
                    inLatina = false;
                    if (result.Count > 0) // NOTE: we want the first index added to be latina
                        result.Add(i);
                }
            }
            // NOTE: odd # of elements means we ended on Latina
            if (result.Count % 2 == 1 && !s[s.Length - 1].IsLatina())
                result.Add(s.Length - 1);

            // NOTE: at this point every 2nd entry in array needs to be pushed back to last latin character

            var arrResult = result.ToArray();
            for (int i = 1; i < arrResult.Length; i += 2)
            {
                var position = arrResult[i];

                // NOTE: don't go past the preivous one, that would be just silly. Consider 1 latin char among cyrillic.
                while (position > arrResult[i - 1] && !s[position].IsLatina())
                    position--;
                // if nothing changed at all, move on
                if (position == arrResult[i])
                    continue;
                arrResult[i] = position + 1;
            }

            return arrResult;
        }

        public static IEnumerable<WordHighlight> FindAllLatinWords(string s)
        {
            return FindAllWordsByFunc(s, IsLatina);
        }

        public static IEnumerable<WordHighlight> FindAllCyrillicWords(string s)
        {
            return FindAllWordsByFunc(s, IsCyrillic);
        }

        private static IEnumerable<WordHighlight> FindAllWordsByFunc(string s, Func<char, bool> funcCriteria)
        {
            if (String.IsNullOrWhiteSpace(s))
                yield break;
            var inWord = false;
            var start = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (!inWord && funcCriteria(s[i]))
                {
                    inWord = true;
                    start = i;
                }
                if (inWord && !funcCriteria(s[i]))
                {
                    inWord = false;
                    yield return new WordHighlight(start, i - start);
                }
            }
            // tail
            if (inWord)
                yield return new WordHighlight(start, s.Length - start);
        }

        public static MultiLangStringRegioned ParseCyrillicaText(string docText, string latinaLangCode)
        {
            var result = new MultiLangStringRegioned(docText);
            var splitIndices = Utils.FindAllNonCyrillics(docText);
            var start = 0;
            for (int i = 0; i < splitIndices.Length; i++)
            {
                var end = splitIndices[i];
                result.AddRegion(start, end - start, (i % 2 == 0) ? TtsGlobals.LangCodeRussian : latinaLangCode);
                start = end;
            }
            if (start < docText.Length-1)
                result.AddRegion(start, docText.Length - 1 - start, (result.Count % 2 == 0) ? TtsGlobals.LangCodeRussian : latinaLangCode);
            return result;
        }
    }
}
