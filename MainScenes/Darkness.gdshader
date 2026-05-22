shader_type canvas_item;

uniform float ShadowAmount : hint_range(0.0,2.0,0.1);
uniform float ShadowStrenght : hint_range(0.0, 2.0, 0.1);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	//COLOR.a *= 1.0-(UV.y*UV.x)*PI;
	float alpha = (distance(UV,vec2(0.5,0.5)))*ShadowAmount;
	if (alpha > 0.0){
		alpha += ShadowStrenght;
	}
	if (alpha > 1.0){
		alpha = 1.0;
	}
	COLOR.a = alpha;
	//if (COLOR.a != 0.0){
	//	COLOR.a *= 1.0-(-UV.y*-UV.x)*PI;
	//}
	//COLOR.a *= 1.0-(-UV.y*-UV.x)*PI;
	// Called for every pixel the material is visible on.
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
