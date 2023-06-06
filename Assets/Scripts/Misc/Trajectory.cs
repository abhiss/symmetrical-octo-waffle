using UnityEngine;

// Made with help from Sebastion Lague's Kinematic Equations video:
// https://www.youtube.com/watch?v=IvT8hjy6q4o&t=909s
namespace SharedMath
{
    public readonly struct LaunchData
    {
        public readonly Vector3 InitalVelocity;
        public readonly float TravelTime;

        public LaunchData(Vector3 initialVelocity, float travelTime)
        {
            this.InitalVelocity = initialVelocity;
            this.TravelTime = travelTime;
        }
    }

    public static class Trajectory
    {
        public static LaunchData CalculateLaunchData(Vector3 originPosition, Vector3 targetPosition, float height)
        {
            // Using S U V A T
            // Goal: Return initial velocity
            // 3 Knowns: Height, horizontal displacement, vertical displacement
            // Find: Up, Down, Right Velocity

            Vector3 sRight = targetPosition - originPosition;
            float a = Physics.gravity.y;

            // Up (final velocity = 0)
            // vUp = sqrt(-2as)
            // tUp = sqrt(-2s/a)
            float vUp = Mathf.Sqrt(-2 * a * height);
            float tUp = Mathf.Sqrt(-2 * height / a);

            // Down (inital velocity = 0)
            // tDown = sqrt(2s/a)
            float tDown = Mathf.Sqrt(2 * (sRight.y - height) / a);

            // Time
            float t = tUp + tDown;

            // Right (acceleration = 0)
            // vRight = s/t
            Vector3 vRight = sRight / t;

            Vector3 initialVelocity = vRight;
            initialVelocity.y = vUp;

            return new LaunchData(initialVelocity, t);
        }

        public static Vector3[] GetTrajectoryPath(LaunchData launchData, Vector3 origin, int resolution)
        {
            Vector3[] points = new Vector3[resolution];
            points[0] = origin;
            for (int i = 1; i <= resolution; i++)
            {
                float timeStep = i / (float)resolution * launchData.TravelTime;

                // S = u * t
                Vector3 s = launchData.InitalVelocity * timeStep;
                // S += a * t^2 / 2
                s.y += Physics.gravity.y * timeStep * timeStep / 2.0f;

                points[i] = origin + s;
            }

            return points;
        }
    }
}
