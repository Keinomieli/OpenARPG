using Godot;
using System;
using System.Collections.Generic;

namespace OpenARPG
{
    public partial class DebugDraw2D : Node2D
    {
        public static Action<Vector2, Vector2, Color, float, bool> Line {get; private set;} = (from, to, color, width, antiAlias) => { };
        public static Action<Vector2, float, Color, bool, float, bool> Circle {get; private set;} = (position, radius, color, filled, width, antiAlias) => { };

        bool dirty = false;
        List<DebugDrawObject2D> objectsToDraw = new List<DebugDrawObject2D>();

        private abstract class DebugDrawObject2D { }

        private class DebugLine2D: DebugDrawObject2D
        {
            public Vector2 from;
            public Vector2 to;
            public Color color = Colors.White;
            public float width = -1f;
            public bool antiAlias = false;
        }

        private class DebugCircle2D: DebugDrawObject2D
        {
            public Vector2 position;
            public float radius;
            public Color color = Colors.White;
            public bool filled = true;
            public float width = -1f;
            public bool antiAlias = false;
        }

        public override void _EnterTree()
        {
            if (OS.HasFeature("release"))
            {
                QueueFree();
                return;
            }

            Line = (From, To, Color, Width, AntiAlias) => 
            { 
                objectsToDraw.Add(new DebugLine2D()
                {
                    from = From,
                    to = To,
                    color = Color,
                    width = Width,
                    antiAlias = AntiAlias
                });

                dirty = true;
            };

            Circle = (Position, Radius, Color, Filled, Width, AntiAlias) => 
            { 
                objectsToDraw.Add(new DebugCircle2D()
                {
                    position = Position,
                    radius = Radius,
                    color = Color,
                    filled = Filled,
                    width = Width,
                    antiAlias = AntiAlias
                });

                dirty = true;
            };
        }

        public override void _Process(double delta)
        {
            if (!dirty)
                return;

            QueueRedraw();
            dirty = false;
        }

        public override void _Draw()
        {
            if (objectsToDraw.Count == 0)
                return;

            foreach(DebugDrawObject2D debugDrawObject in objectsToDraw)
            {
                switch (debugDrawObject)
                {
                    default:
                        Logger.Error($"DebugDraw2D: DrawObject: Unrecognized debug draw object type \"{debugDrawObject.GetType()}\"");
                    break;

                    case DebugLine2D l:
                        DrawLine(l.from, l.to, l.color, l.width, l.antiAlias);
                    break;

                    case DebugCircle2D c:
                        DrawCircle(c.position, c.radius, c.color, c.filled, c.width, c.antiAlias);
                    break;
                };
            }

            objectsToDraw.Clear();
        }
    }
}