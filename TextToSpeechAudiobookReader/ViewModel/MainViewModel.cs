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
using TextToSpeechAudiobookReader.Code.Document;
using System.Collections.Generic;
using Miktemk.PropertyChanged;
using System.Windows.Media;

namespace TextToSpeechAudiobookReader.ViewModel
{
    public class MainViewModel : MainViewModelTts
    {
        private IMyRegistryService myRegistryService;
        private TtsDocumentFactory docFactory;
        private DocumentReaderByParagraph docReader;
        private int selectionStartOnFirstClick;

        public string CurFilename { get; private set; }
        public ITtsDocument CurDocument { get; private set; }
        public bool SearchPanelVisible { get; private set; }
        public bool SearchTextInFocus { get; private set; }
        public string SearchText { get; set; }

        // for avalon
        public TextDocument CodeDocument { get; } = new TextDocument();
        public IEnumerable<WordHighlight> EmphasizedWords { get; private set; }
        public WordHighlight Highlight { get; private set; }

        public ICommand SelectionFirstClickCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand ShowSearchPanelCommand { get; }
        public ICommand HideSearchPanelCommand { get; }

        public MainViewModel(
            TtsDocumentFactory docFactory,
            ITtsService ttsService,
            IMyRegistryService registryService,
            IDialogService dialogService)
            : base(ttsService, registryService, dialogService)
        {
            // .... services
            this.docFactory = docFactory;
            this.myRegistryService = registryService;

            // .... commands
            SelectionFirstClickCommand = new RelayCommand<int>(OnSelectionFirstClick);
            SelectionChangedCommand = new RelayCommand<int>(OnSelectionChanged);
            ShowSearchPanelCommand = new RelayCommand(() =>
            {
                SearchPanelVisible = true;
                SearchTextInFocus = true;
            });
            HideSearchPanelCommand = new RelayCommand(() => { SearchPanelVisible = false; });

            // .... logic
            docReader = new DocumentReaderByParagraph(ttsService);
            docReader.SelectWordPlease += DocReader_WordRead;
            ProgressTickFrequency = 10;
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
            // .... no file was previously selected or has been deleted
            if (filename == null || !File.Exists(filename))
            {
                CodeDocument.Text = "";
                docReader.SetDocument(null, null);
                DisEnable();
                return;
            }

            // .... save previous document's state before closing it
            if (CurFilename != null)
            {
                myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
            }

            // .... load file and its state
            CurFilename = filename;
            var ttsDocument = docFactory.LoadFromFile(filename);

            //var allText = File.ReadAllText(filename);
            //string header;
            //string docText;
            //Utils.SplitTtsarHeader(allText, out header, out docText);

            var docState = myRegistryService.GetDocumentState(filename);
            if (docState == null)
            {
                // .... never opened before? detect language, please
                docState = new DocumentState(0)
                {
                    LangCode = ttsDocument.Lang,
                    TtsSpeed = ttsService.GetDefaultSpeedForLang(ttsDocument.Lang),
                };
            }

            // .... set up doc reader
            docReader.SetDocument(ttsDocument, docState);

            // .... setup UI
            CodeDocument.Text = ttsDocument.Text;
            LangCode = docState.LangCode; // NOTE: lang code needs to be set before TtsSpeed because TtsSpeed will then use LangCode in case of cyrillica document
            TtsSpeed = docState.TtsSpeed;
            EmphasizedWords = ttsDocument.HighlightRegions;

            //TODO: docReader.SetDocument(ttsDocument.MultiLangText, docState);
            ProgressTotal = GetProgressValue(ttsDocument.Text.Length);

            // .... finish setting up UI
            DisEnable();

            docReader.Init();
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

        protected override bool CanPlayPause(PlayPauseButton.PlayState arg)
        {
            return docReader.DocumentState != null;
        }

        private void OnSelectionFirstClick(int selectionStart)
        {
            selectionStartOnFirstClick = selectionStart;
        }
        private void OnSelectionChanged(int selectionStart)
        {
            if (PlayButtonState == PlayPauseButton.PlayState.Playing)
            {
                PlayButtonState = PlayPauseButton.PlayState.Idle;
                ttsService.StopCurrentSynthAndCallMeWhenPlayable(() => {
                    docReader.Goto(selectionStartOnFirstClick);
                    docReader.Play();
                    PlayButtonState = PlayPauseButton.PlayState.Playing;
                });
            }
            else
            {
                docReader.Goto(selectionStartOnFirstClick);
                docReader.Play();
                PlayButtonState = PlayPauseButton.PlayState.Playing;
            }
        }

        protected override void OnTtsSpeedChanged()
        {
            //base.OnTtsSpeedChanged(); // NOTE: we do not wish to call base class, since we do not want to save in registry
            if (docReader != null)
            {
                if (docReader.DocumentState != null)
                {
                    // set newly selected speed in document state and save in reg
                    docReader.SetTtsRate(TtsSpeed);
                    myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
                }
            }
        }

        protected override void OnLangCodeChanged()
        {
            docReader.Stop();
            if (docReader.DocumentState != null)
            {
                // set newly selected language in document state and save in reg
                docReader.SetLangCode(LangCode);
                myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
            }
        }

        protected override void OnProgressChangedManually(double newProgress)
        {
            var position = GetPositionFromProgress(newProgress);
            if (PlayButtonState == PlayPauseButton.PlayState.Playing)
            {
                PlayButtonState = PlayPauseButton.PlayState.Idle;
                ttsService.StopCurrentSynthAndCallMeWhenPlayable(() => {
                    docReader.Goto(position);
                    docReader.Play();
                    PlayButtonState = PlayPauseButton.PlayState.Playing;
                });
            }
            else
            {
                docReader.Goto(position);
            }
        }

        #endregion

        #region ---------------------------- Contextual Search --------------------------------

        private void OnSearchPanelVisibleChanged()
        {
            if (!SearchPanelVisible)
                docReader.ClearSearch();
        }

        private void OnSearchTextChanged()
        {
            docReader.SetSearchString(SearchText);
        }

        #endregion

        #region ---------------------------- DocReader --------------------------------

        private void DocReader_WordRead(WordHighlight word)
        {
            Highlight = word?.MakeCopy();
            if (word != null)
                Progress = GetProgressValue(word.StartIndex);
        }

        #endregion

        #region ---------------------------- helpers --------------------------------

        private void SaveDocumentState()
        {
            myRegistryService.SetDocumentState(CurFilename, docReader.DocumentState);
        }

        private double GetProgressValue(int position)
        {
            return position / Globals.CharsPerProgressFactor;
        }
        private int GetPositionFromProgress(double progress)
        {
            return (int)(progress * Globals.CharsPerProgressFactor);
        }

        #endregion

    }
}