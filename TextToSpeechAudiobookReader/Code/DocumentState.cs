using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeechAudiobookReader.Code
{
    /// <summary>
    /// remembers the position
    /// </summary>
    public class DocumentState
    {
        public DocumentState() : this(0) { }
        public DocumentState(int start)
        {
            Word = new WordHighlight(start, 0);
        }

        public WordHighlight Word { get; set; }
        public string LangCode { get; set; }
        public int TtsSpeed { get; set; }
    }
}
