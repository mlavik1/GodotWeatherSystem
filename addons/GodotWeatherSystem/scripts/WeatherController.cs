using Godot;
using System;

[GlobalClass]
public partial class WeatherController : Node
{
	[Export]
	public WorldEnvironment worldEnvironment;

	[Export]
	public DirectionalLight3D directionalLight;

	[Export]
	public Godot.Collections.Array<SeasonResource> seasons;

	[Export]
	public double dayDuration = 86400.0;

	[Export]
	public double timeSpeedMultiplier = 100.0;

	[Export]
	public double timeOfDay = 40000.0;

	[Export]
	public int startSeason = 0;

	[Export]
	public int startWeather = 0;
	
	private int currentSeasonIndex = 0;
	private int currentWeatherIndex = 0;
	private int nextWeatherIndex = 0;
	private double currentWeatherLength = 0.0;
	private double currentWeatherTime = 0.0;
	private double currentSeasonLength = 0.0;
	private double currentSeasonTime = 0.0;
	private GpuParticles3D particleSystem;

	public void SetSeason(int seasonIndex, int weatherIndex = -1)
	{
		if (seasons.Count == 0)
			return;
		currentSeasonIndex = seasonIndex;
		currentSeasonLength = seasons[seasonIndex].durationInDays * dayDuration;
		currentSeasonTime = 0.0f;

		SetWeather(weatherIndex != -1 ? weatherIndex : Random.Shared.Next() % seasons[seasonIndex].weathers.Count);
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
		if (worldEnvironment == null)
		{
			GD.PrintErr("WeatherController.worldEnvironment is null! Please assign it.");
		}
		if (directionalLight == null)
		{
			GD.PrintErr("WeatherController.directionalLight is null! Please assign it.");
		}

		SetSeason(startSeason, startWeather);
	}

	public override void _Process(double delta)
	{
		if (worldEnvironment == null)
			return;

		if (seasons.Count == 0)
			return;

		SeasonResource season = seasons[currentSeasonIndex];
		if (season.weathers.Count == 0)
			return;
		
		if (currentSeasonTime >= currentSeasonLength)
		{
			SetSeason(currentSeasonIndex + 1);
		}

		timeOfDay = (timeOfDay + delta * timeSpeedMultiplier) % dayDuration;

		currentWeatherTime += delta * timeSpeedMultiplier;
		if (currentWeatherTime >= currentWeatherLength)
			SetWeather(nextWeatherIndex);
		
		float tTimeOfDay = (float)timeOfDay / (float)dayDuration;
		tTimeOfDay = 1.0f - season.dayNightCycleCurve.Sample(tTimeOfDay);

		WeatherResource weatherA = season.weathers[currentWeatherIndex];
		WeatherResource weatherB = season.weathers[nextWeatherIndex];
		double tWeatherTime = currentWeatherTime / currentWeatherLength;
		
		float fogDensity = Mathf.Lerp(weatherA.fogDensity, weatherB.fogDensity, (float)tWeatherTime);
		float cloudSpeed = Mathf.Lerp(weatherA.cloudSpeed, weatherB.cloudSpeed, (float)tWeatherTime);
		float smallCloudCover = Mathf.Lerp(weatherA.smallCloudCover, weatherB.smallCloudCover, (float)tWeatherTime);
		float largeCloudCover = Mathf.Lerp(weatherA.largeCloudCover, weatherB.largeCloudCover, (float)tWeatherTime);
		// TODO: LERP season properties (sky colour, etc.)
		SkyColourResource skyColourDaytime = season.skyColourDaytime;
		SkyColourResource skyColourNight = season.skyColourNight;
		Color skyColour = skyColourDaytime.skyColour.Lerp(skyColourNight.skyColour, tTimeOfDay);
		Color horizonColour = skyColourDaytime.horizonColour.Lerp(skyColourNight.horizonColour, tTimeOfDay);
		Color groundColour = skyColourDaytime.groundColour.Lerp(skyColourNight.groundColour, tTimeOfDay);
		float cloudBrightness = Mathf.Lerp(skyColourDaytime.cloudBrightness, skyColourNight.cloudBrightness, tTimeOfDay);
		Color cloudInnerColour = weatherA.cloudInnerColour.Lerp(weatherB.cloudInnerColour, (float)tWeatherTime) * cloudBrightness;
		Color cloudOuterColour = weatherA.cloudOuterColour.Lerp(weatherB.cloudOuterColour, (float)tWeatherTime) * cloudBrightness;

		ShaderMaterial skyMaterial = worldEnvironment.Environment.Sky.SkyMaterial as ShaderMaterial;
		skyMaterial.SetShaderParameter("small_cloud_cover", smallCloudCover);
		skyMaterial.SetShaderParameter("large_cloud_cover", largeCloudCover);
		skyMaterial.SetShaderParameter("cloud_speed", cloudSpeed);
		skyMaterial.SetShaderParameter("cloud_shape_change_speed", cloudSpeed);
		skyMaterial.SetShaderParameter("cloud_inner_colour", cloudInnerColour);
		skyMaterial.SetShaderParameter("cloud_outer_colour", cloudOuterColour);
		skyMaterial.SetShaderParameter("sky_top_color", skyColour);
		skyMaterial.SetShaderParameter("sky_horizon_color", horizonColour);
		skyMaterial.SetShaderParameter("ground_horizon_color", horizonColour);
		skyMaterial.SetShaderParameter("ground_bottom_color", groundColour);

		worldEnvironment.Environment.VolumetricFogEnabled = fogDensity > 0.0f;
		worldEnvironment.Environment.VolumetricFogDensity = fogDensity;

		if (particleSystem != null)
		{
			float particleAmountRatio = Mathf.Lerp(
				(weatherA.precipitation != null ? weatherA.precipitation.amountRatio : 0.0f),
				(weatherB.precipitation != null ? weatherB.precipitation.amountRatio : 0.0f),
				(float)tWeatherTime
			);
			particleSystem.AmountRatio = particleAmountRatio;
		}

		if (directionalLight != null)
		{
			float tSunAngle = (float)timeOfDay / (float)dayDuration;
			directionalLight.GlobalRotation = new Vector3(tSunAngle * 2.0f * Mathf.Pi + Mathf.Pi * 0.5f, 0.0f, 0.0f);
			directionalLight.LightEnergy = 1.0f - tTimeOfDay;
		}
		worldEnvironment.Environment.AmbientLightSkyContribution = tTimeOfDay;
	}
}
