// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Custom/BasicLightTintedRampVC"
{
	Properties{

		/*_ShadowColor("Shadow Color", Color) = (0,0,0,0)
		_DirectionalMix("Directional Mix", Range(0,1)) = 1
		_EnvColor("Enviroment Color", Color) = (0,0,0,0)
		*/
		_ShadowColor("Shadow Color", Color) = (0,0,0,0)
		_VariationColor("Variation Color", Color) = (1,1,1,1)
		_ShadeColor("Shade Color", Color) = (0.5,0.5,0.5,0.5)
		_ShadeMin("Shade Min", Range (-1 , 1)) = 0
		_ShadeMax("Shade Max", Range (-1 , 1)) = 0
		_TintColor("TintColor", Color) = (1,1,1,1)
		_HighliteColor("HighliteColor", Color) = (0,0,0,0)

	}
		SubShader{
		//Tags{ "Queue" = "Geometry+1" }
		Tags{ "LightMode" = "ForwardBase"  "RenderType"="Opaque"  }
		LOD 200

		Pass
	{
		Name "BASE"

		//ZTest Always
		Lighting On

		Cull Back// Off


		CGPROGRAM

#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase
#pragma multi_compile_fog
#pragma multi_compile_instancing

	uniform float _DirectionalMix;
	uniform fixed4 _ShadowColor;
	uniform fixed4 _EnvColor;

	uniform float _ShadeMin;
	uniform float _ShadeMax;

	uniform fixed4 _TintColor;
	uniform fixed4 _HighliteColor;

	uniform fixed4 _VariationColor;
	uniform fixed4 _ShadeColor;

	struct VertIn
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 color: COLOR;
		float2 uv : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID

	};
	struct VertOut
	{
		float4 pos : POSITION;
		float4 color: COLOR;
		float3 normal : NORMAL;
		float2 vN : TEXTCOORD3;
		float2 uv0 : TEXTCOORD0;

		LIGHTING_COORDS(1, 2)

        float2 fogCoord: TEXTCOORD5;
		UNITY_VERTEX_INPUT_INSTANCE_ID

	};
	
	
	UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_INSTANCING_BUFFER_END(Props)
	
	VertOut vert(VertIn v)
	{

		VertOut outData;
		
		UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, outData);
		
		float4 vertex = v.vertex;

		outData.pos = UnityObjectToClipPos(vertex);
		outData.color = v.color;
		TRANSFER_VERTEX_TO_FRAGMENT(outData);
		
//		outData.normal = mul(unity_ObjectToWorld, float4(v.normal, 0));
//		outData.normal = normalize(outData.normal);	 
		
		outData.normal = UnityObjectToWorldNormal(v.normal);

        UNITY_TRANSFER_FOG(outData,outData.pos);

		return outData;
	}

	fixed4 frag(VertOut i) : SV_Target{

        UNITY_SETUP_INSTANCE_ID(i);
    
        fixed4 diffuseColor =  i.color * _VariationColor + _VariationColor.a * i.color;
    
        fixed4 envLight = _EnvColor * diffuseColor;
        
//        fixed lightColor = dot(i.normal, float3(0,1,0));// dot(i.normal, _WorldSpaceLightPos0.xyz);
        fixed lightColor = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz));
//        fixed attenFactor = pow(lightColor, 5);
        
        lightColor = smoothstep(_ShadeMin, _ShadeMax, lightColor);
    
//        float  atten = lerp(LIGHT_ATTENUATION(i), 1, 1 - attenFactor);
        fixed atten = SHADOW_ATTENUATION(i);// LIGHT_ATTENUATION(i);//
        
        fixed4 resultColor = lerp(diffuseColor * _ShadeColor, diffuseColor, lightColor);
    
        fixed4 shadow = _ShadowColor * resultColor;
    
        resultColor = resultColor * (atten) + shadow * (1 - atten);
    
        resultColor =  resultColor * _TintColor + _HighliteColor;

        UNITY_APPLY_FOG(i.fogCoord, resultColor);

        resultColor.a = 1;
        return resultColor;

	}

		ENDCG
	}
	}
		FallBack "Diffuse"

}
