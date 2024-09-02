using Godot;
using System;

namespace OpenARPG
{
    public static partial class AxMath
    {
        //decay is typically from 1 to 25, from slow to fast (good default is 16.0f)
        public static float ExpDecay(float a, float b, float decay, float delta) 
            => b + (a - b) * MathF.Exp(-decay * delta);

        public static Vector2 ExpDecay(Vector2 a, Vector2 b, float decay, float delta) 
            => b + (a - b) * MathF.Exp(-decay * delta);

        public static Vector3 ExpDecay(Vector3 a, Vector3 b, float decay, float delta) 
            => b + (a - b) * MathF.Exp(-decay * delta);
    }
}