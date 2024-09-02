using System;

namespace Keinomieli
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InspectorButtonAttribute(string VisibleName) : Attribute
    {
        public string visibleName { get; private set; } = VisibleName;
    }
}