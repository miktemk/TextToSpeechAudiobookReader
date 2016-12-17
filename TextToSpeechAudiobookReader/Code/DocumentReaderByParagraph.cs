using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miktemk.TextToSpeech.Services;
using Miktemk.TextToSpeech.Core;
using System.Diagnostics;

namespace TextToSpeechAudiobookReader.Code
{
    public class DocumentReaderByParagraph
    {
        public delegate void OnWordReadHandler(WordHighlight word);
        public event OnWordReadHandler OnWordRead;

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
            if (DocumentState != null)
                OnWordRead?.Invoke(state.Word);
        }

        public void Play()
        {
            if (DocumentState == null)
                return;

            startedReadingFromHere = DocumentState.Word.StartIndex;
            var textStartingFromHere = allText.Substring(startedReadingFromHere);

            ttsService.SetVoiceOverrideSpeed(DocumentState.TtsSpeed);
            ttsService.SayAsync(DocumentState.LangCode, textStartingFromHere, ttsService_DonePlaying);
        }

        private void ttsService_DonePlaying()
        {
            if (DocumentState == null)
                return;

            DocumentState.Word.StartIndex = 0;
            OnDocumentFinished?.Invoke();
        }

        public void Stop()
        {
            ttsService.StopCurrentSynth();
        }

        public void Goto(int position)
        {
            if (DocumentState == null)
                return;

            // search up in the string to find correct start of the double-clicked word
            int wStart, wLength;
            Utils.FindWord(
                text: allText,
                position: position,
                paraStart: out wStart,
                paraLength: out wLength);
            var subword = allText.Substring(wStart, wLength);
            DocumentState.Word.StartIndex = wStart;
            DocumentState.Word.Length = wLength;
            OnWordRead?.Invoke(DocumentState.Word);
        }

        private void ttsService_Word(string text, int start, int length)
        {
            if (DocumentState == null)
                return;
            DocumentState.Word.StartIndex = start + startedReadingFromHere;
            DocumentState.Word.Length = length;
            OnWordRead?.Invoke(DocumentState.Word);
        }

    }
}
