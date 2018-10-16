using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TextToSpeechAudiobookReader.Code
{
    public static class Globals
    {
        public const string RegTtsSpeed = "TtsSpeed";
        public const string RegLastOpenFilePath = "LastOpenFilePath";
        public static readonly string RegKeyDocumentStates = $@"{Miktemk.Wpf.Properties.Settings.Default.RegRoot}\DocumentStates";
        public const double CharsPerProgressFactor = 1000;
        public static readonly Brush CyrillicaEmphasisBrush = Brushes.DeepSkyBlue;
    }
}
