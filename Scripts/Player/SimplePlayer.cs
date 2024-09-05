using Godot;
using System;

namespace OpenARPG
{
	public partial class SimplePlayer : Node
	{
		[Export] private float moveSpeed = 4.0f;
		[Export] private Camera3D followCamera;
		[Export] private Node3D visualModel;
		[Export] private Node3D visionSource;
		[Export] private Node3D cameraPivot;
		[Export] private Vector3 cameraOffset;
		[Export] private Vector2 cameraVerticalClamp = new(-80,80);
		[Export] private Vector2 cameraSensitivity = new(.2f,.2f);
		[Export(PropertyHint.Range, "1,25,0.1,or_greater,or_less")] 
		private float positionSmoothingSpeed = 20.0f;
		[Export(PropertyHint.Range, "1,25,0.1,or_greater,or_less")] 
		private float rotationSmoothingSpeed = 16.0f;
		[Export(PropertyHint.Range, "1,25,0.1,or_greater,or_less")] 
		private float cameraPositionSmoothingSpeed = 12.0f;

		Vector3 targetPosition;
		Vector2 moveInput;
		Vector2 cameraSpherical;

        public override void _Ready()
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        public override void _Input(InputEvent @event)
        {
			if (@event is InputEventMouseMotion motion)
			{
				cameraSpherical -= motion.Relative * cameraSensitivity;
				cameraSpherical.Y = Mathf.Clamp(cameraSpherical.Y, cameraVerticalClamp.X, cameraVerticalClamp.Y);
				
				if (cameraSpherical.X < -180f)
					cameraSpherical.X += 360f;
				else if (cameraSpherical.X > 180f)
					cameraSpherical.X -= 360f;
			}
        }

        public override void _Process(double delta)
		{
			moveInput = Input.GetVector("Move_Left", "Move_Right", "Move_Down", "Move_Up");
			ProcessTranslation((float)delta);

			visualModel.GlobalPosition = AxMath.ExpDecay(visualModel.GlobalPosition, targetPosition, positionSmoothingSpeed, (float)delta);
			cameraPivot.GlobalPosition = AxMath.ExpDecay(cameraPivot.GlobalPosition, visualModel.GlobalPosition + cameraOffset, cameraPositionSmoothingSpeed, (float)delta);

			Vector3 targetRotation = new(Mathf.DegToRad(cameraSpherical.Y), Mathf.DegToRad(cameraSpherical.X), 0);
			cameraPivot.Rotation = targetRotation;
			visualModel.Rotation = new Vector3(0f, cameraPivot.Rotation.Y, 0f);

			RenderingServer.GlobalShaderParameterSet("PlayerVisionSource", visionSource.GlobalPosition);
		}

		private void ProcessTranslation(float delta)
		{
			Vector3 cameraWorldForward = -followCamera.GlobalBasis.Z;
			Vector3 cameraWorldRight = followCamera.GlobalBasis.X;
			cameraWorldForward.Y = 0;
			cameraWorldForward = cameraWorldForward.Normalized();
			cameraWorldRight.Y = 0;
			cameraWorldRight = cameraWorldRight.Normalized();

			Vector3 moveVector = (moveInput.Y * cameraWorldForward + moveInput.X * cameraWorldRight).Normalized();
			targetPosition += moveVector * moveSpeed * (float)delta;
		}
	}
}