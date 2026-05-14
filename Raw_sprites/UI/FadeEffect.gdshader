shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float AlphaAmount = 1.0;
	if (AlphaAmount == 0.0){
		AlphaAmount = 1.0;
	}
	else{
		AlphaAmount -= 1.0*sin(TIME*5.0);
	}
	if (COLOR.a != 0.0){
		COLOR.a = AlphaAmount;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
