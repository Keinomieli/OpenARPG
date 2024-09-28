using Godot;
using System;

namespace OpenARPG.Player
{
	public partial class PlayerCharacter_ThirdPerson : Node
	{
		public static Vector3 PlayerWorldPosition {get; private set;}
		public static Func<Camera3D> GetFollowCamera {get; private set;} = () => { Logger.Error("PlayerCharacter not initialized"); return null; };
		public static Action<float> Tick {get; private set;} = (_) => { Logger.Error("PlayerCharacter not initialized"); };
		public static Action<float> PhysicsTick {get; private set;} = (_) => { Logger.Error("PlayerCharacter not initialized"); };
		public static Action<Vector3> TeleportTo {get; private set;} = (_) => { Logger.Error("PlayerCharacter not initialized"); };

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

		public override void _EnterTree()
		{
			GetFollowCamera = () => followCamera;
			Tick = _Tick;
			PhysicsTick = _PhysicsTick;

			TeleportTo = (teleportPos) => 
			{ 
				targetPosition = teleportPos;
				visualModel.GlobalPosition = teleportPos;
				PlayerWorldPosition = teleportPos;
				cameraPivot.GlobalPosition = teleportPos + cameraOffset;
				RenderingServer.GlobalShaderParameterSet("PlayerVisionSource", teleportPos);

				Logger.Log($"Player teleported to {teleportPos}");
			};
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

        private void _Tick(float delta)
		{
			visualModel.GlobalPosition = AxMath.ExpDecay(visualModel.GlobalPosition, targetPosition, positionSmoothingSpeed, delta);

			PlayerWorldPosition = visualModel.GlobalPosition;
			cameraPivot.GlobalPosition = AxMath.ExpDecay(cameraPivot.GlobalPosition, PlayerWorldPosition + cameraOffset, cameraPositionSmoothingSpeed, delta);

			Vector3 targetRotation = new(Mathf.DegToRad(cameraSpherical.Y), Mathf.DegToRad(cameraSpherical.X), 0);
			cameraPivot.Rotation = targetRotation;
			visualModel.Rotation = new Vector3(0f, cameraPivot.Rotation.Y, 0f);

			RenderingServer.GlobalShaderParameterSet("PlayerVisionSource", visionSource.GlobalPosition);
		}

		private void _PhysicsTick(float delta)
		{
			//don't process player logic during cutscenes or when using spectator camera
			if (followCamera.Current)
				moveInput = Input.GetVector("Move_Left", "Move_Right", "Move_Down", "Move_Up");
			else
				moveInput = Vector2.Zero;

			ProcessTranslation(delta);
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
