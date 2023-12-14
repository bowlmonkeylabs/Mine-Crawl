// Upgrade NOTE: upgraded instancing buffer 'MushroomCap' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MushroomCap"
{
	Properties
	{
		Mask("Mask", 2D) = "white" {}
		_Color1("Color 1", Color) = (1,0,0,1)
		_Color2("Color 2", Color) = (1,1,1,1)
		_Smoothness("Smoothness", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D Mask;
		uniform float _Smoothness;

		UNITY_INSTANCING_BUFFER_START(MushroomCap)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color2)
#define _Color2_arr MushroomCap
			UNITY_DEFINE_INSTANCED_PROP(float4, Mask_ST)
#define Mask_ST_arr MushroomCap
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color1)
#define _Color1_arr MushroomCap
		UNITY_INSTANCING_BUFFER_END(MushroomCap)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 _Color2_Instance = UNITY_ACCESS_INSTANCED_PROP(_Color2_arr, _Color2);
			float4 Mask_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(Mask_ST_arr, Mask_ST);
			float2 uvMask = i.uv_texcoord * Mask_ST_Instance.xy + Mask_ST_Instance.zw;
			float4 tex2DNode1 = tex2D( Mask, uvMask );
			float4 _Color1_Instance = UNITY_ACCESS_INSTANCED_PROP(_Color1_arr, _Color1);
			o.Albedo = ( ( _Color2_Instance * tex2DNode1 ) + ( ( 1.0 - tex2DNode1.r ) * _Color1_Instance ) ).rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
1191;432;2047;947;1246.6;417.9954;1.239949;True;True
Node;AmplifyShaderEditor.SamplerNode;1;-640,-34;Inherit;True;Property;Mask;Mask;0;0;Create;True;0;0;0;False;0;False;-1;a723da023fe193849afa3789ee33c0b1;a723da023fe193849afa3789ee33c0b1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;6;-262,74;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-638,-257;Inherit;False;InstancedProperty;_Color2;Color 2;2;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;5;-640,224;Inherit;False;InstancedProperty;_Color1;Color 1;1;0;Create;True;0;0;0;False;0;False;1,0,0,1;0.957,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-240,-128;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-256,160;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-32,16;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-98,257;Inherit;False;Property;_Metallic;Metallic;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-86.5,374.5;Inherit;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;0;False;0;False;0;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;224,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;MushroomCap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;1;1
WireConnection;3;0;2;0
WireConnection;3;1;1;0
WireConnection;7;0;6;0
WireConnection;7;1;5;0
WireConnection;8;0;3;0
WireConnection;8;1;7;0
WireConnection;0;0;8;0
WireConnection;0;4;9;0
ASEEND*/
//CHKSM=D44F79C9094583E079E0A069FA50C298DB9FC48A