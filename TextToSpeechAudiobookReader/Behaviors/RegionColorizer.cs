﻿using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TextToSpeechAudiobookReader.Code;

namespace TextToSpeechAudiobookReader.Behaviors
{
    public class RegionColorizer : DocumentColorizingTransformer
    {
        private IEnumerable<WordHighlight> regionsToColor;

        public RegionColorizer(IEnumerable<WordHighlight> regionsToColor)
        {
            this.regionsToColor = regionsToColor;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            var offset = line.Offset;
            var endOffset = line.EndOffset;
            var regionsThatApply = regionsToColor.Where(x => offset <= x.StartIndex && x.StartIndex <= endOffset);
            foreach (var region in regionsThatApply)
            {
                var start = Math.Max(region.StartIndex, offset);
                var end = Math.Min(region.StartIndex + region.Length, endOffset);
                ChangeLinePart(start, end, ApplyChanges);
            }
        }

        void ApplyChanges(VisualLineElement element)
        {
            // This is where you do anything with the line
            element.TextRunProperties.SetForegroundBrush(Globals.CyrillicaEmphasisBrush);
        }
    }
}