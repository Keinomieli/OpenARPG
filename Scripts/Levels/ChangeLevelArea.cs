using Godot;
using OpenARPG.System;
using System;

namespace OpenARPG
{
	public partial class ChangeLevelArea : Node
	{
		[Export] private string targetLevel = string.Empty;

		public void _on_area_3d_area_entered(Area3D area3D)
		{
			if (GameStateManager.currentState.AllowPlayerEvents)
			{
				Logger.Log($"Player entered level change area \"{Name}\"");
				LevelManager.LoadLevel(targetLevel);
			}
		}
	}
}