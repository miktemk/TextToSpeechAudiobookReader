using GalaSoft.MvvmLight;
using Miktemk.Wpf.ViewModels;
using System.Windows.Input;
using System;
using GalaSoft.MvvmLight.Command;
using Miktemk.Wpf.Controls;
using System.Linq;
using Miktemk.TextToSpeech.Wpf.ViewModels;
using Miktemk.TextToSpeech.Services;
using Miktemk.Wpf.Services;
using MvvmDialogs;
using TextToSpeechAudiobookReader.Services;
using TextToSpeechAudiobookReader.Code;
using System.IO;

namespace TextToSpeechAudiobookReader.ViewModel
{
    public class MainViewModel : MainViewModelTts
    {
        private IMyRegistryService myRegistryService;
        private DocumentReaderByParagraph docReader;

        public string CurFilename { get; private set; }
        public string CurParagraph { get; private set; }
        public int SelectionStart { get; private set; }
        public int SelectionLength { get; private set; }

        public MainViewModel(
            ITtsService ttsService,
            IMyRegistryService registryService,
            IDialogService dialogService)
            : base(ttsService, registryService, dialogService)
        {
            this.myRegistryService = registryService;

            docReader = new DocumentReaderByParagraph(ttsService);
            docReader.NextParagraph += DocReader_NextParagraph;
            docReader.WordInCurrentParagraph += DocReader_WordInCurrentParagraph;

            // TOOD: this is a hack. Please load file on Window.Load
            LoadFromArgOrLastOpenFile();
        }

        protected override void PlayPause(PlayPauseButton.PlayState obj)
        {
        }

        protected override void LoadFile(string filename)
        {
            if (CurFilename != null)
            {
                // save previous document's state before closing it
                myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
            }

            // load the new file
            CurFilename = filename;
            var allText = File.ReadAllText(filename);
            var docState = myRegistryService.GetDocumentState(filename)
                ?? new DocumentState { WordPosition = 0 };

            // set up doc reader
            docReader.SetDocument(allText, docState.WordPosition);
        }

        #region ---------------------------- DocReader --------------------------------

        private void DocReader_NextParagraph(string paragraph)
        {
            CurParagraph = paragraph;
        }

        private void DocReader_WordInCurrentParagraph(int startChar, int length)
        {
            SelectionStart = startChar;
            SelectionLength = length;
        }

        #endregion

    }
}