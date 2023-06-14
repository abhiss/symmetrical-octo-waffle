#ifndef CUSTOM_HLSL
#define CUSTOM_HLSL

void blend_angle_corrected_normals_half(half3 base_normal, half3 add_normal, out half3 res) {
    half3 b_inc = base_normal;
    b_inc.z += 1;
    half3 add_inv = add_normal * -1;
    add_inv.z *= -1;
    res = b_inc * dot(b_inc, add_inv) - b_inc.z * add_inv;
}

#endif