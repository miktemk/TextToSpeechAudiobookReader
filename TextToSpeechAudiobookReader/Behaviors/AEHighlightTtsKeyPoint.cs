using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using TextToSpeechAudiobookReader.Code;

namespace TextToSpeechAudiobookReader.Behaviors
{
    // TODO: cleanup
    public class AEHighlightWord : DocumentColorizingTransformer
    {
        public WordHighlight Word { get; set; }

        public AEHighlightWord()
        {
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (Word == null)
                return;

            base.ChangeLinePart(
                Word.StartIndex, // startOffset
                Word.StartIndex + Word.Length, // endOffset
                (VisualLineElement element) => {
                    element.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(Colors.CadetBlue));
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Colors.White));
                });
        }
    }
}
