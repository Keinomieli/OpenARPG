using System;
using Godot;

namespace OpenARPG
{
	public partial class Inventory : Control
	{
		public static Action ToggleInventory {get; private set;} = () => { Logger.Error("Inventory not initialized"); };
		public static Func<bool> IsOpen {get; private set;} = () => false;

		[Export] private string toggleInputAction = string.Empty;

		bool mouseOver;

        public override void _EnterTree()
        {
            ToggleInventory = () =>
			{
				Visible = !Visible;
			};

			IsOpen = () => Visible;
        }

        public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton inputEventMouseButton)
            {
                if (@event.IsActionPressed("Main_Mouse_Button"))
                {
					if (mouseOver) 
					{
						Logger.Log("Inventory Click");
						AcceptEvent();
					}
				}
			}

			if (@event.IsActionPressed(toggleInputAction)) 
			{
				Visible = !Visible;
			}
		}

		public void _on_mouse_entered()
		{
			mouseOver = true;
		}

		public void _on_mouse_exited()
		{
			mouseOver = false;
		}
	}
}
