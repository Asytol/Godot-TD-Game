// 2D Fog Shader - Advanced Version for Godot 4
// Apply this to a ColorRect covering the screen.
shader_type canvas_item;
render_mode unshaded; // Good practice for overlays that shouldn't be affected by scene light

//----------------------------------------------------------------------------//
// Uniform Groups for Inspector Organization
//----------------------------------------------------------------------------//
group_uniforms CoreFog;
uniform vec4 fog_color : source_color = vec4(0.1, 0.1, 0.15, 1.0); // Fog color and max opacity
uniform vec2 center_pos = vec2(576.0, 324.0); // Center of the clear area (pixels, updated via script)
uniform float inner_radius : hint_range(0.0, 1000.0, 1.0) = 100.0; // Radius where fog STARTS (pixels)
uniform float outer_radius : hint_range(0.0, 1500.0, 1.0) = 350.0; // Radius where fog reaches FULL opacity (pixels)

group_uniforms NoiseLayer1;
uniform bool use_noise_layer_1 = true;
uniform sampler2D noise_texture_1 : repeat_enable, filter_linear_mipmap; // Assign a noise texture (e.g., FastNoiseLiteTexture)
uniform float noise_scale_1 : hint_range(0.1, 20.0) = 4.0;         // How zoomed-in the noise pattern is (relative to screen UVs)
uniform vec2 noise_velocity_1 = vec2(0.03, 0.02);    // How fast the noise moves (set to 0,0 for static)
uniform float noise_edge_influence_1 : hint_range(0.0, 1.0) = 0.6; // How much noise distorts the fog edge (0=none, 1=max)
uniform float noise_edge_hardness_1 : hint_range(0.0, 500.0) = 80.0; // Pixel range the noise affects the edge

group_uniforms NoiseLayer2;
uniform bool use_noise_layer_2 = false; // Disabled by default
uniform sampler2D noise_texture_2 : repeat_enable, filter_linear_mipmap; // Assign a second, different noise texture
uniform float noise_scale_2 : hint_range(0.1, 40.0) = 8.5;
uniform vec2 noise_velocity_2 = vec2(-0.015, 0.025);
uniform float noise_edge_influence_2 : hint_range(0.0, 1.0) = 0.4;
uniform float noise_edge_hardness_2 : hint_range(0.0, 500.0) = 50.0;

group_uniforms DensityNoise;
uniform bool use_density_noise = true; // Make fog density itself noisy?
// Uses noise values from Layer 1 (and 2 if enabled)
uniform float density_noise_influence : hint_range(0.0, 1.0) = 0.15; // How much noise affects opacity inside the fog

group_uniforms FogTextureOverlay;
uniform bool use_fog_texture = false; // Overlay a texture?
uniform sampler2D fog_texture : repeat_enable, filter_linear_mipmap; // e.g., a seamless cloud/smoke texture
uniform vec2 fog_texture_scale = vec2(1.0, 1.0); // Texture tiling scale (relative to screen UVs)
uniform vec2 fog_texture_velocity = vec2(0.01, 0.005); // Texture scroll speed
uniform float fog_texture_influence : hint_range(0.0, 1.0) = 0.5; // Blend factor for the texture

group_uniforms DistanceTinting;
uniform bool use_distance_tint = false;
uniform vec4 clear_area_tint : source_color = vec4(0.8, 0.7, 0.5, 1.0); // Color tint near the clear radius edge
uniform float tint_feather : hint_range(1.0, 200.0) = 50.0; // How far the tint blends into the main fog color (pixels)


//----------------------------------------------------------------------------//
// Fragment Shader Logic
//----------------------------------------------------------------------------//
void fragment() {
    // -- 1. Calculate Pixel Position and Distance --
    // FRAGCOORD.xy = Screen pixel coordinates (e.g., 0,0 to 1920,1080) - Used for distance calcs.
    // SCREEN_UV = Normalized screen coordinates (0.0 to 1.0) - Used for texture sampling.
    vec2 screen_pixel_pos = FRAGCOORD.xy;
    float base_dist = distance(screen_pixel_pos, center_pos);

    // -- 2. Calculate Noise Value(s) --
    float noise_value_1 = 0.0; // Raw noise value (0-1) from layer 1
    float noise_value_2 = 0.0; // Raw noise value (0-1) from layer 2
    float combined_noise_centered = 0.0; // Combined noise centered around 0 (-0.5 to 0.5)

    if (use_noise_layer_1) {
        vec2 noise_uv_1 = SCREEN_UV * noise_scale_1 + TIME * noise_velocity_1;
        noise_value_1 = texture(noise_texture_1, noise_uv_1).r; // Assuming grayscale noise
        combined_noise_centered += (noise_value_1 - 0.5) * noise_edge_influence_1;

        if (use_noise_layer_2) {
            vec2 noise_uv_2 = SCREEN_UV * noise_scale_2 + TIME * noise_velocity_2;
            noise_value_2 = texture(noise_texture_2, noise_uv_2).r;
            // Combine Influence: Add the centered noise value weighted by its influence
            combined_noise_centered += (noise_value_2 - 0.5) * noise_edge_influence_2;
        }
    }

    // -- 3. Apply Noise to Distance (Edge Perturbation) --
    // Calculate the pixel offset caused by noise
    float noise_pixel_offset = 0.0;
    if (use_noise_layer_1) {
        // Use hardness from layer 1 as the primary control for offset magnitude
        noise_pixel_offset = combined_noise_centered * noise_edge_hardness_1;
        // Optional: Could add hardness_2 influence here if layer 2 is active
        // noise_pixel_offset += (noise_value_2 - 0.5) * noise_edge_influence_2 * noise_edge_hardness_2; // Alternative combination
    }

    float final_dist = base_dist + noise_pixel_offset;

    // -- 4. Calculate Base Fog Amount --
    // Ensure outer radius is always slightly larger than inner to prevent division by zero/NaN
    float effective_outer_radius = max(inner_radius + 0.01, outer_radius);
    // Calculate smooth transition from clear (0.0) to full fog (1.0)
    float fog_amount = smoothstep(inner_radius, effective_outer_radius, final_dist);

    // -- 5. Apply Density Noise (Optional) --
    if (use_density_noise && use_noise_layer_1) { // Only if base noise is active
         // Use the *same* combined noise, scaled by density influence
         // This makes dense spots correspond to noisy edges pushing outwards
        fog_amount += combined_noise_centered * density_noise_influence;
        fog_amount = clamp(fog_amount, 0.0, 1.0); // Ensure fog amount stays within valid range
    }

    // -- 6. Determine Fog Color (Base, Tinted, Textured) --
    vec3 final_fog_rgb = fog_color.rgb;

    // Apply Distance Tinting (Optional)
    if (use_distance_tint) {
        // Blend from clear_area_tint to fog_color based on distance from inner_radius
        float tint_falloff = inner_radius + tint_feather;
        float tint_mix = smoothstep(inner_radius, tint_falloff, base_dist); // Use base_dist for stable tinting
        final_fog_rgb = mix(clear_area_tint.rgb, final_fog_rgb, tint_mix);
    }

    // Apply Fog Texture Overlay (Optional)
    if (use_fog_texture) {
        vec2 fog_tex_uv = SCREEN_UV * fog_texture_scale + TIME * fog_texture_velocity;
        vec3 fog_tex_color = texture(fog_texture, fog_tex_uv).rgb;
        // Blend the texture using multiply (darkens) - other blend modes possible (add, overlay)
        // Mix based on influence: 1.0 is full white (no texture), influence controls blend amount
        final_fog_rgb = mix(final_fog_rgb * fog_tex_color, final_fog_rgb, 1.0 - fog_texture_influence);
        // Alternative (Additive): final_fog_rgb = mix(final_fog_rgb, final_fog_rgb + fog_tex_color, fog_texture_influence);
        // Alternative (Overlay-like): final_fog_rgb = mix(final_fog_rgb, 1.0 - 2.0 * (1.0 - final_fog_rgb) * (1.0 - fog_tex_color), fog_texture_influence); // Simplified overlay
    }

    // -- 7. Set Final Output Color --
    // The final alpha is determined by the calculated fog_amount multiplied by the base fog alpha.
    COLOR = vec4(final_fog_rgb, fog_amount * fog_color.a);
}