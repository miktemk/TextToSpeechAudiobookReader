using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miktemk.TextToSpeech.Services;

namespace TextToSpeechAudiobookReader.Code
{
    public class DocumentReaderByParagraph
    {
        public delegate void NextParagraphHandler(string paragraph);
        public event NextParagraphHandler NextParagraph;

        public delegate void WordInCurrentParagraphHandler(int startChar, int length);
        public event WordInCurrentParagraphHandler WordInCurrentParagraph;

        private ITtsService ttsService;

        public DocumentReaderByParagraph(ITtsService ttsService)
        {
            this.ttsService = ttsService;
            ttsService.AddWordCallback(ttsService_Word);
        }

        public void SetDocument(string allText, int wordPosition)
        {
            AllText = allText;
            DocumentState = new DocumentState { WordPosition = wordPosition };

            // TODO: invoke the UI updates
            NextParagraph?.Invoke("test paragraph on SetDocument");
            WordInCurrentParagraph?.Invoke(5, 8);
        }

        public string AllText { get; private set; }
        public DocumentState DocumentState { get; private set; }

        private void ttsService_Word(string text, int start, int length)
        {
            // TODO: finish DocumentReaderByParagraph
            //NextParagraph?.Invoke("blah");
            //WordInCurrentParagraph?.Invoke(1, 2);
        }
    }
}
