using Godot;
using System;
using System.Reflection;

namespace OpenARPG.System
{
    public partial class DevConsole : Control
    {
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("Toggle_DevConsole"))
            {
                Visible = !Visible;
            }
        }

        public static Action Initialize { get; private set; } = () => 
        {
            Initialize = () => { Logger.Error("DevConsole: Initialize: DevConsole already initialized"); };

            Logger.Log("DevConsole: Initialize: DevConsole initialized");

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in assembly.GetTypes())
                    foreach (MethodInfo method in type.GetMethods())
                        if (method.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0)
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
        };
    }
}