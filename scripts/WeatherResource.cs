using Godot;
using System;

[GlobalClass]
public partial class WeatherResource : Resource
{
    [Export]
    // Minimum duration, in seconds
    public double minDuration = 40000.0;
    [Export]
    // Maximum duration, in seconds
    public double maxDuration = 100000.0;
    [Export]
    // How fast the clouds move
    public float cloudSpeed = 0.001f;
    [Export(PropertyHint.Range, "0.0,1.0,")]
    public float smallCloudCover = 0.5f;
    [Export(PropertyHint.Range, "0.0,1.0,")]
    public float largeCloudCover = 0.5f;
    [Export]
    public Color cloudInnerColour = new Color(1.0f, 1.0f, 1.0f);
    [Export]
    public Color cloudOuterColour = new Color(0.5f, 0.5f, 0.5f);
    [Export]
    public PrecipitationResource precipitation;
    [Export]
    public float fogDensity = 0.0f;
}
