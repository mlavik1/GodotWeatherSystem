using Godot;
using System;

[GlobalClass]
public partial class TerrainWeatherHandler : Node
{
	[Export]
    public WeatherController weatherController;

	[Export]
    public Node3D terrainNode;

	public override void _Ready()
	{
        weatherController.onSeasonChange += OnSeasonChange;
	}

	public override void _Process(double delta)
	{
	}

    private void OnSeasonChange(SeasonResource season)
    {
		Color groundColour = new Color(0.314f, 0.412f, 0.011f);
        if (season.seasonId == "winter")
			groundColour = new Color(0.9f, 0.9f, 0.9f);

		MeshInstance3D node = terrainNode.FindChild("Sphere") as MeshInstance3D;
		if (node != null)
		{
			StandardMaterial3D material = node.Mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
			material.AlbedoColor = groundColour;
		}
    }
}
