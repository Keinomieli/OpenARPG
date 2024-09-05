using Godot;
using OpenARPG.MapLoader;
using System;
using System.Collections.Generic;

namespace OpenARPG.Entities
{
    [Tool]
    public partial class MarsFrog : Node3D, MapEntity
    {
        [Export] private int hitpoints = 100;

        public bool AutoReflectProperties => true;

        public void AfterMapLoad() { GD.Print("Hello World"); }

        public void OnReflectProperties(Dictionary<string, string> properties) { }

    }
}