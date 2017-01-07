
using Miktemk.TextToSpeech.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeechAudiobookReader.Code.Document
{
    public class TtsDocumentCyrillica : TtsDocument
    {
        internal TtsDocumentCyrillica(string docText, MultiLangStringRegioned multiLangText, string foreignLangCode,
            bool highlightWordsOnly = true, bool invert = false)
        {
            Text = docText;
            MultiLangText = multiLangText;
            this.Lang = foreignLangCode;

            if (highlightWordsOnly)
            {
                if (invert)
                    HighlightRegions.AddRange(Utils.FindAllCyrillicWords(docText));
                else
                    HighlightRegions.AddRange(Utils.FindAllLatinWords(docText));
            }
            else
            {
                // NOTE: the following can be used for debugging purposes e.g. to check TTSer
                foreach (var region in multiLangText.Regions)
                {
                    var isRu = region.Lang == TtsGlobals.LangCodeRussian;
                    if (isRu == invert)
                        HighlightRegions.Add(new WordHighlight(region.Start, region.Length));
                }
            }
        }

        public override string Lang { get; set; }
        public override bool OneLanguage => false;
    }
}
