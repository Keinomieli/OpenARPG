using Godot;
using OpenARPG.Player;

namespace OpenARPG.System
{
	public partial class Initializer : Node
	{
		[Export] private string loadLevel = "";
		[Export] private int playerVisualModel = -1;

		public override void _Ready()
		{
			Logger.Initialize();
			DevConsole.Initialize();
			GameStateManager.Initialize();
			UserPrefs.LoadOrCreate();

			if (!string.IsNullOrEmpty(loadLevel))
				LevelManager.LoadLevel(loadLevel);
			
			if (playerVisualModel >= 0)
				PlayerVisualModel.SetVisualModel(playerVisualModel);

			Logger.Log("Initializer: Ready: Game initialization finished");

			QueueFree();
		}
	}
}
