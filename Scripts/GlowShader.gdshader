shader_type canvas_item;

uniform float Glow : hint_range(0.0,10.0,0.1);

void vertex() {
}

void fragment() {
	if (COLOR.a != 0.0){
		COLOR.a += Glow;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
