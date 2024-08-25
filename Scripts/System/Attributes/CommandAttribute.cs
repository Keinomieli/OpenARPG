using Godot;
using System;

namespace OpenARPG
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        internal string visibleName;

        public CommandAttribute(string VisibleName)
        {
            visibleName = VisibleName;
        }
    }
}