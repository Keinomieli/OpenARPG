#if TOOLS
using Godot;

namespace OpenARPG.Plugins
{
    [Tool]
    public partial class OpenARPG_InspectorPlugin : EditorPlugin
    {
        private InspectorPlugin _plugin;

        public override void _EnterTree()
        {
            _plugin = new InspectorPlugin();
            AddInspectorPlugin(_plugin);
        }

        public override void _ExitTree()
        {
            RemoveInspectorPlugin(_plugin);
        }
    }
}
#endif