using Godot;
using System;

namespace OpenARPG.Player
{
    public partial class MouseHandler2D : Node2D
    {
        public override void _EnterTree()
        {
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton inputEventMouseButton)
            {
                if (@event.IsActionPressed("Main_Mouse_Button"))
                {
                    /*var mousePosition = GetViewport().GetMousePosition();

                    var spaceRid = GetWorld2D().Space;
                    var spaceState = PhysicsServer2D.SpaceGetDirectState(spaceRid);
                    var query = PhysicsRayQueryParameters2D.Create(Vector2.Zero, mousePosition);
                    var result = spaceState.IntersectRay(query);

                    if (result.Count > 0)
                    {
                    }*/
                }
            }
        }
    }
}