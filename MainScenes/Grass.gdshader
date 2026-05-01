shader_type canvas_item;
render_mode unshaded;

uniform sampler2D noise_texture_1 : repeat_enable, filter_linear_mipmap;
uniform float Red1 : hint_range(0.0,1.0,0.1);
uniform float Green1 : hint_range(0.0,1.0,0.1);
uniform float Blue1 : hint_range(0.0,1.0,0.1);

uniform sampler2D noise_texture_2 : repeat_enable, filter_linear_mipmap;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float noise_valueR = texture(noise_texture_1,UV).r;
	float noise_valueG = texture(noise_texture_1,UV).g;
	float noise_valueB = texture(noise_texture_1,UV).b;
	
	float noise_valueR2 = texture(noise_texture_2,UV).r;
	float noise_valueG2 = texture(noise_texture_2,UV).g;
	float noise_valueB2 = texture(noise_texture_2,UV).b;
	COLOR = vec4(noise_valueR + noise_valueR2,
	noise_valueG + noise_valueG2,
	noise_valueB + noise_valueB2,0.3);
	
	vec4 n1 = texture(noise_texture_1, UV);
    vec4 n2 = texture(noise_texture_2, UV);
	vec3 final_noise = mix(n1.rgb, n2.rgb, 0.4);
	//COLOR = texture(noise_texture_1,UV) + texture(noise_texture_2,UV);
	COLOR = vec4(final_noise,1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
