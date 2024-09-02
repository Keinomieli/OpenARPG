#if TOOLS
using Godot;

namespace Keinomieli
{
    [Tool]
    public partial class InspectorButtonEditorPlugin : EditorPlugin
    {
        private InspectorButtonPlugin _plugin;

        public override void _EnterTree()
        {
            _plugin = new InspectorButtonPlugin();
            AddInspectorPlugin(_plugin);
        }

        public override void _ExitTree()
        {
            RemoveInspectorPlugin(_plugin);
        }
    }
}
#endif