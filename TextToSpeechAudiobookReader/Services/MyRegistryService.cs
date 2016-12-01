using Miktemk;
using Miktemk.Wpf.Properties;
using Miktemk.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextToSpeechAudiobookReader.Code;

namespace TextToSpeechAudiobookReader.Services
{
    public interface IMyRegistryService : IRegistryService
    {
        DocumentState GetDocumentState(string filename);
        void SetDocumentState(string filename, DocumentState state);
    }
    public class MyRegistryService : RegistryService, IMyRegistryService
    {

        public DocumentState GetDocumentState(string filename)
        {
            return UtilsRegistry
                .OpenUserSoftwareKey(Globals.RegKeyDocumentStates)
                .LoadRegistryJson<DocumentState>(filename);
        }

        public void SetDocumentState(string filename, DocumentState state)
        {
            UtilsRegistry
                .OpenUserSoftwareKey(Globals.RegKeyDocumentStates)
                .SaveRegistryJson<DocumentState>(filename, state);
        }
    }
}
