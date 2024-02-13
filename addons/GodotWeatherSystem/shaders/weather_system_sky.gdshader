
// Shader inspired by https://www.youtube.com/watch?v=XKz0S2GU068

shader_type sky;
render_mode use_debanding;

uniform vec4 sky_top_color : source_color = vec4(0.385, 0.454, 0.55, 1.0);
uniform vec4 sky_horizon_color : source_color = vec4(0.646, 0.656, 0.67, 1.0);
uniform float sky_curve : hint_range(0, 1) = 0.15;
uniform float sky_energy = 1.0; // In Lux.
uniform sampler2D sky_cover : filter_linear, source_color, hint_default_black;
uniform vec4 sky_cover_modulate : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec4 ground_bottom_color : source_color = vec4(0.2, 0.169, 0.133, 1.0);
uniform vec4 ground_horizon_color : source_color = vec4(0.646, 0.656, 0.67, 1.0);
uniform float ground_curve : hint_range(0, 1) = 0.02;
uniform float ground_energy = 1.0;
uniform float sun_angle_max = 30.0;
uniform float sun_curve : hint_range(0, 1) = 0.15;

uniform sampler2D cloud_texture;
uniform sampler2D cloud_texture2;
uniform float cloud_uv_scale = 1.0f;
uniform float cloud_uv_scale2 = 1.0f;
uniform float small_cloud_cover : hint_range(0.0, 1.0) = 0.5;
uniform float large_cloud_cover : hint_range(0.0, 1.0) = 0.5;
uniform vec3 cloud_inner_colour : source_color = vec3(1.0, 1.0, 1.0);
uniform vec3 cloud_outer_colour : source_color = vec3(0.5, 0.5, 0.5);
uniform vec2 cloud_direction = vec2(1.0, 0.0);
uniform float cloud_speed = 0.001;
uniform float cloud_shape_change_speed = 0.001;
uniform bool cloud_blend_interpolated = false;

void sky() {
	float v_angle = acos(clamp(EYEDIR.y, -1.0, 1.0));
	float c = (1.0 - v_angle / (PI * 0.5));
	vec3 sky = mix(sky_horizon_color.rgb, sky_top_color.rgb, clamp(1.0 - pow(1.0 - c, 1.0 / sky_curve), 0.0, 1.0));
	sky *= sky_energy;

	if (LIGHT0_ENABLED) {
		float sun_angle = acos(dot(LIGHT0_DIRECTION, EYEDIR));
		if (sun_angle < LIGHT0_SIZE) {
			sky = LIGHT0_COLOR * LIGHT0_ENERGY;
		} else if (sun_angle < sun_angle_max) {
			float c2 = (sun_angle - LIGHT0_SIZE) / (sun_angle_max - LIGHT0_SIZE);
			sky = mix(LIGHT0_COLOR * LIGHT0_ENERGY, sky, clamp(1.0 - pow(1.0 - c2, 1.0 / sun_curve), 0.0, 1.0));
		}
	}

	if (LIGHT1_ENABLED) {
		float sun_angle = acos(dot(LIGHT1_DIRECTION, EYEDIR));
		if (sun_angle < LIGHT1_SIZE) {
			sky = LIGHT1_COLOR * LIGHT1_ENERGY;
		} else if (sun_angle < sun_angle_max) {
			float c2 = (sun_angle - LIGHT1_SIZE) / (sun_angle_max - LIGHT1_SIZE);
			sky = mix(LIGHT1_COLOR * LIGHT1_ENERGY, sky, clamp(1.0 - pow(1.0 - c2, 1.0 / sun_curve), 0.0, 1.0));
		}
	}

	if (LIGHT2_ENABLED) {
		float sun_angle = acos(dot(LIGHT2_DIRECTION, EYEDIR));
		if (sun_angle < LIGHT2_SIZE) {
			sky = LIGHT2_COLOR * LIGHT2_ENERGY;
		} else if (sun_angle < sun_angle_max) {
			float c2 = (sun_angle - LIGHT2_SIZE) / (sun_angle_max - LIGHT2_SIZE);
			sky = mix(LIGHT2_COLOR * LIGHT2_ENERGY, sky, clamp(1.0 - pow(1.0 - c2, 1.0 / sun_curve), 0.0, 1.0));
		}
	}

	if (LIGHT3_ENABLED) {
		float sun_angle = acos(dot(LIGHT3_DIRECTION, EYEDIR));
		if (sun_angle < LIGHT3_SIZE) {
			sky = LIGHT3_COLOR * LIGHT3_ENERGY;
		} else if (sun_angle < sun_angle_max) {
			float c2 = (sun_angle - LIGHT3_SIZE) / (sun_angle_max - LIGHT3_SIZE);
			sky = mix(LIGHT3_COLOR * LIGHT3_ENERGY, sky, clamp(1.0 - pow(1.0 - c2, 1.0 / sun_curve), 0.0, 1.0));
		}
	}

	vec4 sky_cover_texture = texture(sky_cover, SKY_COORDS);
	sky += (sky_cover_texture.rgb * sky_cover_modulate.rgb) * sky_cover_texture.a * sky_cover_modulate.a * sky_energy;

	c = (v_angle - (PI * 0.5)) / (PI * 0.5);
	vec3 ground = mix(ground_horizon_color.rgb, ground_bottom_color.rgb, clamp(1.0 - pow(1.0 - c, 1.0 / ground_curve), 0.0, 1.0));
	ground *= ground_energy;
	
	vec2 sky_uv = EYEDIR.xz / EYEDIR.y;
	vec2 cloud_uv_offset = cloud_direction * cloud_speed * TIME;
	float clouds1 = texture(cloud_texture, sky_uv * cloud_uv_scale + cloud_uv_offset).r;
	float clouds2 = texture(cloud_texture2, sky_uv * cloud_uv_scale2 + cloud_uv_offset + cloud_shape_change_speed * TIME).r;
	float clouds = clouds1 * small_cloud_cover + clouds2 * large_cloud_cover;
	clouds = clamp(clouds, 0.0f, 1.0);
	vec4 clouds_out_colour = vec4(mix(cloud_outer_colour, cloud_inner_colour, clouds), clouds);

	if (cloud_blend_interpolated)
		sky.rgb = mix(sky.rgb, clouds_out_colour.rgb, clouds_out_colour.a);
	else
		sky.rgb += clouds_out_colour.rgb * clouds_out_colour.a;
	COLOR = mix(ground, sky, step(0.0, EYEDIR.y));
}
