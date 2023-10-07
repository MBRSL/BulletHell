using UnityEngine;

/// <summary>
/// Use this class to transform continous movements to discrete movements
/// This speeds up training process and lower the difficulty
/// </summary>
public class Movement
{
    public readonly static Vector2[] ClockDirection = new Vector2[]
    {
        // 12'o clock        
        Vector2.up,
        // 1'o clock
        new Vector2(0.5f, 0.86602540378f),
        new Vector2(0.86602540378f, 0.5f),
        Vector2.right,
        new Vector2(0.86602540378f, -0.5f),
        new Vector2(0.5f, -0.86602540378f),
        Vector2.down,
        new Vector2(-0.5f, -0.86602540378f),
        new Vector2(-0.86602540378f, -0.5f),
        Vector2.left,
        new Vector2(-0.86602540378f, 0.5f),
        new Vector2(-0.5f, 0.86602540378f),
        // Special case for not moving
        Vector2.zero
    };
    public readonly static int Up = 0;
    public readonly static int Right = 3;
    public readonly static int Down = 6;
    public readonly static int Left = 9;
    public readonly static int NotMoving = 12;

    public static int ToClockDirection(Vector2 vector)
    {
        if (vector == Vector2.zero)
        {
            return NotMoving;
        }

        float angleRad = Mathf.Atan2(vector.y, vector.x);
        if (angleRad < 0)
        {
            angleRad += Mathf.PI*2;
        }
        var clockAngleRad = Mathf.PI*2 - angleRad + Mathf.PI/2;
        return (int)Mathf.Round(clockAngleRad/(Mathf.PI/6)) % 12;
    }

    public static Vector2 SnapToClockDirection(Vector2 vector)
    {
        var idx = ToClockDirection(vector);
        return ClockDirection[idx];
    }
}
/*
0 => PI/2
PI/2 => 0
PI => 3/2PI = -PI/2
-PI => 3/2PI
-PI/2 => PI

-90 => 270 => 180
90  => 90 => 0
0   => 0  => 90

-179 = 181
-1   = 359
*/