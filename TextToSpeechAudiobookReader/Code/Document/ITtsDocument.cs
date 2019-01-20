using Miktemk.Models;
using Miktemk.TextToSpeech.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeechAudiobookReader.Code.Document
{
    public interface ITtsDocument
    {
        string Text { get; }
        IMultiLangString MultiLangText { get; }
        List<WordHighlight> HighlightRegions { get; }

        // settable
        string Lang { get; set; }
        bool OneLanguage { get; }
    }

    public abstract class TtsDocument : ITtsDocument
    {
        public string Text { get; protected set; }
        public IMultiLangString MultiLangText { get; protected set; }
        public List<WordHighlight> HighlightRegions { get; } = new List<WordHighlight>();

        public abstract string Lang { get; set; }
        public abstract bool OneLanguage { get; }
    }

}
