shader_type canvas_item;
render_mode unshaded;

uniform sampler2D FogTexture1 : repeat_enable, filter_nearest; 
uniform float Fog1XVelocity : hint_range(0.0,100.0,0.5);
uniform float Fog1YVelocity : hint_range(0.0,100.0,0.5);
uniform float Fog1MiddleInfluence : hint_range(0.0,2.0,0.1);

uniform sampler2D FogTexture2 : repeat_enable, filter_nearest; 
uniform float Fog2XVelocity : hint_range(0.0,100.0,0.5);
uniform float Fog2YVelocity : hint_range(0.0,100.0,0.5);
uniform float Fog2MiddleInfluence : hint_range(0.0,2.0,0.1);


varying vec2 world_position;
void vertex() {
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0,1.0)).xy;
	// Called for every vertex the material is visible on.
}

void fragment() {
	float Distance = distance(UV,vec2(0.5,0.5))*Fog1MiddleInfluence-0.05;
	float alpha = Distance*Distance;
	if (alpha > 1.0){
		alpha = 1.0;
	}
	if (alpha < 0.0){
		alpha = 0.0;
	}
	
	vec4 overlay_color_1 = texture(FogTexture1,vec2(world_position.x*0.0001+TIME*0.002,world_position.y*0.00026));
	
	vec4 overlay_color_2 = texture(FogTexture2,vec2(world_position.x*0.0001-TIME*0.002,world_position.y*0.00026));
	
	COLOR = mix(COLOR,overlay_color_1,0.9);
	COLOR = overlay_color_1 + overlay_color_2;
	COLOR.a = alpha;
	//COLOR.rgb = pow(COLOR.rgb, vec3(2.2));
	// Called for every pixel the material is visible on.
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
