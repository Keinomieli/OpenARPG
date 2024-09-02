using Godot;
using System;

namespace OpenARPG.Player
{
    public partial class MouseHandler3D : Node3D
    {
        public static Action UpdateMouseWorldPosition {get; private set;} = () => { Logger.Error("MouseHandler not initialized"); };
        public static Vector3? MouseWorldPosition {get; private set;}

        [Export] private Camera3D raycastCamera;
        [Export] private float raycastLength = 100f;

        Plane playerGroundPlane;

        public override void _EnterTree()
        {
            playerGroundPlane = new Plane(Vector3.Up, 0f);
            UpdateMouseWorldPosition = _UpdateMouseWorldPosition;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton inputEventMouseButton)
            {
                if (@event.IsActionPressed("Main_Mouse_Button"))
                {
                    if (MouseWorldPosition.HasValue)
                    {
                        Logger.Log("World Click");
                    }
                }
            }
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionPressed("Main_Mouse_Button"))
            {
                if (MouseWorldPosition.HasValue)
                {
                    Vector3 a = PlayerCharacter.PlayerWorldPosition;
                    Vector3 b = MouseWorldPosition.Value;
                    DebugDraw3D.Sphere(a, 0.1f, Colors.Blue, 0f);
                    DebugDraw3D.Sphere(b, 0.1f, Colors.Red, 0f);
                    DebugDraw3D.Line(a, b, Colors.Cyan, 0f);
                }
            }
        }

        private void _UpdateMouseWorldPosition()
        {
            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector3 from = raycastCamera.ProjectRayOrigin(mousePos);
            Vector3 to = from + raycastCamera.ProjectRayNormal(mousePos) * raycastLength;

            PhysicsRayQueryParameters3D rayQuery = new()
            {
                From = from,
                To = to
            };
            
            //checks against colliders in the world
            Godot.Collections.Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(rayQuery);

            if (result.Count > 0)
            {
                MouseWorldPosition = (Vector3)result["position"];
            }
            else
            {
                //as a fallback checks against player feet plane
                playerGroundPlane.D = PlayerCharacter.PlayerWorldPosition.Y;
                Vector3? point = playerGroundPlane.IntersectsSegment(from, to);
                
                if (point != null)
                {
                    MouseWorldPosition = point.Value;
                }
                else
                {
                    MouseWorldPosition = null;
                }
            }
        }
    }
}