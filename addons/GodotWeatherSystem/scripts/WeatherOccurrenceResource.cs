using Godot;
using System;

[GlobalClass]
// Weather occurrence: Basically just warps a WeatherResource, and has a weighted probability ratio of the weather to occur.
public partial class WeatherOccurrenceResource : Resource
{
    [Export]
    public WeatherResource weather;

    [Export(PropertyHint.Range, "0.0,1.0,")]
    // Weighted probability ratio of the specified weather (Set to a lower value if you want the weather to occur rarely)
    public float probabilityRatio = 1.0f;
}
