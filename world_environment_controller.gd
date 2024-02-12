extends WorldEnvironment

var cloud_cover = 1.0;

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	cloud_cover += delta * 0.1;
	var sky_material = environment.sky.sky_material as ShaderMaterial;
	sky_material.set_shader_parameter("cloud_cover", 1.0 / cloud_cover);
	#this.Sky += delta * 0.01f;
	pass
