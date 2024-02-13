using Godot;
using System;

[GlobalClass]
public partial class SkyColourResource : Resource
{
    [Export]
    public Color skyColour = new Color(0.3f, 0.5f, 0.7f);
    [Export]
    public Color horizonColour = new Color(0.65f, 0.75f, 0.85f);
    [Export]
    public Color groundColour = new Color(0.35f, 0.65f, 0.85f);
    [Export]
    public float cloudBrightness = 1.0f;
}
