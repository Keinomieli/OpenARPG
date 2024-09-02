#if TOOLS
using System;
using System.Reflection;
using Godot;

namespace Keinomieli
{
    public partial class InspectorButtonPlugin : EditorInspectorPlugin
    {
        const BindingFlags binding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public override void _ParseBegin(GodotObject @object)
        {
            Type type = @object.GetType();

            foreach (MethodInfo method in type.GetMethods(binding))
            {
                Attribute attribute = method.GetCustomAttribute(typeof(InspectorButtonAttribute), false);

                if (attribute is InspectorButtonAttribute inspectorButtonAttribute)
                {
                    InspectorButtonEditor inspectorButton = new();
                    inspectorButton.SetButtonText(inspectorButtonAttribute.visibleName);
                    inspectorButton.SetButtonAction(() => method.Invoke(@object, null));
                    AddCustomControl(inspectorButton);
                }
            }
        }

        public override bool _CanHandle(GodotObject @object)
        {
            return true;
        }
    }
}
#endif