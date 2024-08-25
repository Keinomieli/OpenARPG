using Godot;
using System;

namespace OpenARPG
{
    [GlobalClass] public partial class Level : Resource
    {
        [Export]public string referenceName;
        [Export]public PackedScene levelScene;
    }
}