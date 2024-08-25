using Godot;
using System;

namespace OpenARPG
{
    [Tool]
    public partial class Weather : Node
    {
        [ExportGroup("Ambient Light")]
        [Export] private Vector3 ambientDirection = new Vector3(1f,2f,1f).Normalized();
        [Export] private float ambientIntensity = 1f;
        [Export] private Texture2D ambientGradient = null;
        
        [ExportGroup("Distance Fog")]
        [Export] private Vector2 distanceFogRange = new Vector2(5f,10f);
        [Export] private Texture2D distanceFogGradient = null;

        public override void _Ready()
        {
            if (!OS.HasFeature("editor"))
            {
                SetProcess(false);
                UpdateWeather();
            }
        }

        public override void _Process(double _delta)
        {
            if (OS.HasFeature("editor"))
                UpdateWeather();
        }

        private void UpdateWeather()
        {
            RenderingServer.GlobalShaderParameterSet("AmbientDirection", ambientDirection);
            RenderingServer.GlobalShaderParameterSet("AmbientIntensity", ambientIntensity);
            RenderingServer.GlobalShaderParameterSet("AmbientGradient", ambientGradient);
            RenderingServer.GlobalShaderParameterSet("DistanceFogRange", distanceFogRange);
            RenderingServer.GlobalShaderParameterSet("DistanceFogGradient", distanceFogGradient);
        }
    }
}