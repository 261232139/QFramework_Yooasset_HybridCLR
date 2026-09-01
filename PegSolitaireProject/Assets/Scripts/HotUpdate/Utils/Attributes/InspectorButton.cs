using System;

namespace HotUpdate.Utils.Attributes
{
    /// <summary>
    /// Marks a parameterless instance method for rendering as a button in a custom Unity Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class InspectorButton : Attribute
    {
    }
}
