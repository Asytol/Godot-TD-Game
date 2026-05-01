shader_type canvas_item;
render_mode unshaded;

uniform sampler2D noise_texture_1 : repeat_enable, filter_nearest;

uniform float scale1 = 0.01;
uniform float Red1 : hint_range(0.0,1.0,0.1);
uniform float Green1 : hint_range(0.0,1.0,0.1);
uniform float Blue1 : hint_range(0.0,1.0,0.1);

uniform sampler2D noise_texture_2 : repeat_enable, filter_linear_mipmap;

varying vec2 world_position;

const float cellsize = 16.0;
uniform float HeightLimit = 2;
uniform vec4 HeightColor = vec4(0,0,0,1);
void vertex() {
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0,1.0)).xy;
	
}

void fragment() {
	float mix_amount = floor(COLOR.r);
	vec4 overlay_color_1 = texture(noise_texture_1, world_position*scale1);
	if (COLOR.r == 1.0)
	{
		COLOR = mix(COLOR,overlay_color_1,1);
		
		COLOR.rgb = pow(COLOR.rgb, vec3(2.2));
	}
	//COLOR = overlay_color_1;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}