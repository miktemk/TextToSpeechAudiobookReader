using Miktemk.TextToSpeech.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeechAudiobookReader.Code.Document
{
    public class TtsDocumentPlain : TtsDocument
    {
        private MultiLangStringSingle mlstrSingle;

        internal TtsDocumentPlain(string docText, string langCode)
        {
            Text = docText;
            mlstrSingle = new MultiLangStringSingle(docText, langCode);
            MultiLangText = mlstrSingle;
        }

        public override string Lang
        {
            get { return mlstrSingle.Lang; }
            set { mlstrSingle.Lang = value; }
        }
        public override bool OneLanguage => true;
    }


}
