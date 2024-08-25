using Godot;
using System;

namespace OpenARPG.Player
{
    public partial class SpectatorCamera : Camera3D
    {
        public static bool IsActive {get; private set;}

        [Export] private Key toggleKey = Key.None;
        [Export] private float speed = 6f;
        [Export] private float sensitivity = 0.2f;

        Vector2 sphericalRotation;
        bool focus = true;

        public override void _Notification(int what)
        {
            switch(what)
            {
                default:
                break;

                case (int)NotificationApplicationFocusOut:
                    focus = false;
                break;

                case (int)NotificationApplicationFocusIn:
                    focus = true;
                break;
            }
        }

        public override void _Ready()
        {
            Vector3 euler = Basis.GetEuler();
            sphericalRotation.X = Mathf.RadToDeg(euler.X);
            sphericalRotation.Y = Mathf.RadToDeg(euler.Y);
            
            Current = false;
            SetProcess(false);
        }

        private void UpdateRotation()
        {
            Vector3 euler = new(Mathf.DegToRad(sphericalRotation.X), Mathf.DegToRad(sphericalRotation.Y), 0f);
            Basis = new Basis(Quaternion.FromEuler(euler));
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent)
            {
                if (keyEvent.Keycode == toggleKey && keyEvent.Pressed)
                {
                    Current = !Current;

                    Camera3D playerCam = PlayerCharacter.GetFollowCamera();
                    GlobalPosition = playerCam.GlobalPosition;
                    GlobalBasis = playerCam.GlobalBasis;

                    Vector3 euler = Basis.GetEuler();
                    sphericalRotation.X = Mathf.RadToDeg(euler.X);
                    sphericalRotation.Y = Mathf.RadToDeg(euler.Y);

                    playerCam.Current = !Current;

                    IsActive = Current;
                    SetProcess(Current);
                }
            }

            if (!focus || !Current)
                return;

            if (@event is InputEventMouseMotion mouseEvent)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    sphericalRotation.Y -= mouseEvent.Relative.X * sensitivity;
                    sphericalRotation.X -= mouseEvent.Relative.Y * sensitivity;

                    if (sphericalRotation.Y > 180f)
                        sphericalRotation.Y -= 360f;

                    if (sphericalRotation.Y < -180f)
                        sphericalRotation.Y += 360f;

                    sphericalRotation.X = Mathf.Clamp(sphericalRotation.X, -90f, +90f);

                    UpdateRotation();
                }
            }
        }

        public override void _Process(double delta)
        {
            Vector3 move = Vector3.Zero;

            if (Input.IsActionPressed("Spectator_Forward"))
                move.Z -= 1;

            if (Input.IsActionPressed("Spectator_Backward"))
                move.Z += 1;

            if (Input.IsActionPressed("Spectator_Left"))
                move.X -= 1;

            if (Input.IsActionPressed("Spectator_Right"))
                move.X += 1;

            if (Input.IsActionPressed("Spectator_Up"))
                move.Y += 1;

            if (Input.IsActionPressed("Spectator_Down"))
                move.Y -= 1;

            Position += Basis * move * speed * (float)delta;
        }
    }
}