using Godot;
using System;

namespace OpenARPG.Player
{
    public partial class PlayerVisualModel : Node3D
    {
        public static Action<int> SetVisualModel {get; private set;} = (index) => { Logger.Error("PlayerVisualModel: SetVisualModel: PlayerVisualModel not initialized"); };

        Node3D currentVisualModel = null;

        public override void _EnterTree()
        {
            SetVisualModel = (index) => 
            { 
                if (currentVisualModel != null)
                {
                    currentVisualModel.Visible = false;
                    currentVisualModel.SetProcess(false);
                    currentVisualModel = null;
                }

                if (index >= 0 && index < GetChildCount())
                {
                    Node3D child3D = GetChild(index) as Node3D;

                    if (child3D != null)
                    {
                        child3D.Visible = true;
                        child3D.SetProcess(false);
                    }
                }

                Logger.Log($"PlayerVisualModel: SetVisualModel: Player visual model set to index {index}");
            };
        }
    }
}