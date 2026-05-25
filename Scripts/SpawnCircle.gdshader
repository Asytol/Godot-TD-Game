shader_type canvas_item;
render_mode unshaded;
uniform sampler2D AddedTexture : repeat_enable, filter_linear;
uniform float ExtraAlpha = 0.06;

varying vec2 world_position;
uniform float TextSize = 0.1;

void vertex() {
	// Called for every vertex the material is visible on.
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0,1.0)).xy;
}

void fragment() {
	//COLOR.a = distance(UV,vec2(0.5,0.5))*2.0;
	vec4 overlay_texture = texture(AddedTexture,vec2(world_position.x*TextSize+TIME*0.4,world_position.y*TextSize));
	//overlay_texture *= texture(AddedTexture,vec2(world_position.x*TextSize-TIME*0.4,world_position.y*TextSize));
	if ((distance(UV,vec2(0.5,0.5)) > 0.1)){
		COLOR = mix(COLOR,overlay_texture,1.0);
	}
	
	COLOR.a -= distance(UV,vec2(0.5,0.5))*2.7+0.2;
	if (distance(UV,vec2(0.5,0.5)) < 0.23){
		COLOR.a = distance(UV,vec2(0.5,0.5))*2.3-0.4;
	}
	if (COLOR.a < 0.0){
		COLOR.a = 0.0;
	}
	if (COLOR.a != 0.0){
		COLOR.a += ExtraAlpha;
	}
	//COLOR.rgb = pow(COLOR.rgb, vec3(2.2));
	// Called for every pixel the material is visible on.
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
