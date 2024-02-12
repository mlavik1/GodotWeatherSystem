using Godot;
using System;

[GlobalClass]
public partial class WeatherController : Node
{
	[Export]
	public WorldEnvironment worldEnvironment;

	[Export]
	public Godot.Collections.Array<SeasonResource> seasons;

	[Export]
	public double dayDuration = 600.0;

	[Export]
	public double timeSpeedMultiplier = 1.0;

	[Export]
	public double timeofDay = 300.0;

	private int currentSeasonIndex = 0;
	private int currentWeatherIndex = 0;
	private int nextWeatherIndex = 0;
	private double currentWeatherLength = 0.0;
	private double currentWeatherTime = 0.0;
	private GpuParticles3D particleSystem;

	public void SetSeason(int seasonIndex)
	{
		if (seasons.Count == 0)
			return;
		currentSeasonIndex = seasonIndex;

		SetWeather(2);
	}

	public void SetWeather(int weatherIndex)
	{
		if (seasons.Count == 0)
			return;

		SeasonResource season = seasons[currentSeasonIndex];
		if (season.weathers.Count == 0)
			return;

		currentWeatherIndex = weatherIndex;
		nextWeatherIndex = Random.Shared.Next() % season.weathers.Count;
		WeatherResource weather = season.weathers[currentWeatherIndex];
		currentWeatherLength = Mathf.Lerp(weather.minDuration, weather.maxDuration, Random.Shared.NextDouble());
		currentWeatherTime = 0.0;

		if (particleSystem != null)
		{
			RemoveChild(particleSystem);
			particleSystem = null;
		}
		if (weather.precipitation != null && weather.precipitation.particles != null)
		{
			particleSystem = weather.precipitation.particles.Instantiate<GpuParticles3D>();
			GD.Print(particleSystem);
			AddChild(particleSystem);
		}
	}

	public override void _Ready()
	{
		SetSeason(0);
	}

	public override void _Process(double delta)
	{
		if (seasons.Count == 0)
			return;

		SeasonResource season = seasons[currentSeasonIndex];
		if (season.weathers.Count == 0)
			return;

		timeofDay = (timeofDay + delta * timeSpeedMultiplier) % dayDuration;

		currentWeatherTime += delta * timeSpeedMultiplier;
		if (currentWeatherTime >= currentWeatherLength)
			SetWeather(nextWeatherIndex);
		
		float tTimeOfDay = (float)timeofDay / (float)dayDuration;
		tTimeOfDay = 1.0f - season.dayNightCycleCurve.Sample(tTimeOfDay);

		WeatherResource weatherA = season.weathers[currentWeatherIndex];
		WeatherResource weatherB = season.weathers[nextWeatherIndex];
		double tWeatherTime = currentWeatherTime / currentWeatherLength;
		
		float cloudSpeed = Mathf.Lerp(weatherA.cloudSpeed, weatherB.cloudSpeed, (float)tWeatherTime);
		float cloudCover = Mathf.Lerp(weatherA.cloudCover, weatherB.cloudCover, (float)tWeatherTime);
		SkyColourResource skyColourDaytime = season.skyColourDaytime;
		SkyColourResource skyColourNight = season.skyColourNight;
		Color skyColour = skyColourDaytime.skyColour.Lerp(skyColourNight.skyColour, tTimeOfDay);
		Color horizonColour = skyColourDaytime.horizonColour.Lerp(skyColourNight.horizonColour, tTimeOfDay);
		Color groundColour = skyColourDaytime.groundColour.Lerp(skyColourNight.groundColour, tTimeOfDay);
		float cloudBrightness = Mathf.Lerp(skyColourDaytime.cloudBrightness, skyColourNight.cloudBrightness, tTimeOfDay);
		Color cloudInnerColour = weatherA.cloudInnerColour.Lerp(weatherB.cloudInnerColour, (float)tWeatherTime) * cloudBrightness;
		Color cloudOuterColour = weatherA.cloudOuterColour.Lerp(weatherB.cloudOuterColour, (float)tWeatherTime) * cloudBrightness;

		ShaderMaterial skyMaterial = worldEnvironment.Environment.Sky.SkyMaterial as ShaderMaterial;
		skyMaterial.SetShaderParameter("cloud_cover", 1.0f / cloudCover);
		skyMaterial.SetShaderParameter("cloud_speed", cloudSpeed);
		skyMaterial.SetShaderParameter("cloud_shape_change_speed", cloudSpeed);
		skyMaterial.SetShaderParameter("cloud_inner_colour", cloudInnerColour);
		skyMaterial.SetShaderParameter("cloud_outer_colour", cloudOuterColour);
		skyMaterial.SetShaderParameter("sky_top_color", skyColour);
		skyMaterial.SetShaderParameter("sky_horizon_color", horizonColour);
		skyMaterial.SetShaderParameter("ground_horizon_color", horizonColour);
		skyMaterial.SetShaderParameter("ground_bottom_color", groundColour);
		//worldEnvironment.Environment.VolumetricFogEnabled = true;
		//worldEnvironment.Environment.VolumetricFogDensity = 0.05f;

		if (particleSystem != null)
		{
			float particleAmountRatio = Mathf.Lerp(
				(weatherA.precipitation != null ? weatherA.precipitation.amountRatio : 0.0f),
				(weatherB.precipitation != null ? weatherB.precipitation.amountRatio : 0.0f),
				(float)tWeatherTime
			);
			particleSystem.AmountRatio = particleAmountRatio;
			particleSystem.Transparency = Mathf.Clamp(Mathf.Sqrt(tTimeOfDay), 0.01f, 1.0f); // TODO: Do something smarter
		}
	}
}
