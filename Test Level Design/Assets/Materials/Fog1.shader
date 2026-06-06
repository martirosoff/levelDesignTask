// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Fog1"
{
	Properties
	{
		_CameraMin("CameraMin", Float) = 0
		_CameraMax("CameraMax", Float) = 1
		_CameraColorMin("CameraColorMin", Color) = (1,1,1,1)
		_CameraColorMax("CameraColorMax", Color) = (0,0,0,1)
		_VerticalMin("VerticalMin", Float) = 0
		_VerticalMax("VerticalMax", Float) = 1
		_VerticalColorMin("VerticalColorMin", Color) = (1,1,1,1)
		_VerticalColorMax("VerticalColorMax", Color) = (0,0,0,1)
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Emission("Emission", Range( 0 , 1)) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Glossiness("Glossiness", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float4 _CameraColorMin;
		uniform float4 _CameraColorMax;
		uniform float _CameraMin;
		uniform float _CameraMax;
		uniform float4 _VerticalColorMax;
		uniform float4 _VerticalColorMin;
		uniform float _VerticalMin;
		uniform float _VerticalMax;
		uniform float _Emission;
		uniform float _Metallic;
		uniform float _Glossiness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float4 lerpResult13 = lerp( _CameraColorMin , _CameraColorMax , saturate( (0.0 + (length( ( ase_worldPos - _WorldSpaceCameraPos ) ) - _CameraMin) * (1.0 - 0.0) / (_CameraMax - _CameraMin)) ));
			float4 lerpResult23 = lerp( _VerticalColorMax , _VerticalColorMin , saturate( (0.0 + (ase_worldPos.y - _VerticalMin) * (1.0 - 0.0) / (_VerticalMax - _VerticalMin)) ));
			float4 temp_output_28_0 = ( tex2D( _TextureSample0, uv_TextureSample0 ) * ( lerpResult13 * lerpResult23 ) );
			o.Albedo = temp_output_28_0.rgb;
			o.Emission = ( temp_output_28_0 * _Emission ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17101
543;211;1503;549;2314.67;632.6127;3.702388;True;False
Node;AmplifyShaderEditor.WorldSpaceCameraPos;2;-600.1147,280.6133;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;1;-422.5,10;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3;-98.43042,238.9798;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;6;0.8304195,343.9348;Inherit;False;Property;_CameraMin;CameraMin;0;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;14;-264.7108,797.131;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LengthOpNode;4;82.6757,241.0619;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;11.92469,483.2959;Inherit;False;Property;_CameraMax;CameraMax;1;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;68.21559,906.2634;Inherit;False;Property;_VerticalMin;VerticalMin;4;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;66.30986,982.6248;Inherit;False;Property;_VerticalMax;VerticalMax;5;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;19;290.6903,836.9171;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;5;279.3052,241.5885;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;20;521.3108,840.3076;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;22;306.9266,1215.202;Inherit;False;Property;_VerticalColorMax;VerticalColorMax;7;0;Create;True;0;0;False;0;0,0,0,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;21;297.5021,1039.179;Inherit;False;Property;_VerticalColorMin;VerticalColorMin;6;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;286.1169,443.8503;Inherit;False;Property;_CameraColorMin;CameraColorMin;2;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;12;295.5414,619.8726;Inherit;False;Property;_CameraColorMax;CameraColorMax;3;0;Create;True;0;0;False;0;0,0,0,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;8;509.9255,244.979;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;13;653.9976,438.9051;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;23;665.3829,1034.234;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;984.4968,569.2194;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;27;978.3768,281.6472;Inherit;True;Property;_TextureSample0;Texture Sample 0;8;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;1317.547,487.2049;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;29;1066.766,694.818;Inherit;False;Property;_Emission;Emission;9;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;1373.048,643.4286;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;26;1120.932,812.8474;Inherit;False;Property;_Metallic;Metallic;10;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;1122.987,892.1142;Inherit;False;Property;_Glossiness;Glossiness;11;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1673.842,591.4426;Float;False;True;2;ASEMaterialInspector;0;0;Standard;Custom/Fog1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;4;0;3;0
WireConnection;19;0;14;2
WireConnection;19;1;17;0
WireConnection;19;2;18;0
WireConnection;5;0;4;0
WireConnection;5;1;6;0
WireConnection;5;2;9;0
WireConnection;20;0;19;0
WireConnection;8;0;5;0
WireConnection;13;0;11;0
WireConnection;13;1;12;0
WireConnection;13;2;8;0
WireConnection;23;0;22;0
WireConnection;23;1;21;0
WireConnection;23;2;20;0
WireConnection;24;0;13;0
WireConnection;24;1;23;0
WireConnection;28;0;27;0
WireConnection;28;1;24;0
WireConnection;30;0;28;0
WireConnection;30;1;29;0
WireConnection;0;0;28;0
WireConnection;0;2;30;0
WireConnection;0;3;26;0
WireConnection;0;4;25;0
ASEEND*/
//CHKSM=96F5E78E231ADAADEB1478974DFD4D29F15D9172