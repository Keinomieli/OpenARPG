using Godot;
using System;

namespace OpenARPG.System
{
    public partial class Sequencer : Node
    {
        public override void _Process(double delta)
        {
            if (delta <= 0.0)
                return;

            GameStateManager.Tick((float)delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (delta <= 0.0)
                return;

            GameStateManager.PhysicsTick((float)delta);
        }
    }
}