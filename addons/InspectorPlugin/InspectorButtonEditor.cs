#if TOOLS
using System;
using Godot;

namespace OpenARPG.Plugins
{
    public partial class InspectorButtonEditor : Button
    {
        private bool _updating = false;

        public InspectorButtonEditor()
        {
        }

        public void SetButtonText(string buttonText)
        {
            Text = buttonText;
        }

        public void SetButtonAction(Action buttonAction)
        {
            Pressed += buttonAction;
        }
    }
}
#endif