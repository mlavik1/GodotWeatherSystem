# Godot Weather System

A customisable season/weather system for Godot, made in C#.

**Features:**
- Day-night cycles
- Random weather transitions (weighted randomness)
- Weather settings:
  - Clouds (coverage, colour)
  - Rain (precipitation)
  - Fog
  - Probability of weather to appear
- Season settings
  - Adjust day/night length with curve
  - Specify different types of weather for each season

![screenshot](screenshots/screenshot2.jpg)

## How to use

Quick start:
1. Open the [sample scene](sample_scene/sample_scene.tscn) in Godot
2. Adjust the settings on the "WeatherController" object to your liking
3. Click play

To use it in your own project you can either:
A. Copy the "WeatherController" object from the sample scene over to your own scene
or
B. Set up the weather related nodes yourself:
1. Copy [world_environment.tscn](addons/GodotWeatherSystem/nodes/world_environment.tscn) to your scene (you need to use this one, since it's using a custom shader)
2. Add a directional light
3. Add a WeatherController node (available through the "add node" menu after building with MSBuild)
4. In the WeatherController's inspector, link your directional light and world environment

### Weather Controller settings

The weather controller has the following settings:
- World Environment: Link to the WorldEnvironment object. Note: you should use [world_environment.tscn](addons/GodotWeatherSystem/nodes/world_environment.tscn), since it uses a custom shader
- Directional Light:
- Seasons: List of SeasonResource objects
- Day Duration: How long a day is, in seconds (controls the day-night cycles)
- Time Speed Multiplier: How fast time moves (100 for 100s per second, etc.)
- Start time: Time at the beginning of the game (set to dayDuration/2 for daytime, or it will start at midnight - if set to 0)
- Start Season: Index of the season (in the season list) to use at game start
- Start Weather: Index of the weather (in the weather list) to use at game start. Set it to -1 if you want it to be random.

![screenshot](screenshots/weather_controller_settings.jpg)

### Season settings (SeasonResource)

The first thing you want to do is to create some seaons.
These controll what types of weather you will see (summer => clear sky, autumn => rain, etc.), and how long the nights are.

You can create a season settings either by adding a new entry to the WeatherController's weather list and clicking "New SeasonResource", or by right clicking anywhere in Godot's file system and clicking "new resource", and then choose "SeasonResource".

![screenshot](screenshots/new_season.jpg)

**Settings:**
- Weathers: List of available weathers (WeatherOccurrenceResource) for this season
- Duration in days: How many days the season lasts
- Sky colour daytime: How the sky looks at day time (SkyColourResource)
- Sky colour night: How the sky looks at night (SkyColourResource)
- Day night cycle curve: Curve that maps daytime (X axis starting at 00:00) to sky brightness. Allows you to make days longer during summer, and nights longer during winter

![screenshot](screenshots/season_settings.jpg)

The weather list contains `WeatherOccurrenceResource` objects. These specify the probability of a given weather to occur, and have a "Probability Ratio" value between 0.0 and 1,0 (the higher the vlaue the more often they will appear).
You probably don't want to save WeatherOccurrenceResource to file, but instead just click "New WeatherOccurrenceResource" and embed them directly.

### Weather settings (WeatherResource)

The WeatherResource allows you to set up weather types, such as clear sky, clouded, foggy, rain, storm, etc.

Note: There are currently two types of clouds: "small" and "large clouds, and they have each their intensity/coverage settings.

**Settings:**
- Min duration: Minimum duration of season, in seconds (duration will be random, between set min/max values)
- Max duration: Maximum duration of season, in seconds (duration will be random, between set min/max values)
- Cloud speed: How fast the clouds moves
- Small cloud cover: How much "small" clouds there are (these look almost like a mackerel sky)
- Cloud cloud cover: How much "large" clouds there are (these clouds are thicker - you may want to use them for rainy weather and thunder storms)
- Percipitation: A reference to a PercipitationResource, that has settings for rain and other types of percipitation. There is a default [rain.tres](addons/GodotWeatherSystem/precipitation/rain.tres) that you can use out of the box.
- Fog density: Volumetric fog (you may want this on rainy days)

![screenshot](screenshots/weather_settings.jpg)

### Percipitation / rain (PercipitationResource)

These contain a reference to a particle system (scene), and an "amount ratio", that sets the intensity of the effect (how many particles - how much rain).

You can create your own particle effects. There's also a default [rain_particles.tscn](addons/GodotWeatherSystem/particles/rain_particles.tscn) that you can use.
