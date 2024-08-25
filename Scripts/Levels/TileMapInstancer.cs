using Godot;
using System;

namespace OpenARPG.System
{
	[Tool]
	public partial class TileMapInstancer : Node
	{
		[Export] private Vector2 gridSize = new Vector2(2f,2f);
		[Export] private Node2D designsHierarchy = null;
		[Export] private Node3D creationRoot = null;

		[InspectorButton("Instance TileMap")]
		private void InstanceTileMap()
		{
			if (designsHierarchy == null)
			{
				GD.PrintErr("Designs hierarchy is null");
				return;
			}

			if (creationRoot == null)
			{
				GD.PrintErr("Creation root is null");
				return;
			}

			DestroyCreated();

			foreach (TileMapLayer tileMapLayer in designsHierarchy.GetChildren())
			{
				float yOffset = 0f;

				if (tileMapLayer is InstancerLayer instancerLayer)
					yOffset = instancerLayer.yOffset;

				Rect2I usedRect = tileMapLayer.GetUsedRect();

				for (int x = usedRect.Position.X; x < usedRect.Position.X + usedRect.Size.X; x++)
					for (int y = usedRect.Position.Y; y < usedRect.Position.Y + usedRect.Size.Y; y++)
					{
                        TileData tileData = tileMapLayer.GetCellTileData(new Vector2I(x, y));

						if (tileData == null)
							continue;

                        Variant customData = tileData.GetCustomData("MeshObject");
                        GodotObject go = customData.AsGodotObject();

						if (go == null || go is not PackedScene)
							continue;

						Vector3 pos = new(x * gridSize.X, yOffset, y * gridSize.Y);

						Node3D created = (go as PackedScene).Instantiate() as Node3D;
						creationRoot.AddChild(created);
						created.Owner = creationRoot.Owner;
						created.Position = pos + tileData.GetCustomData("Translation").AsVector3();
					
						if (tileData.GetCustomData("RandomRotation").AsBool())
							created.Rotation = new Vector3(0f, GD.RandRange(0,3) * (Mathf.Pi * 0.5f), 0f);
						else
							created.Rotation = new Vector3(0f, Mathf.DegToRad(tileData.GetCustomData("Rotation").AsSingle()), 0f);
					}
			}
		}

		[InspectorButton("Destroy Created")]
		private void DestroyCreated()
		{	
			if (creationRoot == null)
			{
				GD.PrintErr("Creation root is null");
				return;
			}

			foreach(Node child in creationRoot.GetChildren())
				child.QueueFree();
		}
	}
}