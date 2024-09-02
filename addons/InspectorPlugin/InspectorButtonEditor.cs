#if TOOLS
using System;
using Godot;

namespace Keinomieli
{
    public partial class InspectorButtonEditor : Button
    {
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