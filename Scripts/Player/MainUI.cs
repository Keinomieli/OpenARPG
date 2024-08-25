using Godot;
using System;

namespace OpenARPG.Player
{
	public partial class MainUI : Node
	{
		public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed("Escape")) 
			{
				GetTree().Quit();
			}
		}

        public override void _EnterTree()
		{
			/*foreach(Node child in GetChildren())
			{
				Control childControl = child as Control;

				if (childControl != null)
				{
					childControl.Visible = false;
				}
			}*/
		}
	}
}
