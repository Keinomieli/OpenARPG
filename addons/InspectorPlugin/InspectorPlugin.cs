#if TOOLS
using System;
using System.Reflection;
using Godot;

namespace OpenARPG.Plugins
{
    public partial class InspectorPlugin : EditorInspectorPlugin
    {
        //note that classes need to be declared as [Tool] in order to be correctly inherited in editor
        public override void _ParseBegin(GodotObject @object)
        {
            Type type = @object.GetType();

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object[] inspectorButtonAttributes = method.GetCustomAttributes(typeof(InspectorButtonAttribute), false);

                if (inspectorButtonAttributes.Length > 0)
                {
                    InspectorButtonAttribute attribute = inspectorButtonAttributes[0] as InspectorButtonAttribute;
                    var inspectorButton = new InspectorButtonEditor();
                    inspectorButton.SetButtonText(attribute.visibleName);
                    inspectorButton.SetButtonAction(() => method.Invoke(@object, null));
                    AddCustomControl(inspectorButton);
                }
            }

            //foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                //foreach (Type type in assembly.GetTypes())
                    //foreach (MethodInfo method in type.GetMethods())
                        //if (method.GetCustomAttributes(typeof(InspectorButtonAttribute), false).Length > 0)
                        {
                            /*ParameterInfo[] parameters = method.GetParameters();

                            if (parameters.Length > 0)
                            {
                                Logger.Log($"[Command] {type}.{method.Name}");

                                foreach(ParameterInfo parameter in parameters)
                                {
                                    Logger.Log($"-param: {parameter.ParameterType}");

                                    if (parameter.ParameterType == typeof(int))
                                    {
                                        method.Invoke(parameter, new object[] { 42069 });
                                    }
                                }
                            }
                            else
                            {
                                Logger.Log($"[Command] {type}.{method.Name}");
                                method.Invoke(null,null);
                            }*/
                        }
        }

        public override bool _CanHandle(GodotObject @object)
        {
            return true;
        }

        public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
        {
            /*
            // We handle properties of type integer.
            if (type == Variant.Type.Int)
            {
                // Create an instance of the custom property editor and register
                // it to a specific property path.
                AddPropertyEditor(name, new RandomIntEditor());
                // Inform the editor to remove the default property editor for
                // this property type.
                return true;
            }*/

            return false;
        }
    }
}
#endif