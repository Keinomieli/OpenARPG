using Godot;
using Keinomieli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenARPG.MapLoader
{
	[Tool]
	public partial class MapLoader : Node
	{
		[ExportGroup("Settings")]
		[Export] private Node3D creationRoot;
		[Export(PropertyHint.GlobalFile, "*.map")] private string mapFile;
		[Export(PropertyHint.Range,"1.0,128.0")] private float inverseScale = 64f;
		[Export(PropertyHint.Dir)] private string entitiesFolder = "res://Assets/Maps/Entities";
		[Export(PropertyHint.Dir)] private string materialsFolder = "res://Assets/Maps/Materials";

		[ExportGroup("Lightmapping")]
		[Export] private bool unwrapLightmapUV2 = true;
		[Export(PropertyHint.Range,"0.05,1.0")] private float lightmapTexelSize = 0.2f;

		[ExportGroup("Special Surfaces")]
		[Export] private string[] ignoreTextures = [];
		[Export] private string[] noCollisionTextures = [];
		[Export] private string[] collisionOnlyTextures = [];

		private enum ParserMode
		{
			File,
			Entity,
			Brush
		}

		Vector3 ConvertAxisAndScale(Vector3 vertex) 
				=> new(vertex.X / inverseScale, vertex.Z / inverseScale, -vertex.Y / inverseScale);

        static Vector3 ConvertAxis(Vector3 vector)
				=> new(vector.X, vector.Z, -vector.Y);

		[InspectorButton("Load Map")]
		private void LoadMap()
		{
			if (creationRoot == null)
			{
				GD.PrintErr("MapLoader: LoadMap: creation root not set");
				return;
			}

			foreach(Node child in creationRoot.GetChildren())
				child.QueueFree();

			if (string.IsNullOrEmpty(mapFile))
			{
				GD.PrintErr("MapLoader: LoadMap: mapFile not set");
				return;
			}

			string filePath = ProjectSettings.GlobalizePath(mapFile);

			if (!File.Exists(filePath))
			{
				GD.PrintErr($"MapLoader: LoadMap: could not find mapFile \"{mapFile}\"");
				return;
			}

			string[] lines = File.ReadAllLines(filePath);

			if (!lines[0].Contains("Game: OpenARPG"))
			{
				GD.PrintErr($"MapLoader: LoadMap: mapFile had wrong game setting");
				return;
			}

			if (!lines[1].Contains("Format: Valve"))
			{
				GD.PrintErr($"MapLoader: LoadMap: mapFile was not saved in Valve format");
				return;
			}

			ParserMode currentMode = ParserMode.File;
			List<TrenchEntity> trenchEntities = [];
			TrenchEntity currentEntity = null;
			TrenchBrush currentBrush = null;

			for(int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				if (line.StartsWith("//"))
				{					
					continue;
				}
				else if (line.StartsWith('{'))
				{
					switch(currentMode)
					{
						default:
							GD.PrintErr($"Illegal bracket open at line {i}");
							return;

						case ParserMode.File: 
							if (currentEntity != null)
							{
								GD.PrintErr($"Tried to start an entity while previous still active. At line {i}");
								return;
							}

							currentEntity = new TrenchEntity();
							currentMode = ParserMode.Entity;
							break;

						case ParserMode.Entity:
							if (currentBrush != null)
							{
								GD.PrintErr($"Tried to start a brush while previous still active. At line {i}");
								return;
							}
							if (currentEntity == null)
							{
								GD.PrintErr($"Tried to start a brush while no entity was active. At line {i}");
								return;
							}

							currentBrush = new TrenchBrush();
							currentMode = ParserMode.Brush;
							break;
					}
				}
				else if (line.StartsWith('}'))
				{
					switch(currentMode)
					{
						default:
							GD.PrintErr($"Illegal bracket close at line {i}");
							return;

						case ParserMode.Entity: 
							if (currentEntity == null)
							{
								GD.PrintErr($"Tried to end an entity while none was active. At line {i}");
								return;
							}
							trenchEntities.Add(currentEntity);
							currentEntity = null;
							currentMode = ParserMode.File;
							break;

						case ParserMode.Brush:
							if (currentBrush == null)
							{
								GD.PrintErr($"Tried to end a brush while none was active. At line {i}");
								return;
							}
							if (currentEntity == null)
							{
								GD.PrintErr($"Tried to end a brush while no entity was active. At line {i}");
								return;
							}
							currentEntity.brushes.Add(currentBrush);
							currentBrush = null;
							currentMode = ParserMode.Entity;
							break;
					}
				}
				else if (line.StartsWith('\"'))
				{
					if (currentMode != ParserMode.Entity)
					{
						GD.PrintErr($"Illegal quotation marks at line {i}");
						return;
					}

					if (currentEntity == null)
					{
						GD.PrintErr($"Tried to start a property while no entity was active. At line {i}");
						return;
					}

					List<string> properties = TrenchEntity.ParsePropertyLine(line);

					if (properties.Count != 2)
					{
						GD.PrintErr($"Illegal amount of property definitions at line {i}");
						return;
					}

					currentEntity.properties.Add(properties[0],properties[1]);
				}
				else if (line.StartsWith('('))
				{
					if (currentMode != ParserMode.Brush)
					{
						GD.PrintErr($"Illegal quotation marks at line {i}");
						return;
					}

					if (currentBrush == null)
					{
						GD.PrintErr($"Tried to read a brush plane while no brush was active. At line {i}");
						return;
					}

					if (currentEntity == null)
					{
						GD.PrintErr($"Tried to read a brush plane no entity was active. At line {i}");
						return;
					}

                    BrushPlane brushPlane = BrushPlane.Parse(line);

					if (brushPlane == null)
					{
						GD.PrintErr($"Unable to parse brush plane at line {i}");
						return;
					}
					
					brushPlane.CalculateDerivatives();

					currentBrush.planes.Add(brushPlane);
				}
			}
			//file read end

			Dictionary<int,string> layerNames = new() { {-1,"Default Layer"}};
			Dictionary<int,int> groupLayers = [];

			//first pass to gather layers and see if export set to false
			List<int> skippedLayers = [];
			foreach(TrenchEntity trenchEntity in trenchEntities)
			{
				int tbId = -1;

				if (trenchEntity.properties.TryGetValue("_tb_type", out string tbType))
					if (trenchEntity.properties.TryGetValue("_tb_id", out string _tbId))
					{
						tbId = int.Parse(_tbId);

						if (tbType == "_tb_layer")
						{
							if (trenchEntity.properties.TryGetValue("_tb_name", out string tbName))
								layerNames.Add(tbId, tbName);
						}
						else if (tbType == "_tb_group")
						{
							if (trenchEntity.properties.TryGetValue("_tb_layer", out string tbLayer))
								groupLayers.Add(tbId, int.Parse(tbLayer));
						}
					}
					
				if (trenchEntity.properties.TryGetValue("_tb_layer_omit_from_export", out string value))
					if (value == "1")
						skippedLayers.Add(tbId);
			}

			Dictionary<int, List<TrenchEntity>> layers = [];
			Dictionary<int, Node3D> layerNodes = [];
			foreach(KeyValuePair<int, string> layer in layerNames)
				if (!skippedLayers.Contains(layer.Key))
				{
					layers.Add(layer.Key, []);

                    Node3D layerNode = new();
					creationRoot.AddChild(layerNode);
					layerNode.Owner = creationRoot.Owner;
					layerNode.Name = layer.Value;
					layerNodes.Add(layer.Key, layerNode);
                }

			//second pass, remove entities that are on skipped layers
			for(int i = trenchEntities.Count - 1; i >= 0; i--)
			{
				TrenchEntity trenchEntity = trenchEntities[i];

                int layerId = -1;

                if (trenchEntity.properties.TryGetValue("_tb_type", out string tbType))
					if (tbType == "_tb_layer")
						if (trenchEntity.properties.TryGetValue("_tb_id", out string tbId))
							layerId = int.Parse(tbId);

				if (layerId == -1)
					if (trenchEntity.properties.TryGetValue("_tb_layer", out string tbLayer))
						layerId = int.Parse(tbLayer);
					else if (trenchEntity.properties.TryGetValue("_tb_group", out string tbGroup))
						layerId = groupLayers[int.Parse(tbGroup)];
				
				if (!skippedLayers.Contains(layerId))
					layers[layerId].Add(trenchEntity);
			}

			List<(MapEntity, TrenchEntity)> createdEntities = [];

			//and finally we instantiate all the remaining entities
			foreach(KeyValuePair<int,List<TrenchEntity>> layer in layers)
			{
				Node3D layerNode = layerNodes[layer.Key];
				Dictionary<string, BrushSurface> layerSurfaces = [];
				List<ConvexCollider> convexColliders = [];
				List<ConcaveObject> concaveObjects = [];
				List<ConvexObject> convexObjects = [];

				foreach(TrenchEntity trenchEntity in layer.Value)
				{
					if (!trenchEntity.properties.TryGetValue("classname", out string classname))
					{
						GD.PrintErr("Entity with no class name");
						return;
					}

					if (trenchEntity.brushes.Count > 0)
					{
						if (trenchEntity.properties.TryGetValue("brush_type", out string brushType))
						{
							if (trenchEntity.properties.TryGetValue("pivot", out string pivotType))
								trenchEntity.pivotType = (TrenchEntity.PivotType)int.Parse(pivotType);

							switch (brushType)
							{
								default:
									GD.PrintErr($"Unknown brushType \"{brushType}\"");
									return;
								
								case "ConvexCollider":
								{
									ConvexCollider convexBrush = new()
									{
										ownerEntity = trenchEntity
									};

									foreach (TrenchBrush brush in trenchEntity.brushes)
									{
										brush.Triangulate();

										foreach(BrushPlane plane in brush.planes)
										{
											foreach (Vector3 vertex in plane.vertices)
											{
												Vector3 convertedVertex = ConvertAxisAndScale(vertex);
												convexBrush.collisionVertices.Add(convertedVertex);
											}
										}
									}

									convexColliders.Add(convexBrush);
								}
								break;

								case "ConcaveStaticBody":
								{
									ConcaveObject concaveObject = new()
									{
										ownerEntity = trenchEntity
									};

									AddSurfaces(trenchEntity, ref concaveObject.surfaces);
									
									if (concaveObject.surfaces.Count > 0)
										concaveObjects.Add(concaveObject);
								}
								break;

								case "ConvexStaticBody":
								case "ConvexRigidBody":
								{
									ConvexObject convexObject = new()
									{
										ownerEntity = trenchEntity,
										useRigidBody = brushType == "ConvexRigidBody"
									};

									AddSurfaces(trenchEntity, ref convexObject.surfaces);
									
									foreach (TrenchBrush brush in trenchEntity.brushes)
									{
										//AddSurfaces function already triangulated the brushes
										//brush.Triangulate();

										foreach(BrushPlane plane in brush.planes)
										{
											foreach (Vector3 vertex in plane.vertices)
											{
												Vector3 convertedVertex = ConvertAxisAndScale(vertex);
												convexObject.collisionVertices.Add(convertedVertex);
											}
										}
									}

									if (convexObject.surfaces.Count > 0 || convexObject.collisionVertices.Count > 2)
										convexObjects.Add(convexObject);
								}
								break;
							}
						}
						else
						{
							AddSurfaces(trenchEntity, ref layerSurfaces);
						}
					}
					else
					{
						switch(classname)
						{
							//if entity has no brushes and is not one of the Trenchbroom internal classes it is a point entity
							default:
									string resourcePath = $"{entitiesFolder}/{classname}.tscn";
									Resource resource = GD.Load(resourcePath);
									if (resource == null)
									{
										GD.PrintErr($"Could not load resource \"{resourcePath}\"");
										return;
									}

									PackedScene entityScene = (PackedScene)resource;
									Node entity = entityScene.Instantiate();
									layerNode.AddChild(entity);
									entity.Owner = layerNode.Owner;
									entity.Name = classname;

									if (entity is Node3D node3D)
									{
										if (trenchEntity.properties.TryGetValue("origin", out string origin))
										{
											string[] parts = origin.Split(' ');
											Vector3 originValue = new(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
											node3D.Position = ConvertAxisAndScale(originValue);
										}

										if (trenchEntity.properties.TryGetValue("angle", out string angle))
										{
											node3D.Rotation = new(0f, Mathf.DegToRad(float.Parse(angle)), 0f);
										}
									}

									if (entity is MapEntity mapEntity)
										createdEntities.Add((mapEntity, trenchEntity));
								break;

							case "worldspawn":
							case "func_group":
								break;
						}
					}
				}

				//create layer static surfaces
				{
					List<Vector3> collisionVertices = [];
					ArrayMesh arrMesh = new();
					int surfaceIndex = 0;

					foreach(KeyValuePair<string, BrushSurface> pair in layerSurfaces)
					{
						BrushSurface layerSurface = pair.Value;

						foreach(int index in layerSurface.collisionIndices)
							collisionVertices.Add(layerSurface.vertices[index]);

						if (layerSurface.indices.Count > 2)
						{
							Godot.Collections.Array arrays = [];
							arrays.Resize((int)Mesh.ArrayType.Max);
							arrays[(int)Mesh.ArrayType.Vertex] = layerSurface.vertices.ToArray();
							arrays[(int)Mesh.ArrayType.Index] = layerSurface.indices.ToArray();
							arrays[(int)Mesh.ArrayType.Normal] = layerSurface.normals.ToArray();
							arrays[(int)Mesh.ArrayType.TexUV] = layerSurface.uvs.ToArray();
							arrays[(int)Mesh.ArrayType.Tangent] = layerSurface.tangents.ToArray();
							arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
							arrMesh.SurfaceSetMaterial(surfaceIndex, layerSurface.material);
							surfaceIndex++;
						}
					}

					if (unwrapLightmapUV2)
						arrMesh.LightmapUnwrap(creationRoot.GlobalTransform, lightmapTexelSize);

					MeshInstance3D m = new()
					{
						Mesh = arrMesh,
						Name = "_geometry"
					};

					layerNode.AddChild(m);
					m.Owner = layerNode.Owner;

					if (collisionVertices.Count > 2)
					{
						StaticBody3D staticBody3D = new();
						layerNode.AddChild(staticBody3D);
						staticBody3D.Owner = layerNode.Owner;
						staticBody3D.Name = "StaticBody3D";

						CollisionShape3D collisionShape3D = new();
						staticBody3D.AddChild(collisionShape3D);
						collisionShape3D.Owner = staticBody3D.Owner;
						collisionShape3D.Name = "CollisionShape3D";

						ConcavePolygonShape3D shape3D = new();
						shape3D.SetFaces([.. collisionVertices]);
						collisionShape3D.Shape = shape3D;
					}
				}

				foreach(ConvexCollider convexCollider in convexColliders)
				{
					Type t;
					if (convexCollider.ownerEntity.properties.TryGetValue("assembly_type", out string assemblyType))
						t = Type.GetType($"OpenARPG.MapLoader.{assemblyType}");
					else
						t = typeof(Node3D);
					
                    convexCollider.CalculateBounds(out Vector3 min, out Vector3 max);
					Vector3 pivot = GetPivot(convexCollider.ownerEntity.pivotType, min, max);

                    Node3D node3D = Activator.CreateInstance(t) as Node3D;
					layerNode.AddChild(node3D);
					node3D.Owner = layerNode.Owner;
					if (convexCollider.ownerEntity.properties.TryGetValue("visible_name", out string visibleName))
						node3D.Name = visibleName;
					else
						node3D.Name = "ConvexCollider";
					node3D.Position = pivot;

					CollisionShape3D collisionShape3D = new();
					node3D.AddChild(collisionShape3D);
					collisionShape3D.Owner = node3D.Owner;
					collisionShape3D.Name = "CollisionShape3D";

					Vector3[] points = new Vector3[convexCollider.collisionVertices.Count];
					for (int i = 0; i < points.Length; i++)
						points[i] = convexCollider.collisionVertices[i] - pivot;

					collisionShape3D.Shape = new ConvexPolygonShape3D()
                    {
                        Points = points
                    };

					if (node3D is MapEntity mapEntity)
						createdEntities.Add((mapEntity, convexCollider.ownerEntity));
				}

				foreach(ConcaveObject concaveObject in concaveObjects)
				{
					Type t;
					if (concaveObject.ownerEntity.properties.TryGetValue("assembly_type", out string assemblyType))
						t = Type.GetType($"OpenARPG.MapLoader.{assemblyType}");
					else
						t = typeof(Node3D);

					concaveObject.CalculateBounds(out Vector3 min, out Vector3 max);
					Vector3 pivot = GetPivot(concaveObject.ownerEntity.pivotType, min, max);

					Node3D node3D = Activator.CreateInstance(t) as Node3D;
					layerNode.AddChild(node3D);
					node3D.Owner = layerNode.Owner;
					if (concaveObject.ownerEntity.properties.TryGetValue("visible_name", out string visibleName))
						node3D.Name = visibleName;
					else
						node3D.Name = "ConcaveObject";
					node3D.Position = pivot;

					bool smoothNormals = false;
					if (concaveObject.ownerEntity.properties.TryGetValue("normals", out string normals))
						smoothNormals = normals == "1";

					List<Vector3> collisionVertices = [];
					ArrayMesh arrMesh = new();
					int surfaceIndex = 0;

					foreach(KeyValuePair<string, BrushSurface> pair in concaveObject.surfaces)
					{
						BrushSurface surface = pair.Value;

						if (smoothNormals)
							surface.SmoothNormals();

						foreach(int index in surface.collisionIndices)
							collisionVertices.Add(surface.vertices[index] - pivot);

						if (surface.indices.Count > 2)
						{
							Vector3[] vertices = new Vector3[surface.vertices.Count];
							for (int i = 0; i < vertices.Length; i++)
								vertices[i] = surface.vertices[i] - pivot;

							Godot.Collections.Array arrays = [];
							arrays.Resize((int)Mesh.ArrayType.Max);
							arrays[(int)Mesh.ArrayType.Vertex] = vertices;
							arrays[(int)Mesh.ArrayType.Index] = surface.indices.ToArray();
							arrays[(int)Mesh.ArrayType.Normal] = surface.normals.ToArray();
							arrays[(int)Mesh.ArrayType.TexUV] = surface.uvs.ToArray();
							arrays[(int)Mesh.ArrayType.Tangent] = surface.tangents.ToArray();
							arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
							arrMesh.SurfaceSetMaterial(surfaceIndex, surface.material);
							surfaceIndex++;
						}
					}

					if (unwrapLightmapUV2)
						arrMesh.LightmapUnwrap(creationRoot.GlobalTransform, lightmapTexelSize);

					MeshInstance3D m = new()
					{
						Mesh = arrMesh,
						Name = "_geometry"
					};

					node3D.AddChild(m);
					m.Owner = node3D.Owner;

					if (collisionVertices.Count > 2)
					{
						StaticBody3D staticBody3D = new();
						node3D.AddChild(staticBody3D);
						staticBody3D.Owner = node3D.Owner;
						staticBody3D.Name = "StaticBody3D";

						CollisionShape3D collisionShape3D = new();
						staticBody3D.AddChild(collisionShape3D);
						collisionShape3D.Owner = staticBody3D.Owner;
						collisionShape3D.Name = "CollisionShape3D";

						ConcavePolygonShape3D shape3D = new();
						shape3D.SetFaces([.. collisionVertices]);
						collisionShape3D.Shape = shape3D;
					}

					if (node3D is MapEntity mapEntity)
						createdEntities.Add((mapEntity, concaveObject.ownerEntity));
				}

				foreach(ConvexObject convexObject in convexObjects)
				{
					Type t;
					if (convexObject.ownerEntity.properties.TryGetValue("assembly_type", out string assemblyType))
						t = Type.GetType($"OpenARPG.MapLoader.{assemblyType}");
					else
						t = typeof(Node3D);

                    convexObject.CalculateBounds(out Vector3 min, out Vector3 max);
					Vector3 pivot = GetPivot(convexObject.ownerEntity.pivotType, min, max);

					Node3D node3D = Activator.CreateInstance(t) as Node3D;
					layerNode.AddChild(node3D);
					node3D.Owner = layerNode.Owner;
					if (convexObject.ownerEntity.properties.TryGetValue("visible_name", out string visibleName))
						node3D.Name = visibleName;
					else
						node3D.Name = "ConvexRigidBody";
					node3D.Position = pivot;

					bool smoothNormals = false;
					if (convexObject.ownerEntity.properties.TryGetValue("normals", out string normals))
						smoothNormals = normals == "1";

					ArrayMesh arrMesh = new();
					int surfaceIndex = 0;

					foreach(KeyValuePair<string, BrushSurface> pair in convexObject.surfaces)
					{
						BrushSurface surface = pair.Value;

						if (smoothNormals)
							surface.SmoothNormals();

						if (surface.indices.Count > 2)
						{
							Vector3[] vertices = new Vector3[surface.vertices.Count];
							for (int i = 0; i < vertices.Length; i++)
								vertices[i] = surface.vertices[i] - pivot;

							Godot.Collections.Array arrays = [];
							arrays.Resize((int)Mesh.ArrayType.Max);
							arrays[(int)Mesh.ArrayType.Vertex] = vertices;
							arrays[(int)Mesh.ArrayType.Index] = surface.indices.ToArray();
							arrays[(int)Mesh.ArrayType.Normal] = surface.normals.ToArray();
							arrays[(int)Mesh.ArrayType.TexUV] = surface.uvs.ToArray();
							arrays[(int)Mesh.ArrayType.Tangent] = surface.tangents.ToArray();
							arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
							arrMesh.SurfaceSetMaterial(surfaceIndex, surface.material);
							surfaceIndex++;
						}
					}

					if (unwrapLightmapUV2)
						arrMesh.LightmapUnwrap(creationRoot.GlobalTransform, lightmapTexelSize);

					MeshInstance3D m = new()
					{
						Mesh = arrMesh,
						Name = "_geometry"
					};

					node3D.AddChild(m);
					m.Owner = node3D.Owner;

					if (convexObject.collisionVertices.Count > 2)
					{
						if (convexObject.useRigidBody)
						{
							RigidBody3D rigidBody3D = new();
							node3D.AddChild(rigidBody3D);
							rigidBody3D.Owner = node3D.Owner;
							rigidBody3D.Name = "RigidBody3D";

							Vector3[] points = new Vector3[convexObject.collisionVertices.Count];
							for (int i = 0; i < points.Length; i++)
								points[i] = convexObject.collisionVertices[i] - pivot;

							CollisionShape3D collisionShape3D = new();
							rigidBody3D.AddChild(collisionShape3D);
							collisionShape3D.Owner = rigidBody3D.Owner;
							collisionShape3D.Name = "CollisionShape3D";
							collisionShape3D.Shape = new ConvexPolygonShape3D()
							{
								Points = points
							};
						}
						else
						{
							StaticBody3D staticBody3D = new();
							node3D.AddChild(staticBody3D);
							staticBody3D.Owner = node3D.Owner;
							staticBody3D.Name = "StaticBody3D";

							Vector3[] points = new Vector3[convexObject.collisionVertices.Count];
							for (int i = 0; i < points.Length; i++)
								points[i] = convexObject.collisionVertices[i] - pivot;

							CollisionShape3D collisionShape3D = new();
							staticBody3D.AddChild(collisionShape3D);
							collisionShape3D.Owner = staticBody3D.Owner;
							collisionShape3D.Name = "CollisionShape3D";
							collisionShape3D.Shape = new ConvexPolygonShape3D()
							{
								Points = points
							};
						}
					}

					if (node3D is MapEntity mapEntity)
						createdEntities.Add((mapEntity, convexObject.ownerEntity));
				}
			}

			//done in two passes so the objects can reference each other based on their properties
			foreach((MapEntity mapEntity, TrenchEntity trenchEntity) in createdEntities)
			{
				if (mapEntity.AutoReflectProperties)
					mapEntity.ReflectProperties(trenchEntity.properties);
				
				mapEntity.OnReflectProperties(trenchEntity.properties);
			}

			foreach((MapEntity mapEntity, TrenchEntity trenchEntity) in createdEntities)
				mapEntity.AfterMapLoad();

			GD.Print($"Loaded map \"{mapFile}\"");
		}

		private static Vector3 GetPivot(TrenchEntity.PivotType pivotType, Vector3 min, Vector3 max)
			=> pivotType switch
            {
                TrenchEntity.PivotType.Center => new((min.X + max.X) * .5f, (min.Y + max.Y) * .5f, (min.Z + max.Z) * .5f),
                TrenchEntity.PivotType.Bottom => new((min.X + max.X) * .5f, min.Y, (min.Z + max.Z) * .5f),
                TrenchEntity.PivotType.Top => new((min.X + max.X) * .5f, max.Y, (min.Z + max.Z) * .5f),
                _ => Vector3.Zero,
            };

		private void AddSurfaces(TrenchEntity trenchEntity, ref Dictionary<string, BrushSurface> surfaces) 
		{
			foreach(TrenchBrush brush in trenchEntity.brushes)
			{
				brush.Triangulate();

				foreach(BrushPlane plane in brush.planes)
				{
					if (ignoreTextures.Contains(plane.textureName))
						continue;

					if (!surfaces.TryGetValue(plane.textureName, out BrushSurface surface))
					{
						int textureWidth = 64;
						int textureHeight = 64;

						string materialPath = $"{materialsFolder}/{plane.textureName}.tres";
						Resource materialResource = GD.Load(materialPath);
						if (materialResource == null)
						{
							GD.PrintErr($"Could not load material \"{materialPath}\"");
							return;
						}

						if (materialResource is StandardMaterial3D standardMaterial3D)
						{
							if (standardMaterial3D.AlbedoTexture != null)
							{
								textureWidth = standardMaterial3D.AlbedoTexture.GetWidth();
								textureHeight = standardMaterial3D.AlbedoTexture.GetHeight();
							}
						}
						else if (materialResource is ShaderMaterial shaderMaterial)
						{
							Variant variant = shaderMaterial.GetShaderParameter("_AlbedoTexture");
							Texture2D texture = (Texture2D)variant;
							if (texture != null)
							{
								textureWidth = texture.GetWidth();
								textureHeight = texture.GetHeight();
							}
						}

						surface = new()
						{
							material = materialResource as Material,
							textureWidth = textureWidth,
							textureHeight = textureHeight
						}; 

						surfaces.Add(plane.textureName, surface);
					}
					
					foreach (Vector3 vertex in plane.vertices)
					{
						//apply scale and axis conversion
						Vector3 convertedVertex = ConvertAxisAndScale(vertex);
						surface.vertices.Add(convertedVertex);
						surface.uvs.Add(plane.CalculateUv(vertex, surface.textureWidth, surface.textureHeight));
						surface.tangents.Add(plane.tangent.X);
						surface.tangents.Add(plane.tangent.Y);
						surface.tangents.Add(plane.tangent.Z);
						surface.tangents.Add(plane.tangent.W);
					}

					if (!collisionOnlyTextures.Contains(plane.textureName))
						for(int i = 0; i < plane.indices.Length; i++)
							surface.indices.Add(plane.indices[i] + surface.indexOffset);

					if (!noCollisionTextures.Contains(plane.textureName))
						for(int i = 0; i < plane.indices.Length; i++)
							surface.collisionIndices.Add(plane.indices[i] + surface.indexOffset);

					surface.indexOffset += plane.vertices.Count;
					
					//apply axis conversion
					Vector3 convertedNormal = ConvertAxis(plane.normal);
					for(int i = 0; i < plane.vertices.Count; i++)
						surface.normals.Add(convertedNormal);
				}
			}
		}

		private class BrushSurface
		{
			public int indexOffset = 0;
			public List<Vector3> vertices = [];
			public List<int> indices = [];
			public List<Vector3> normals = [];
			public List<Vector2> uvs = [];
			public List<float> tangents = [];

			public Material material;
			public int textureWidth;
			public int textureHeight;

			public List<int> collisionIndices = [];

			public void SmoothNormals()
			{
				const float weldDistance = 0.001f;

				for(int i = 0; i < vertices.Count; i++)
				{
					int count = 1;
					Vector3 accumulate = normals[i];

					for(int j = 0; j < vertices.Count; j++)
					{
						if (i == j)
							continue;

						if (vertices[i].DistanceTo(vertices[j]) <= weldDistance)
						{
							accumulate += normals[j];
							count++;
						}
					}

					normals[i] = accumulate.Normalized();
				}
			}
		}

		private class ConcaveObject
		{
			public Dictionary<string, BrushSurface> surfaces = [];
			public TrenchEntity ownerEntity;

			public void CalculateBounds(out Vector3 min, out Vector3 max)
			{
				min = new(float.MaxValue,float.MaxValue,float.MaxValue);
				max = new(float.MinValue,float.MinValue,float.MinValue);

				foreach (KeyValuePair<string, BrushSurface> surface in surfaces)
					foreach(Vector3 vertex in surface.Value.vertices)
					{
						min.X = Mathf.Min(vertex.X, min.X);
						max.X = Mathf.Max(vertex.X, max.X);
						min.Y = Mathf.Min(vertex.Y, min.Y);
						max.Y = Mathf.Max(vertex.Y, max.Y);
						min.Z = Mathf.Min(vertex.Z, min.Z);
						max.Z = Mathf.Max(vertex.Z, max.Z);
					}
			}
		}

		private class ConvexObject
		{
			public Dictionary<string, BrushSurface> surfaces = [];
			public List<Vector3> collisionVertices = [];
			public TrenchEntity ownerEntity;
			public bool useRigidBody = false;

			public void CalculateBounds(out Vector3 min, out Vector3 max)
			{
				min = new(float.MaxValue,float.MaxValue,float.MaxValue);
				max = new(float.MinValue,float.MinValue,float.MinValue);

				foreach(Vector3 vertex in collisionVertices)
				{
					min.X = Mathf.Min(vertex.X, min.X);
					max.X = Mathf.Max(vertex.X, max.X);
					min.Y = Mathf.Min(vertex.Y, min.Y);
					max.Y = Mathf.Max(vertex.Y, max.Y);
					min.Z = Mathf.Min(vertex.Z, min.Z);
					max.Z = Mathf.Max(vertex.Z, max.Z);
				}
			}
		}

		private class ConvexCollider
		{
			public List<Vector3> collisionVertices = [];
			public TrenchEntity ownerEntity;

			public void CalculateBounds(out Vector3 min, out Vector3 max)
			{
				min = new(float.MaxValue,float.MaxValue,float.MaxValue);
				max = new(float.MinValue,float.MinValue,float.MinValue);

				foreach(Vector3 vertex in collisionVertices)
				{
					min.X = Mathf.Min(vertex.X, min.X);
					max.X = Mathf.Max(vertex.X, max.X);
					min.Y = Mathf.Min(vertex.Y, min.Y);
					max.Y = Mathf.Max(vertex.Y, max.Y);
					min.Z = Mathf.Min(vertex.Z, min.Z);
					max.Z = Mathf.Max(vertex.Z, max.Z);
				}
			}
		}

		private class TrenchEntity
		{
			public Dictionary<string,string> properties = [];
			public List<TrenchBrush> brushes = [];
			public PivotType pivotType = PivotType.Root;

			public enum PivotType : int 
			{
				Root = 0,
				Center = 1,
				Bottom = 2,
				Top = 3
			}

			public static List<string> ParsePropertyLine(string input)
			{
				List<string> properties = [];
				Regex regex = new("\"([^\"]*)\"");
				MatchCollection matches = regex.Matches(input);

				foreach (Match match in matches)
					if (match.Groups.Count > 1)
						properties.Add(match.Groups[1].Value);

				return properties;
			}
		}

		private class BrushPlane
		{
			public Vector3 point1;
			public Vector3 point2;
			public Vector3 point3;
			public string textureName;
			public Vector3 uAxis;
			public float uOffset;
			public Vector3 vAxis;
			public float vOffset;
			public float rotation;
			public float Uscale;
			public float Vscale;

			public void CalculateDerivatives()
			{
            	Vector3 a = point2 - point1;
            	Vector3 b = point3 - point2;
            	normal = b.Cross(a).Normalized();
				distance = normal.Dot(point1);

				Vector3 u = uAxis.Normalized();
				Vector3 v = vAxis.Normalized();
				float vSign = Mathf.Sign(normal.Cross(u).Dot(v));
				tangent = new(u.X, u.Y, u.Z, vSign);
			}

			public Vector3 normal;
			public float distance;
			public Vector4 tangent;

			public List<Vector3> vertices = [];
			public int[] indices;

			public Vector2 CalculateUv(Vector3 vertex, int textureWidth, int textureHeight) 
			{
				Vector2 uvOut = new(uAxis.Dot(vertex), vAxis.Dot(vertex));

				uvOut.X /= textureWidth;
				uvOut.Y /= textureHeight;

				uvOut.X /= Uscale;
				uvOut.Y /= Vscale;

				uvOut.X += uOffset / textureWidth;
				uvOut.Y += vOffset / textureHeight;

				return uvOut;
			}

			private const string BrushPlanePattern = @"^\s*\(\s*(?<x1>-?\d+(\.\d+)?)\s+(?<y1>-?\d+(\.\d+)?)\s+(?<z1>-?\d+(\.\d+)?)\s*\)\s*\(\s*(?<x2>-?\d+(\.\d+)?)\s+(?<y2>-?\d+(\.\d+)?)\s+(?<z2>-?\d+(\.\d+)?)\s*\)\s*\(\s*(?<x3>-?\d+(\.\d+)?)\s+(?<y3>-?\d+(\.\d+)?)\s+(?<z3>-?\d+(\.\d+)?)\s*\)\s*(?<textureName>\w+)\s*\[\s*(?<Ux>-?\d+(\.\d+)?)\s+(?<Uy>-?\d+(\.\d+)?)\s+(?<Uz>-?\d+(\.\d+)?)\s+(?<Uoffset>-?\d+(\.\d+)?)\s*\]\s*\[\s*(?<Vx>-?\d+(\.\d+)?)\s+(?<Vy>-?\d+(\.\d+)?)\s+(?<Vz>-?\d+(\.\d+)?)\s+(?<Voffset>-?\d+(\.\d+)?)\s*\]\s*(?<rotation>-?\d+(\.\d+)?)\s+(?<Uscale>-?\d+(\.\d+)?)\s+(?<Vscale>-?\d+(\.\d+)?)\s*$";

			public static BrushPlane Parse(string input)
			{
				Match match = Regex.Match(input, BrushPlanePattern);

				if (!match.Success)
				{
					GD.PrintErr("Input string does not match the expected format.");
					return null;
				}

				return new BrushPlane()
				{
					point1 = new(
						float.Parse(match.Groups["x1"].Value),
						float.Parse(match.Groups["y1"].Value),
						float.Parse(match.Groups["z1"].Value)
					),
					point2 = new(
						float.Parse(match.Groups["x2"].Value),
						float.Parse(match.Groups["y2"].Value),
						float.Parse(match.Groups["z2"].Value)
					),
					point3 = new(
						float.Parse(match.Groups["x3"].Value),
						float.Parse(match.Groups["y3"].Value),
						float.Parse(match.Groups["z3"].Value)
					),
					textureName = match.Groups["textureName"].Value,
					uAxis = new(
						float.Parse(match.Groups["Ux"].Value),
						float.Parse(match.Groups["Uy"].Value),
						float.Parse(match.Groups["Uz"].Value)
					),
					uOffset = float.Parse(match.Groups["Uoffset"].Value),
					vAxis = new(
						float.Parse(match.Groups["Vx"].Value),
						float.Parse(match.Groups["Vy"].Value),
						float.Parse(match.Groups["Vz"].Value)
					),
					vOffset = float.Parse(match.Groups["Voffset"].Value),
					rotation = float.Parse(match.Groups["rotation"].Value),
					Uscale = float.Parse(match.Groups["Uscale"].Value),
					Vscale = float.Parse(match.Groups["Vscale"].Value)
				};
			}

            public override string ToString()
				=> $"( {point1.X} {point1.Y} {point1.Z} ) ( {point2.X} {point2.Y} {point2.Z} ) ( {point3.X} {point3.Y} {point3.Z} ) {textureName} [ {uAxis.X} {uAxis.Y} {uAxis.Z} {uOffset} ] [ {vAxis.X} {vAxis.Y} {vAxis.Z} {vOffset} ] {rotation} {Uscale} {Vscale}";
        }

		private class TrenchBrush
		{
			private const float epsilon = 0.008f;

			public List<BrushPlane> planes = [];

			public void Triangulate()
			{
				//generate
				for (int p0 = 0; p0 < planes.Count; p0++)
					for (int p1 = 0; p1 < planes.Count; p1++)
						for (int p2 = 0; p2 < planes.Count; p2++)
						{
							Vector3? vertex = IntersectFaces(planes[p0], planes[p1], planes[p2]);
							
							if (vertex == null || !ContainsPoint(vertex.Value)) 
								continue;

							planes[p0].vertices.Add(vertex.Value);
						}
				
				//wind
				for (int p = 0; p < planes.Count; p++)
				{
					BrushPlane plane = planes[p];

					if (plane.vertices.Count < 3) 
						continue;

					Vector3 windFaceBasis = (plane.vertices[1] - plane.vertices[0]).Normalized();
					Vector3 windFaceCenter = Vector3.Zero;
					Vector3 windFaceNormal = plane.normal;

					foreach(Vector3 vertex in plane.vertices)
						windFaceCenter += vertex;
					windFaceCenter /= plane.vertices.Count;

                    Vector3[] _vertexArray = [.. plane.vertices];
					Array.Sort(_vertexArray, (l, r) =>
					{
						Vector3 u = windFaceBasis.Normalized();
						Vector3 v = u.Cross(windFaceNormal).Normalized();

						Vector3 loc_a = l - windFaceCenter;
						float a_pu = loc_a.Dot(u);
						float a_pv = loc_a.Dot(v);

						Vector3 loc_b = r - windFaceCenter;
						float b_pu = loc_b.Dot(u);
						float b_pv = loc_b.Dot(v);

						float a_angle = Mathf.Atan2(a_pv, a_pu);
						float b_angle = Mathf.Atan2(b_pv, b_pu);

						if (a_angle == b_angle) 
							return 0;
						
						return a_angle > b_angle ? 1 : -1;
					});

					for(int i = 0; i < plane.vertices.Count; i++)
						plane.vertices[i] = _vertexArray[i];
				}

				//index
				foreach(BrushPlane plane in planes)
				{
					if (plane.vertices.Count < 3)
					{
						GD.PrintErr("Plane with less than 3 vertices");
						continue;
					}

					plane.indices = new int[(plane.vertices.Count - 2) * 3];

					for (int i = 0, p = 0; i < plane.vertices.Count - 2; i++)
					{
						plane.indices[p++] = 0;
						plane.indices[p++] = i+1;
						plane.indices[p++] = i+2;
					}
				}
			}

			private bool ContainsPoint(Vector3 point)
			{
				foreach(BrushPlane plane in planes)
				{
					float p = plane.normal.Dot(point);
					
					if (p > plane.distance && Mathf.Abs(plane.distance - p) > epsilon) 
						return false;
				}

				return true;
			}

			private static Vector3? IntersectFaces(BrushPlane p0, BrushPlane p1, BrushPlane p2)
			{
				Vector3 n0 = p0.normal;
				Vector3 n1 = p1.normal;
				Vector3 n2 = p2.normal;

				float d = n2.Dot(n0.Cross(n1));
				
				if (d < epsilon) 
					return null;

				Vector3 a = n1.Cross(n2) * p0.distance;
				Vector3 b = n2.Cross(n0) * p1.distance;
				Vector3 c = n0.Cross(n1) * p2.distance;
				
				return (a+b+c)/d;
			}
		}
	}
}