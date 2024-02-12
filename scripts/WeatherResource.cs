using Godot;
using System;

[GlobalClass]
public partial class WeatherResource : Resource
{
    [Export]
    public double minDuration = 40000.0;
    [Export]
    public double maxDuration = 100000.0;
    [Export]
    public float cloudSpeed = 0.001f;
    [Export]
    public float cloudCover = 1.0f;
    [Export]
    public Color cloudInnerColour = new Color(1.0f, 1.0f, 1.0f);
    [Export]
    public Color cloudOuterColour = new Color(0.5f, 0.5f, 0.5f);
}
