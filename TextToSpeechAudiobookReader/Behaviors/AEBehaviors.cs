using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TextToSpeechAudiobookReader.Code;

namespace TextToSpeechAudiobookReader.Behaviors
{
    public static class AEBehaviors
    {
        #region ---------------------- HighlightWord ---------------------------

        public static WordHighlight GetHighlightWord(DependencyObject obj)
        {
            return (WordHighlight)obj.GetValue(HighlightWordProperty);
        }

        public static void SetHighlightWord(DependencyObject obj, WordHighlight value)
        {
            obj.SetValue(HighlightWordProperty, value);
        }

        // Using a DependencyProperty as the backing store for HighlightWord.  This enables animation, styling, binding, etc…
        public static readonly DependencyProperty HighlightWordProperty =
            DependencyProperty.RegisterAttached("HighlightWord", typeof(WordHighlight), typeof(AEBehaviors), new PropertyMetadata(OnHighlightWordChanged));

        private static void OnHighlightWordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextEditor tEdit = (TextEditor)d;
            tEdit.TextArea.TextView.LineTransformers.Clear(); //TODO: persist old shit but faded???

            var word = GetHighlightWord(d);
            if (word != null)
            {
                tEdit.Select(word.StartIndex, word.Length);
                DocumentLine line = tEdit.Document.GetLineByOffset(tEdit.CaretOffset);
                tEdit.ScrollToLine(line.LineNumber);
            }
        }

        #endregion

        #region ---------------------- SelectionChanged ---------------------------

        public static ICommand GetSelectionChanged(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(SelectionChangedProperty);
        }

        public static void SetSelectionChanged(DependencyObject obj, ICommand value)
        {
            obj.SetValue(SelectionChangedProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectionChanged.  This enables animation, styling, binding, etc…
        public static readonly DependencyProperty SelectionChangedProperty =
            DependencyProperty.RegisterAttached("SelectionChanged", typeof(ICommand), typeof(AEBehaviors), new PropertyMetadata(OnSelectionChangedChanged));

        private static void OnSelectionChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextEditor tEdit = d as TextEditor;
            ICommand command = e.NewValue as ICommand;
            tEdit.TextArea.SelectionChanged += (object sender, EventArgs args) => {
                // TODO: consider passing more parameters
                command.Execute(tEdit.SelectionStart);
            };
        }

        #endregion

        #region ---------------------- WordDoubleClicked ---------------------------

        public static ICommand GetWordDoubleClicked(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(WordDoubleClickedProperty);
        }

        public static void SetWordDoubleClicked(DependencyObject obj, ICommand value)
        {
            obj.SetValue(WordDoubleClickedProperty, value);
        }

        // Using a DependencyProperty as the backing store for WordDoubleClicked.  This enables animation, styling, binding, etc…
        public static readonly DependencyProperty WordDoubleClickedProperty =
            DependencyProperty.RegisterAttached("WordDoubleClicked", typeof(ICommand), typeof(AEBehaviors), new PropertyMetadata(OnWordDoubleClickedChanged));

        private static void OnWordDoubleClickedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextEditor tEdit = d as TextEditor;
            ICommand command = e.NewValue as ICommand;
            tEdit.TextArea.MouseDoubleClick += (object sender, MouseButtonEventArgs args) => {
                command.Execute(tEdit.SelectionStart);
            };
        }

        #endregion

        #region --------------------------- ScrollRow ---------------------------

        public static int GetScrollRow(DependencyObject obj)
        {
            return (int)obj.GetValue(ScrollRowProperty);
        }

        public static void SetScrollRow(DependencyObject obj, int value)
        {
            obj.SetValue(ScrollRowProperty, value);
        }

        // Using a DependencyProperty as the backing store for ScrollRow.  This enables animation, styling, binding, etc…
        public static readonly DependencyProperty ScrollRowProperty =
            DependencyProperty.RegisterAttached("ScrollRow", typeof(int), typeof(AEBehaviors), new PropertyMetadata(OnScrollRowChanged));

        private static void OnScrollRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tEdit = d as TextEditor;
            var scrollRow = GetScrollRow(d);
            tEdit.ScrollToLine(scrollRow);
        }

        private static void TextView_ScrollOffsetChanged(object sender, EventArgs e)
        {
            var textView = sender as TextView;
        }

        #endregion

        #region ---------------------------------- ScrollRowTwoWay -------------------------------

        public static bool GetScrollRowTwoWay(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollRowTwoWayProperty);
        }

        public static void SetScrollRowTwoWay(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollRowTwoWayProperty, value);
        }

        // Using a DependencyProperty as the backing store for ScrollRowTwoWay.  This enables animation, styling, binding, etc…
        public static readonly DependencyProperty ScrollRowTwoWayProperty =
            DependencyProperty.RegisterAttached("ScrollRowTwoWay", typeof(bool), typeof(AEBehaviors), new PropertyMetadata(OnScrollRowTwoWayChanged));

        private static void OnScrollRowTwoWayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tEdit = d as TextEditor;
            var scrollRow = GetScrollRow(d);

            TextView textView = tEdit.TextArea.TextView;
            textView.ScrollOffsetChanged += (object sender, EventArgs args) => {
                // This is actual top visible line of current TextView ((e.g. line130) 
                int firstLine = tEdit.TextArea.TextView.GetDocumentLineByVisualTop(tEdit.TextArea.TextView.ScrollOffset.Y).LineNumber;
                SetScrollRow(d, firstLine);
            };
        }

        #endregion
    }
}
