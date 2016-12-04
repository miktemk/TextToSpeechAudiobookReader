using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeechAudiobookReader.Code
{
    public class WordHighlight
    {
        public WordHighlight(int startChar, int length)
        {
            StartIndex = startChar;
            Length = length;
        }

        public int StartIndex { get; set; }
        public int Length { get; set; }

        public WordHighlight MakeCopy()
        {
            return new WordHighlight(StartIndex, Length);
        }
    }
}
