using Godot;
using System;

namespace OpenARPG
{
    public partial class DebugDraw3D : Node3D
    {
        public static Action<Vector3, Vector3, Color, float> Line {get; private set;} = (from, to, color, seconds) => { };
        public static Action<Vector3, float, Color, float> Sphere {get; private set;} = (position, radius, color, seconds) => { };
        public static Action<Vector3, Vector3, Color, float> Box {get; private set;} = (position, size, color, seconds) => { };
        public static Action<Vector3, Vector3, Vector2, Color, float> Plane {get; private set;} = (position, normal, size, color, seconds) => { };
        public static Action<Vector3, Vector3, Vector2, Color, float> Quad {get; private set;} = (position, normal, size, color, seconds) => { };

        public override void _EnterTree()
        {
            if (OS.HasFeature("release"))
            {
                QueueFree();
                return;
            }

            Line = (from, to, color, seconds) => 
            { 
                DrawLine3D(from, to, color, seconds);
            };

            Sphere = (position, radius, color, seconds) =>
            {
                DrawSphere3D(position, radius, color, seconds);
            };

            Box = (position, size, color, seconds) =>
            {
                DrawBox3D(position, size, color, seconds);
            };

            Plane = (position, normal, size, color, seconds) =>
            {
                DrawPlane3D(position, normal, size, color, seconds);
            };

            Quad = (position, normal, size, color, seconds) =>
            {
                DrawQuad3D(position, normal, size, color, seconds);
            };
        }

        private void DrawLine3D(Vector3 from, Vector3 to, Color color, float seconds) 
        {
            MeshInstance3D meshInstance = new();
            ImmediateMesh immediateMesh = new();
            OrmMaterial3D material = new();

            meshInstance.Mesh = immediateMesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

            immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
            immediateMesh.SurfaceAddVertex(from);
            immediateMesh.SurfaceAddVertex(to);
            immediateMesh.SurfaceEnd();

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;
            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;

            if (color.A < 1f)
                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            else
                material.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;

            FinalCleanup(meshInstance, seconds);
        }

        private void DrawSphere3D(Vector3 position, float radius, Color color, float seconds)
        {
            MeshInstance3D meshInstance = new();
            SphereMesh sphereMesh = new();
            OrmMaterial3D material = new();

            meshInstance.Mesh = sphereMesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            meshInstance.Position = position;

            sphereMesh.Radius = radius;
            sphereMesh.Height = radius * 2f;
            sphereMesh.Material = material;

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;
            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;

            if (color.A < 1f)
                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            else
                material.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;

            FinalCleanup(meshInstance, seconds);
        }

        private void DrawBox3D(Vector3 position, Vector3 size, Color color, float seconds)
        {
            MeshInstance3D meshInstance = new();
            BoxMesh boxMesh = new();
            OrmMaterial3D material = new();

            meshInstance.Mesh = boxMesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            meshInstance.Position = position;

            boxMesh.Size = size;
            boxMesh.Material = material;

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;
            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;

            if (color.A < 1f)
                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            else
                material.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;

            FinalCleanup(meshInstance, seconds);
        }

        private void DrawQuad3D(Vector3 position, Vector3 normal, Vector2 size, Color color, float seconds)
        {
            MeshInstance3D meshInstance = new();
            QuadMesh quadMesh = new();
            OrmMaterial3D material = new();

            meshInstance.Mesh = quadMesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            meshInstance.Position = position;
            meshInstance.Basis = Basis.LookingAt(normal);

            quadMesh.Size = size;
            quadMesh.Material = material;

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;
            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;

            if (color.A < 1f)
                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            else
                material.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;

            FinalCleanup(meshInstance, seconds);
        }

        private void DrawPlane3D(Vector3 position, Vector3 normal, Vector2 size, Color color, float seconds)
        {
            MeshInstance3D meshInstance = new();
            PlaneMesh planeMesh = new();
            OrmMaterial3D material = new();

            meshInstance.Mesh = planeMesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            meshInstance.Position = position;
            meshInstance.Basis = Basis.LookingAt(normal);

            planeMesh.Size = size;
            planeMesh.Material = material;

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;
            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;

            if (color.A < 1f)
                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            else
                material.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;

            FinalCleanup(meshInstance, seconds);
        }

        private async void FinalCleanup(MeshInstance3D meshInstance, float seconds)
        {
            GetTree().Root.AddChild(meshInstance);
            
            if (seconds <= 0)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //lasts for one frame
                meshInstance.QueueFree();
            }
            else
            {
                await ToSignal(GetTree().CreateTimer(seconds), "timeout");
                meshInstance.QueueFree();
            }
        }
    }
}