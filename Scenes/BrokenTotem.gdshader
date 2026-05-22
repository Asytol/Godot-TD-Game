shader_type canvas_item;

uniform sampler2D Texture1 : repeat_enable, filter_nearest;

uniform float feather : hint_range(0.01, 1.0) = 0.2;
uniform float threshold : hint_range(0.0, 1.0) = 0.5;

varying vec2 world_position;
void vertex() {
	// Called for every vertex the material is visible on.
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0,1.0)).xy;
}

void fragment() {
	vec4 overlay_color_1 = texture(Texture1, world_position*1.0+TIME*0.002);
	overlay_color_1 = texture(Texture1, world_position*0.005+TIME*0.01);
	overlay_color_1 += texture(Texture1, world_position*0.005-TIME*0.01);
	COLOR = mix(COLOR,overlay_color_1,1.0);
	COLOR.rgb = pow(COLOR.rgb, vec3(2.2));
	
	float mask = smoothstep(0.0,threshold+feather,UV.y);
	COLOR.a *= mask;
	float mask2 = smoothstep(1.0,1.0-threshold-feather,UV.y);
	COLOR.a *= mask2;
	float mask3 = smoothstep(1.0,0.7,UV.x);
	COLOR.a *= mask3;
	COLOR.a *= 0.2;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
