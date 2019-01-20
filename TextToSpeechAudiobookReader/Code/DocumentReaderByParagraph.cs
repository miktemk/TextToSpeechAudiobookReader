using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miktemk.TextToSpeech.Services;
using Miktemk.TextToSpeech.Core;
using System.Diagnostics;
using TextToSpeechAudiobookReader.Code.Document;
using Miktemk.Models;

namespace TextToSpeechAudiobookReader.Code
{
    public class DocumentReaderByParagraph
    {
        public delegate void OnWordReadHandler(WordHighlight word);
        public event OnWordReadHandler SelectWordPlease;

        public delegate void OnDocumentFinishedHandler();
        public event OnDocumentFinishedHandler OnDocumentFinished;

        private ITtsService ttsService;
        //private int startedReadingFromHere;
        private ContextualSearchState searching;
        private ITtsDocument ttsDocument;

        public DocumentState DocumentState { get; private set; }

        public DocumentReaderByParagraph(ITtsService ttsService)
        {
            this.ttsService = ttsService;
            ttsService.AddWordCallback(ttsService_Word);
            searching = new ContextualSearchState();
        }

        public void SetDocument(TtsDocument ttsDocument, DocumentState state)
        {
            this.ttsDocument = ttsDocument;
            DocumentState = state;
        }

        // .... NOTE: this function is separated from SetDocument because UI needs to be updated first before we fire any selection events
        public void Init()
        {
            if (DocumentState != null)
                SelectWordPlease?.Invoke(DocumentState.Word);
        }

        public void Play()
        {
            if (DocumentState == null)
                return;
            ttsService.SayAsyncMany(ttsDocument.MultiLangText, (phrase, id) => { },
                startingChar: DocumentState.Word.StartIndex);

            //startedReadingFromHere = DocumentState.Word.StartIndex;
            //if (ttsDocument.OneLanguage)
            //{
            //    ttsService.SetSpeedForLanguage(DocumentState.LangCode, DocumentState.TtsSpeed);
            //}
            //var textStartingFromHere = allText.Substring(startedReadingFromHere);
            //ttsService.SayAsync(DocumentState.LangCode, textStartingFromHere, ttsService_DonePlaying);
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
                text: ttsDocument.Text,
                position: position,
                paraStart: out wStart,
                paraLength: out wLength);
            var subword = ttsDocument.Text.Substring(wStart, wLength);
            DocumentState.Word.StartIndex = wStart;
            DocumentState.Word.Length = wLength;
            SelectWordPlease?.Invoke(DocumentState.Word);
        }

        private void ttsService_Word(string text, int offset, int start, int length)
        {
            if (DocumentState == null)
                return;
            if (!ttsService.IsPlaying)
                return;
            DocumentState.Word.StartIndex = offset + start; // + startedReadingFromHere;
            DocumentState.Word.Length = length;
            if (!searching.IsSearchInProgress)
                SelectWordPlease?.Invoke(DocumentState.Word);
        }

        public void SetSearchString(string searchText)
        {
            searching.IsSearchInProgress = true;
            var index = ttsDocument.Text.IndexOf(searchText, searching.LastSearchIndex, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
            {
                SelectWordPlease?.Invoke(null);
            }
            else
            {
                //searching.LastSearchIndex = index + searchText.Length;
                searching.SearchHighlight = new WordHighlight(index, searchText.Length);
                SelectWordPlease?.Invoke(searching.SearchHighlight);
            }
        }

        public void FindNext()
        {

        }

        public void ClearSearch()
        {
            searching.IsSearchInProgress = false;
            SelectWordPlease?.Invoke(null);
        }

        public void SetLangCode(string langCode)
        {
            DocumentState.LangCode = langCode;
            ttsDocument.Lang = langCode;
        }

        public void SetTtsRate(int rate)
        {
            DocumentState.TtsSpeed = rate;
            if (ttsDocument.OneLanguage)
                ttsService.SetVoiceOverrideSpeed(rate);
            else
            {
                ttsService.SetVoiceOverrideSpeed(null);
                ttsService.SetSpeedForLanguage(DocumentState.LangCode, rate);
            }
        }
    }
}
