shader_type canvas_item;
render_mode unshaded;


uniform float ChangeSpeed : hint_range(0.0,1.0) = 0.1;
uniform float BlueColorAmount : hint_range(0.0,1.0) = 0.1;

uniform bool EnableShader = false;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	if (EnableShader){
		float BlueIncrease = sin(TIME*4.0);
		if (COLOR.a != 0.0){
			vec4 BlueColor = vec4(0.0,0.0,BlueColorAmount,1.0);
			if (BlueIncrease < 0.0){
				BlueIncrease = 0.0;
			}
			COLOR = mix(COLOR,BlueColor,BlueIncrease*0.8);	
		}
	}
	// Called for every pixel the material is visible on.
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
