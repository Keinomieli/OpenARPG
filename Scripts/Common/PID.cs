using Godot;

namespace OpenARPG
{
    public class PID
    {
        public float value {get; private set;}

        private float p, i, d;
        private float integral;
        private float lastError;

        public PID(float P, float I, float D)
        {
            p = P;
            i = I;
            d = D;
        }

        public PID(Vector3 pid)
        {
            p = pid.X;
            i = pid.Y;
            d = pid.Z;
        }

        public void Update(float current, float target, float delta)
        {
            float present = target - current;
            integral += present * delta;
            float derivative = (present - lastError) / delta;
            lastError = present;
            value = present * p + integral * i + derivative * d;
        }
    }
}