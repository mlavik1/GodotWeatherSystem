using Godot;
using System;

[GlobalClass]
public partial class SeasonResource : Resource
{
    [Export]
	public Godot.Collections.Array<WeatherResource> weathers;
    [Export]
    public SkyColourResource skyColourDaytime;
    [Export]
    public SkyColourResource skyColourNight;
    [Export]
    public Curve dayNightCycleCurve;
}
