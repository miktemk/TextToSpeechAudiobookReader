using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miktemk.TextToSpeech.Services;
using Miktemk.TextToSpeech.Core;

namespace TextToSpeechAudiobookReader.Code
{
    public class DocumentReaderByParagraph
    {
        public delegate void WordReadHandler(WordHighlight word);
        public event WordReadHandler WordRead;

        public delegate void OnDocumentFinishedHandler();
        public event OnDocumentFinishedHandler OnDocumentFinished;

        private ITtsService ttsService;
        private string allText;
        private int startedReadingFromHere;

        public DocumentState DocumentState { get; private set; }

        public DocumentReaderByParagraph(ITtsService ttsService)
        {
            this.ttsService = ttsService;
            ttsService.AddWordCallback(ttsService_Word);
        }

        public void SetDocument(string allText, DocumentState state)
        {
            this.allText = allText;
            DocumentState = state;
            WordRead?.Invoke(state.Word);
        }

        public void Play()
        {
            startedReadingFromHere = DocumentState.Word.StartIndex;
            var textStartingFromHere = allText.Substring(startedReadingFromHere);

            ttsService.SetVoiceOverrideSpeed(DocumentState.TtsSpeed);
            ttsService.SayAsync(DocumentState.LangCode, textStartingFromHere, ttsService_DonePlaying);
        }

        private void ttsService_DonePlaying()
        {
            DocumentState.Word.StartIndex = 0;
            OnDocumentFinished?.Invoke();
        }

        public void Stop()
        {
            ttsService.StopCurrentSynth();
        }

        public void Goto(int position)
        {
            // TODO: search up in the string to find correct position
            DocumentState.Word.StartIndex = position;
        }

        private void ttsService_Word(string text, int start, int length)
        {
            DocumentState.Word.StartIndex = start + startedReadingFromHere;
            DocumentState.Word.Length = length;
            WordRead?.Invoke(DocumentState.Word);
        }
    }
}
