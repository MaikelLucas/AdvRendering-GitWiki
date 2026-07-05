using System;
using UnityEngine;

[Serializable]
public class Rectangle
{
    public float X, Y, Width, Height;

    public Rectangle(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Contains(Transform point)
    {
        return (point.position.x >= X - Width  / 2 &&
                point.position.x <  X + Width  / 2 &&
                point.position.y >= Y - Height / 2 &&
                point.position.y <  Y + Height / 2);
    }

    public bool Intersects(Rectangle other)
    {
        return !(other.X + other.Width / 2 < X - Width / 2 ||
                 other.X - other.Width / 2 > X + Width / 2 ||
                 other.Y + other.Height / 2 < Y - Height / 2 ||
                 other.Y - other.Height / 2 > Y + Height / 2);
    }
}