using Digger.Modules.Core.Editor;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Digger.Modules.PolyTerrainsIntegration.Editor
{
    [InitializeOnLoad]
    public class DiggerPolyTerrainsIntegrationDefines
    {
        private const string DiggerPolyTerrainsIntegrationDefine = "__DIGGER_POLYTERRAINS__";

        static DiggerPolyTerrainsIntegrationDefines()
        {
            DiggerDefines.InitDefine(DiggerPolyTerrainsIntegrationDefine);
        }

        [PostProcessScene(0)]
        public static void OnPostprocessScene()
        {
            DiggerDefines.InitDefine(DiggerPolyTerrainsIntegrationDefine);
        }
    }
}