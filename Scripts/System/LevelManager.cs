using Godot;
using System;

namespace OpenARPG.System
{
	public partial class LevelManager : Node
	{
		[Export] public Level[] includedLevels = Array.Empty<Level>();

		public static Action<string> LoadLevel {get; private set;} = (levelReferenceName) => { Logger.Error("LevelManager not initialized"); };
		public static Action UnloadLevel {get; private set;} = () => { Logger.Error("LevelManager not initialized"); };

		public delegate void PostLevelChangeEventHandler();
		public static event PostLevelChangeEventHandler PostLevelChange;

		private string currentLevelReferenceName = string.Empty;
		private Node currentLevel = null;

		public override void _EnterTree()
		{

			UnloadLevel = () =>
			{
				if (currentLevel == null)
					return;

				currentLevel.QueueFree();

				currentLevel = null;
				currentLevelReferenceName = string.Empty;

				Logger.Log($"LevelManager: UnloadLevel: Unloaded current level");
			};

			LoadLevel = (string levelReferenceName) => 
			{
				if (string.IsNullOrEmpty(levelReferenceName))
				{
					Logger.Error($"LevelManager: LoadLevel: Reference name was null or empty");
					return;
				}

				Level level = null;

				foreach(Level includedLevel in includedLevels)
					if (includedLevel.referenceName == levelReferenceName)
					{
						level = includedLevel;
						break;
					}

				if (level == null)
				{
					Logger.Error($"LevelManager: LoadLevel: Level with reference name \"{levelReferenceName}\" does not exist");
					return;
				}

				GameStateManager.RequestNewState(new GameStateManager.GameState_ChangeLevel());

				UnloadLevel();

				currentLevel = level.levelScene.Instantiate();
				currentLevelReferenceName = levelReferenceName;

				AddChild(currentLevel);

				Logger.Log($"LevelManager: LoadLevel: Loaded level with reference name \"{currentLevelReferenceName}\"");

				PostLevelChange?.Invoke();

				GameStateManager.RequestNewState(new GameStateManager.GameState_Play());
				
				/*Tween tween = GetTree().CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine).SetLoops();
				tween.TweenProperty(instance, "position", new Vector2(-100f, 0f), 2.0);
				tween.TweenInterval(1.0);
				tween.TweenProperty(instance, "position", new Vector2(0f, 0f), 2.0);
				tween.TweenInterval(1.0);*/
			};
		}
	}
}