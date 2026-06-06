Shader "Mobile/MobileDiffuse_Vertex" {

Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Color("Color", Color) = (1,1,1,1)
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

CGPROGRAM

#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
uniform float4 _Color;

struct Input {
    float2 uv_MainTex;
     float4 vertexColor : COLOR;
};


void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = (c* IN.vertexColor*_Color).rgb;
    o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
