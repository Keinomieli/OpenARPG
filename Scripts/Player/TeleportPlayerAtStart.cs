using Godot;
using OpenARPG.Player;
using OpenARPG.System;
using System;

public partial class TeleportPlayerAtStart : Node3D
{
    public override void _EnterTree()
    {
        LevelManager.PostLevelChange += Teleport;
    }

	private void Teleport()
	{
		PlayerCharacter.TeleportTo(GlobalPosition);
		LevelManager.PostLevelChange -= Teleport;
	}

    public override void _ExitTree()
    {
        LevelManager.PostLevelChange -= Teleport;
    }
}
