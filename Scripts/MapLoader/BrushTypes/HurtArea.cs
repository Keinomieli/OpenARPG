using Godot;
using System;
using System.Collections.Generic;

namespace OpenARPG.MapLoader
{
	[Tool]
	public partial class HurtArea : Area3D, MapEntity
	{
		[Export] private int damagePerTick = 10;
		[Export] private int ticksPerSecond = 4;

		[Export] public int damage_type {get; set;} = 0;

        public bool AutoReflectProperties => true;
		public void OnReflectProperties(Dictionary<string, string> properties) {}

        public void AfterMapLoad()
        {
			_ = Connect(Area3D.SignalName.AreaEntered, new Callable(this, MethodName._on_area_3d_area_entered), (uint)ConnectFlags.Persist);
        }

        public void _on_area_3d_area_entered(Area3D area3D)
		{
			GD.Print($"HELLO MISTER {area3D.Name}");
		}
    }
}