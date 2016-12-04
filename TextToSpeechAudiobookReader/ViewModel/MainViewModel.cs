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
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System.Diagnostics;
using Miktemk.TextToSpeech.Core;

namespace TextToSpeechAudiobookReader.ViewModel
{
    public class MainViewModel : MainViewModelTts
    {
        private IMyRegistryService myRegistryService;
        private DocumentReaderByParagraph docReader;

        public string CurFilename { get; private set; }
        public TextDocument CodeDocument { get; } = new TextDocument();

        public WordHighlight Highlight { get; private set; }
        public ICommand SelectionChangedCommand { get; }

        public MainViewModel(
            ITtsService ttsService,
            IMyRegistryService registryService,
            IDialogService dialogService)
            : base(ttsService, registryService, dialogService)
        {
            // .... services
            this.myRegistryService = registryService;

            // .... commands
            SelectionChangedCommand = new RelayCommand<int>(OnSelectionChanged);

            // .... logic
            docReader = new DocumentReaderByParagraph(ttsService);
            docReader.WordRead += DocReader_WordRead;
        }

        protected override void PlayPause(PlayPauseButton.PlayState state)
        {
            if (state == PlayPauseButton.PlayState.Playing)
                docReader.Play();
            else
            {
                docReader.Stop();
                SaveDocumentState();
            }
        }

        protected override void LoadFile(string filename)
        {
            if (CurFilename != null)
            {
                // .... save previous document's state before closing it
                myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
            }

            // .... load file and its state
            CurFilename = filename;
            var allText = File.ReadAllText(filename);
            var docState = myRegistryService.GetDocumentState(filename);
            if (docState == null)
            {
                // .... never opened before? detect language, please
                var qLang = UtilsTts.WhatLanguage(allText);
                var langCode = ttsService.GetLangCodeForQLanguage(qLang);
                docState = new DocumentState(0)
                {
                    LangCode = langCode,
                    TtsSpeed = ttsService.GetDefaultSpeedForLang(langCode),
                };
            }

            // .... setup UI
            CodeDocument.Text = allText;
            TtsSpeed = docState.TtsSpeed;
            LangCode = docState.LangCode;

            // .... set up doc reader
            docReader.SetDocument(allText, docState);
        }

        #region ---------------------------- UI events  --------------------------------

        protected override void WindowLoaded()
        {
            LoadFromArgOrLastOpenFile();
        }

        protected override void WindowClosing()
        {
            docReader.Stop();
            SaveDocumentState();
        }

        private void OnSelectionChanged(int selectionStart)
        {
            docReader.Stop();
            docReader.Goto(selectionStart);
            docReader.Play();
        }

        protected override void OnTtsSpeedChanged()
        {
            //base.OnTtsSpeedChanged(); // NOTE: we do not wish to call base class, since we do not want to save in registry
            if (docReader != null)
            {
                //docReader.Stop(); // On the 2nd thought... don't stop playing
                if (docReader.DocumentState != null)
                {
                    // set newly selected speed in document state and save in reg
                    docReader.DocumentState.TtsSpeed = TtsSpeed;
                    myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
                }
            }
            ttsService.SetVoiceOverrideSpeed(TtsSpeed);
        }

        protected override void OnLangCodeChanged()
        {
            docReader.Stop();
            if (docReader.DocumentState != null)
            {
                // set newly selected language in document state and save in reg
                docReader.DocumentState.LangCode = LangCode;
                myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
            }
        }

        #endregion

        #region ---------------------------- DocReader --------------------------------

        private void DocReader_WordRead(WordHighlight word)
        {
            Highlight = word.MakeCopy();
        }

        #endregion

        #region ---------------------------- helpers --------------------------------

        private void SaveDocumentState()
        {
            myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
        }

        #endregion

    }
}