using Godot;
using System;

namespace OpenARPG.System
{
    [Tool] //need to be marked as tool so it can be casted to in editor
    public partial class InstancerLayer : TileMapLayer
    {
        [Export] public float yOffset = 0f;
    }
}