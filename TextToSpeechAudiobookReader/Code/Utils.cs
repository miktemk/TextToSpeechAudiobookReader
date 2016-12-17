using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
