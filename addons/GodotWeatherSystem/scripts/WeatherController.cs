using Godot;
using System;

[GlobalClass]
public partial class WeatherController : Node
{
	private struct SkyColourParams
	{
		public Color skyColour;
		public Color horizonColour;
		public Color groundColour;
		public float cloudBrightness;
	}

	private struct SeasonParams
	{
		public double durationInDays;
		public SkyColourParams skyColourDaytime;
		public SkyColourParams skyColourNight;
		public Curve dayNightCycleCurve;
	}

	private struct WeatherParams
	{
		public float cloudSpeed;
		public float smallCloudCover;
		public float largeCloudCover;
		public Color cloudInnerColour;
		public Color cloudOuterColour;
		public float fogDensity;
		public float particleAmountRatio;
	}

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
	public double startTime = 40000.0f;

	[Export]
	public int startSeason = 0;

	[Export]
	public int startWeather = 0;

	public Action<SeasonResource> onSeasonChange;
	public Action<WeatherResource> onWeatherChange;
	
	private double timeOfDay = 0.0;
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

		if (weatherIndex != -1)
			SetWeather(weatherIndex);
		else
			SetRandomWeather();

		onSeasonChange(seasons[currentSeasonIndex]);
	}

	public void SetRandomWeather()
	{
		SeasonResource season = seasons[currentSeasonIndex];
		float total = 0.0f;
		foreach (WeatherOccurrenceResource weather in season.weathers)
			total += weather.probabilityRatio;
		float rand = Random.Shared.NextSingle() * total;
		total = 0.0f;
		int weatherIndex = 0;
		foreach (WeatherOccurrenceResource weather in season.weathers)
		{
			total += weather.probabilityRatio;
			if (rand <= total)
				break;			
			weatherIndex++;
		}
		SetWeather(weatherIndex);
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
		WeatherResource weather = season.weathers[currentWeatherIndex].weather;
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
		currentSeasonTime += startTime;
		timeOfDay += startTime;
	}

	public override void _Process(double delta)
	{
		if (worldEnvironment == null)
			return;

		if (seasons.Count == 0)
			return;

		timeOfDay = (timeOfDay + delta * timeSpeedMultiplier) % dayDuration;
		currentWeatherTime += delta * timeSpeedMultiplier;
		currentSeasonTime += delta * timeSpeedMultiplier;
		if (currentWeatherTime >= currentWeatherLength)
			SetWeather(nextWeatherIndex);
		
		SeasonResource season = seasons[currentSeasonIndex];
		SeasonResource nextSeason = seasons[(currentSeasonIndex + 1) % seasons.Count];
		if (season.weathers.Count == 0)
			return;
		
		if (currentSeasonTime >= currentSeasonLength)
		{
			SetSeason((currentSeasonIndex + 1) % seasons.Count);
		}

		SeasonParams seasonParams = InterpolateSeasons(season, nextSeason, (float)currentSeasonTime / (float)currentSeasonLength);
		WeatherParams weatherParams = InterpolateWeathers(season.weathers[currentWeatherIndex].weather, season.weathers[nextWeatherIndex].weather, (float)(currentWeatherTime / currentWeatherLength));

		float tTimeOfDay = (float)timeOfDay / (float)dayDuration;
		tTimeOfDay = Mathf.Clamp(1.0f - seasonParams.dayNightCycleCurve.Sample(tTimeOfDay), 0.0f, 1.0f);
		
		SkyColourParams skyColourDaytime = seasonParams.skyColourDaytime;
		SkyColourParams skyColourNight = seasonParams.skyColourNight;
		Color skyColour = skyColourDaytime.skyColour.Lerp(skyColourNight.skyColour, tTimeOfDay);
		Color horizonColour = skyColourDaytime.horizonColour.Lerp(skyColourNight.horizonColour, tTimeOfDay);
		Color groundColour = skyColourDaytime.groundColour.Lerp(skyColourNight.groundColour, tTimeOfDay);
		float cloudBrightness = Mathf.Lerp(skyColourDaytime.cloudBrightness, skyColourNight.cloudBrightness, tTimeOfDay);
		Color cloudInnerColour = weatherParams.cloudInnerColour * cloudBrightness;
		Color cloudOuterColour = weatherParams.cloudOuterColour * cloudBrightness;

		ShaderMaterial skyMaterial = worldEnvironment.Environment.Sky.SkyMaterial as ShaderMaterial;
		skyMaterial.SetShaderParameter("small_cloud_cover", weatherParams.smallCloudCover);
		skyMaterial.SetShaderParameter("large_cloud_cover", weatherParams.largeCloudCover);
		skyMaterial.SetShaderParameter("cloud_speed", weatherParams.cloudSpeed);
		skyMaterial.SetShaderParameter("cloud_shape_change_speed", weatherParams.cloudSpeed);
		skyMaterial.SetShaderParameter("cloud_inner_colour", cloudInnerColour);
		skyMaterial.SetShaderParameter("cloud_outer_colour", cloudOuterColour);
		skyMaterial.SetShaderParameter("sky_top_color", skyColour);
		skyMaterial.SetShaderParameter("sky_horizon_color", horizonColour);
		skyMaterial.SetShaderParameter("ground_horizon_color", horizonColour);
		skyMaterial.SetShaderParameter("ground_bottom_color", groundColour);

		worldEnvironment.Environment.VolumetricFogEnabled = true;//weatherParams.fogDensity > 0.0f;
		worldEnvironment.Environment.VolumetricFogDensity = weatherParams.fogDensity;

		if (particleSystem != null)
		{
			particleSystem.AmountRatio = weatherParams.particleAmountRatio;
			particleSystem.Transparency = Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(tTimeOfDay))); // TODO: Do something smart (this is just silly)
			particleSystem.GlobalPosition = GetViewport().GetCamera3D().GlobalPosition + Vector3.Up * 10.0f;
		}

		if (directionalLight != null)
		{
			float tSunAngle = (float)timeOfDay / (float)dayDuration;
			directionalLight.GlobalRotation = new Vector3(tSunAngle * 2.0f * Mathf.Pi + Mathf.Pi * 0.5f, 0.0f, 0.0f);
			directionalLight.LightEnergy = 1.0f - tTimeOfDay;
		}
		worldEnvironment.Environment.AmbientLightSkyContribution = tTimeOfDay;
	}

	private SeasonParams InterpolateSeasons(SeasonResource seasonA, SeasonResource seasonB, float t)
	{
		SeasonParams seasonParams;
		seasonParams.skyColourDaytime.skyColour = seasonA.skyColourDaytime.skyColour.Lerp(seasonB.skyColourDaytime.skyColour, t);
		seasonParams.skyColourDaytime.horizonColour = seasonA.skyColourDaytime.horizonColour.Lerp(seasonB.skyColourDaytime.horizonColour, t);
		seasonParams.skyColourDaytime.groundColour = seasonA.skyColourDaytime.groundColour.Lerp(seasonB.skyColourDaytime.groundColour, t);
		seasonParams.skyColourDaytime.cloudBrightness = Mathf.Lerp(seasonA.skyColourDaytime.cloudBrightness, seasonB.skyColourDaytime.cloudBrightness, t);
		seasonParams.skyColourNight.skyColour = seasonA.skyColourNight.skyColour.Lerp(seasonB.skyColourNight.skyColour, t);
		seasonParams.skyColourNight.horizonColour = seasonA.skyColourNight.horizonColour.Lerp(seasonB.skyColourNight.horizonColour, t);
		seasonParams.skyColourNight.groundColour = seasonA.skyColourNight.groundColour.Lerp(seasonB.skyColourNight.groundColour, t);
		seasonParams.skyColourNight.cloudBrightness = Mathf.Lerp(seasonA.skyColourNight.cloudBrightness, seasonB.skyColourNight.cloudBrightness, t);
		seasonParams.durationInDays = Mathf.Lerp(seasonA.durationInDays, seasonB.durationInDays, t);
		seasonParams.dayNightCycleCurve = seasonA.dayNightCycleCurve; // TODO: Interpolate curves?
		return seasonParams;
	}

	private WeatherParams InterpolateWeathers(WeatherResource weatherA, WeatherResource weatherB, float t)
	{
		WeatherParams weatherParams;
		weatherParams.fogDensity = Mathf.Lerp(weatherA.fogDensity, weatherB.fogDensity, t);
		weatherParams.cloudSpeed = Mathf.Lerp(weatherA.cloudSpeed, weatherB.cloudSpeed, t);
		weatherParams.smallCloudCover = Mathf.Lerp(weatherA.smallCloudCover, weatherB.smallCloudCover, t);
		weatherParams.largeCloudCover = Mathf.Lerp(weatherA.largeCloudCover, weatherB.largeCloudCover, t);
		weatherParams.cloudInnerColour = weatherA.cloudInnerColour.Lerp(weatherB.cloudInnerColour, t);
		weatherParams.cloudOuterColour = weatherA.cloudOuterColour.Lerp(weatherB.cloudOuterColour, t);
		weatherParams.cloudSpeed = Mathf.Lerp(weatherA.cloudSpeed, weatherB.cloudSpeed, t);
		weatherParams.particleAmountRatio = Mathf.Lerp(
			(weatherA.precipitation != null ? weatherA.precipitation.amountRatio : 0.0f),
			(weatherB.precipitation != null ? weatherB.precipitation.amountRatio : 0.0f),
			t
		);
		return weatherParams;
	}
}
