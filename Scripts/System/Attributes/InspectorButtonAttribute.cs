using Godot;
using System;

namespace OpenARPG
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InspectorButtonAttribute : Attribute
    {
        public string visibleName;

        public InspectorButtonAttribute(string VisibleName)
        {
            visibleName = VisibleName;
        }
    }
}