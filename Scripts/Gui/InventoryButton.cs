using Godot;
using System;

namespace OpenARPG
{
    public partial class InventoryButton : TextureRect
    {
        [Export] private Material idleMaterial;
        [Export] private Material highlightMaterial;

        bool mouseOver;

        public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton inputEventMouseButton)
            {
                if (@event.IsActionPressed("Main_Mouse_Button"))
                {
					if (mouseOver) 
					{
                        Inventory.ToggleInventory();
						AcceptEvent();
					}
				}
			}
		}

        public void _on_mouse_entered()
		{
			mouseOver = true;
            Material = highlightMaterial;
		}

		public void _on_mouse_exited()
		{
			mouseOver = false;
            Material = idleMaterial;
		}
    }
}