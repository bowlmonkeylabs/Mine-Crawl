// Upgrade NOTE: upgraded instancing buffer 'Shader_OreCrystalNew' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shader_OreCrystalNew"
{
	Properties
	{
		_SmoothnessMetallic("Smoothness+Metallic", Range( 0 , 1)) = 0.383
		_Color("Color", Color) = (0.282353,0.8666667,0.8235294,1)
		_DamageFac("DamageFac", Range( 0 , 1)) = 0
		_DamageInsetMultiplier("DamageInsetMultiplier", Range( -1 , 1)) = -0.18
		_DamageScaleMultiplier("DamageScaleMultiplier", Range( -2 , 2)) = -0.05
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 4.6
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			half filler;
		};

		uniform float _DamageInsetMultiplier;
		uniform float _DamageScaleMultiplier;
		uniform float4 _Color;
		uniform float _SmoothnessMetallic;

		UNITY_INSTANCING_BUFFER_START(Shader_OreCrystalNew)
			UNITY_DEFINE_INSTANCED_PROP(float, _DamageFac)
#define _DamageFac_arr Shader_OreCrystalNew
		UNITY_INSTANCING_BUFFER_END(Shader_OreCrystalNew)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 appendResult12 = (float4(0.0 , 1.0 , 0.0 , 0.0));
			float _DamageFac_Instance = UNITY_ACCESS_INSTANCED_PROP(_DamageFac_arr, _DamageFac);
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 break20 = ase_vertex3Pos;
			float clampResult54 = clamp( (0.7 + (_DamageFac_Instance - 0.0) * (0.4 - 0.7) / (1.0 - 0.0)) , 0.0 , 1.0 );
			float clampResult47 = clamp( (0.0 + (break20.y - clampResult54) * (0.5 - 0.0) / (0.73 - clampResult54)) , 0.0 , 1.0 );
			float4 appendResult56 = (float4(break20.x , 0.0 , break20.z , 0.0));
			float4 normalizeResult57 = normalize( appendResult56 );
			v.vertex.xyz += ( ( appendResult12 * (0.0 + (_DamageFac_Instance - 0.0) * (_DamageInsetMultiplier - 0.0) / (1.0 - 0.0)) * clampResult47 ) + ( _DamageScaleMultiplier * normalizeResult57 * _DamageFac_Instance ) ).xyz;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Color.rgb;
			float temp_output_2_0 = _SmoothnessMetallic;
			o.Metallic = temp_output_2_0;
			o.Smoothness = temp_output_2_0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
869;424;1729;955;805.6033;-531.7943;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;49;-988.8012,916.2682;Inherit;False;996.7859;503.8422;Use 'Damage Factor' to lerp the highest vertices down;9;17;20;26;47;10;48;12;27;61;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-946.8012,955.2682;Inherit;False;InstancedProperty;_DamageFac;DamageFac;2;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;50;-1057.222,1488.258;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.7;False;4;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;17;-938.45,1162.241;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;20;-747.4497,1163.241;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ClampOpNode;54;-776.2219,1490.258;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;56;-587.6083,1452.767;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-948.6083,1052.767;Inherit;False;Property;_DamageInsetMultiplier;DamageInsetMultiplier;3;0;Create;True;0;0;0;False;0;False;-0.18;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;26;-610.015,1166.11;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.7;False;2;FLOAT;0.73;False;3;FLOAT;0;False;4;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;47;-339.6006,1165.847;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-610.6083,1619.767;Inherit;False;Property;_DamageScaleMultiplier;DamageScaleMultiplier;4;0;Create;True;0;0;0;False;0;False;-0.05;-0.05;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;57;-451.6083,1452.767;Inherit;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;12;-330.858,1007.186;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCRemapNode;48;-642.6004,968.8472;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;-0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-277.6083,1454.767;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;3;461.7161,680.2491;Inherit;False;617.5722;357.2172;Tesselation;4;7;6;5;4;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-154.0152,1067.11;Inherit;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;11.39172,1181.767;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;52;-1368.222,1485.258;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;511.716,730.2493;Inherit;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;0;False;0;False;32;0;1;32;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-1208.222,1487.258;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-673.7815,769.8032;Inherit;False;Property;_SmoothnessMetallic;Smoothness+Metallic;0;0;Create;True;0;0;0;False;0;False;0.383;0.383;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceBasedTessNode;7;831.289,792.1249;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;6;648.784,824.708;Inherit;False;Constant;_Float4;Float 4;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;647.774,921.4664;Inherit;False;Constant;_Float5;Float 5;3;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-273.8732,633.3308;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;0.282353,0.8666667,0.8235294,1;0.9333333,0.6901961,0.2078429,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;165.0051,794.8578;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;Shader_OreCrystalNew;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;50;0;10;0
WireConnection;20;0;17;0
WireConnection;54;0;50;0
WireConnection;56;0;20;0
WireConnection;56;2;20;2
WireConnection;26;0;20;1
WireConnection;26;1;54;0
WireConnection;47;0;26;0
WireConnection;57;0;56;0
WireConnection;48;0;10;0
WireConnection;48;4;61;0
WireConnection;58;0;60;0
WireConnection;58;1;57;0
WireConnection;58;2;10;0
WireConnection;27;0;12;0
WireConnection;27;1;48;0
WireConnection;27;2;47;0
WireConnection;59;0;27;0
WireConnection;59;1;58;0
WireConnection;52;0;10;0
WireConnection;53;0;52;0
WireConnection;53;1;52;0
WireConnection;7;0;5;0
WireConnection;7;1;6;0
WireConnection;7;2;4;0
WireConnection;0;0;1;0
WireConnection;0;3;2;0
WireConnection;0;4;2;0
WireConnection;0;11;59;0
ASEEND*/
//CHKSM=F6D0B375B37FA1F1463F6A02931E6E1EB3124252