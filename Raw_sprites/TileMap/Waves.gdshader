shader_type canvas_item;
render_mode unshaded;

uniform sampler2D texture1 : repeat_enable, filter_nearest;
uniform float scale1 = 0.01;
uniform float Mix1 : hint_range(0.0,1.0,0.1);

uniform sampler2D texture2 : repeat_enable, filter_nearest;
uniform float scale2 = 0.01;
uniform float Mix2 : hint_range(0.0,1.0,0.1);

uniform sampler2D white_texture: repeat_enable, filter_nearest;
uniform float scale3 = 0.01;
uniform float Mix3 : hint_range(0.0,1.0,0.1);

varying vec2 world_position;
void vertex() {
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0,1.0)).xy;
}

void fragment() {
	float mix_amount = floor(COLOR.r);
	vec4 overlay_color_1 = texture(texture1, world_position*scale1+TIME*0.002);;
	
	vec4 overlay_color_2 = texture(texture2, world_position*scale2-TIME*0.002);
	
	vec4 white_overlay_1 = texture(white_texture, world_position*scale3+TIME*0.001);;
	if (COLOR.r == 1.0)
	{
		COLOR = mix(COLOR,overlay_color_1,Mix1);
		COLOR = mix(COLOR,overlay_color_2,Mix2);
		if (white_overlay_1.a != 0.0){
			COLOR = mix(COLOR,white_overlay_1,Mix3);	
		}
		
		COLOR.rgb = pow(COLOR.rgb, vec3(2.2));
	}
	//COLOR = overlay_color_1;
}
