using Miktemk.TextToSpeech.Core;
using Miktemk.TextToSpeech.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextToSpeechAudiobookReader.Code.Document
{
    public class TtsDocumentFactory
    {
        private ITtsService ttsService;

        public TtsDocumentFactory(ITtsService ttsService)
        {
            this.ttsService = ttsService;
        }

        public TtsDocument LoadFromFile(string filename)
        {
            var allText = File.ReadAllText(filename);
            string header;
            string docText;
            Utils.SplitTtsarHeader(allText, out header, out docText);

            if (header != null)
            {
                var regex = new Regex(@"cyrillica: (.*)", RegexOptions.IgnoreCase);
                var moHeader = regex.Match(header);
                if (moHeader.Success)
                {
                    var latinaLangCode = moHeader.Groups[1].Value;
                    var multiLangText = Utils.ParseCyrillicaText(docText, latinaLangCode);
                    return new TtsDocumentCyrillica(docText, multiLangText, latinaLangCode);
                }

                regex = new Regex(@"cyrillica-invert: (.*)", RegexOptions.IgnoreCase);
                moHeader = regex.Match(header);
                if (moHeader.Success)
                {
                    var latinaLangCode = moHeader.Groups[1].Value;
                    var multiLangText = Utils.ParseCyrillicaText(docText, latinaLangCode);
                    return new TtsDocumentCyrillica(docText, multiLangText, latinaLangCode,
                        invert: true,
                        highlightWordsOnly: true);
                }

                
            }

            // .... else it is plain

            var qLang = UtilsTts.WhatLanguage(docText);
            var langCode = ttsService.GetLangCodeForQLanguage(qLang);
            return new TtsDocumentPlain(docText, langCode);
        }
    }
}
