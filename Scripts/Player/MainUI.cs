using Godot;
using System;

namespace OpenARPG.Player
{
	public partial class MainUI : Node
	{
		//public static Action UpdateMouseState {get; private set;} = () => { Logger.Error("MainUI not initialized"); };
		//public static MouseMode currentMouseMode {get; private set;} = MouseMode.Gui;

		//private static Vector2 savedGuiPos;
	
		/*private void SetMode(MouseMode newMode)
		{
			if (currentMouseMode == newMode)
				return;

			if (newMode != MouseMode.Gui)
			{
				savedGuiPos = GetViewport().GetMousePosition();
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
				Input.WarpMouse(savedGuiPos);
			}

			currentMouseMode = newMode;
		}*/

		/*public enum MouseMode
		{
			Gui,
			Character,
		}*/

        public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed("Escape")) 
			{
				GetTree().Quit();
			}
		}

        /*public override void _EnterTree()
		{
			UpdateMouseState = () => 
			{
				if (Inventory.IsOpen())
				{
					SetMode(MouseMode.Gui);
				}
				else
				{
					SetMode(MouseMode.Character);
				}
			};
		}*/
	}
}
