using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeechAudiobookReader.Code
{
    public class ContextualSearchState
    {
        public bool IsSearchInProgress { get; set; }
        public WordHighlight SearchHighlight { get; set; }
        public int LastSearchIndex { get; set; }
    }
}
