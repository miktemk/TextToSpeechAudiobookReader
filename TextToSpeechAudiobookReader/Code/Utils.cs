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

            if (position < 0 || position >= text.Length)
            {
                paraLength = 0;
                return;
            }

            // go back
            var i = position;
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
            i = position;
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
