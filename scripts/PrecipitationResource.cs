using Godot;
using System;

[GlobalClass]
public partial class PrecipitationResource : Resource
{
    [Export]
	public PackedScene particles;
    [Export]
	public float amountRatio = 1.0f;
}
