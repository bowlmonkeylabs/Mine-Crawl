// Upgrade NOTE: upgraded instancing buffer 'Shader_Ore' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shader_Ore"
{
	Properties
	{
		_Albedo("Albedo", Color) = (0.4039216,0.2980392,0.07843138,1)
		_CrackAlbedo("Crack Albedo", Color) = (0.4039216,0.2980392,0.07843138,1)
		_VertexCrackThickness("Vertex Crack Thickness", Float) = 0.37
		_VertexCrackScale("Vertex Crack Scale", Float) = 0.56
		_CrackScale("Crack Scale", Float) = 2
		_DebugVertexCrack("Debug Vertex Crack", Range( 0 , 1)) = 0
		_VertexCrackStrengthFactor("Vertex Crack Strength Factor", Range( 0 , 1)) = 0
		_CrackStrengthFactor("Crack Strength Factor", Range( 0 , 1)) = 0
		_MaxVertexCrackStrength("Max Vertex Crack Strength", Float) = -3
		_MaxCrackStrength("Max Crack Strength", Float) = 6
		_MinVertexCrackStrength("Min Vertex Crack Strength", Float) = 0
		_MinCrackStrength("Min Crack Strength", Float) = 0
		_CenterOffset("Center Offset", Vector) = (0,0,0,0)
		_Float1("Float 1", Float) = 0
		_Float2("Float 2", Float) = 0
		_Float3("Float 3", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float _VertexCrackScale;
		uniform float _VertexCrackThickness;
		uniform float _MinVertexCrackStrength;
		uniform float _MaxVertexCrackStrength;
		uniform float4 _CrackAlbedo;
		uniform float4 _Albedo;
		uniform float _DebugVertexCrack;
		uniform float _MinCrackStrength;
		uniform float _MaxCrackStrength;
		uniform float _CrackScale;
		uniform float _Float1;
		uniform float _Float2;
		uniform float _Float3;

		UNITY_INSTANCING_BUFFER_START(Shader_Ore)
			UNITY_DEFINE_INSTANCED_PROP(float3, _CenterOffset)
#define _CenterOffset_arr Shader_Ore
			UNITY_DEFINE_INSTANCED_PROP(float, _VertexCrackStrengthFactor)
#define _VertexCrackStrengthFactor_arr Shader_Ore
			UNITY_DEFINE_INSTANCED_PROP(float, _CrackStrengthFactor)
#define _CrackStrengthFactor_arr Shader_Ore
		UNITY_INSTANCING_BUFFER_END(Shader_Ore)


		float2 voronoihash1( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi1( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash1( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			
			 		}
			 	}
			}
			return F2 - F1;
		}


		float2 voronoihash95( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi95( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash95( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			
			 		}
			 	}
			}
			
F1 = 8.0;
for ( int j = -2; j <= 2; j++ )
{
for ( int i = -2; i <= 2; i++ )
{
float2 g = mg + float2( i, j );
float2 o = voronoihash95( n + g );
		o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
float d = dot( 0.5 * ( r + mr ), normalize( r - mr ) );
F1 = min( F1, d );
}
}
return F1;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float time1 = 0.0;
			float2 voronoiSmoothId1 = 0;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float4 appendResult118 = (float4(0.0 , 1.0 , 0.0 , 0.0));
			float3 worldToObjDir117 = mul( unity_WorldToObject, float4( appendResult118.xyz, 0 ) ).xyz;
			float3 temp_output_6_0_g1 = worldToObjDir117;
			float dotResult1_g1 = dot( ase_vertex3Pos , temp_output_6_0_g1 );
			float dotResult2_g1 = dot( temp_output_6_0_g1 , temp_output_6_0_g1 );
			float3 _CenterOffset_Instance = UNITY_ACCESS_INSTANCED_PROP(_CenterOffset_arr, _CenterOffset);
			float3 modifiedUV100 = ( ( ase_vertex3Pos - ( ( dotResult1_g1 / dotResult2_g1 ) * temp_output_6_0_g1 ) ) + _CenterOffset_Instance );
			float2 coords1 = modifiedUV100.xy * _VertexCrackScale;
			float2 id1 = 0;
			float2 uv1 = 0;
			float voroi1 = voronoi1( coords1, time1, id1, uv1, 0, voronoiSmoothId1 );
			float temp_output_3_0_g138 = ( ( 1.0 - voroi1 ) - ( 1.0 - _VertexCrackThickness ) );
			float temp_output_34_0 = saturate( ( temp_output_3_0_g138 / 0 ) );
			float clampResult26 = clamp( ( 0.5 * distance( float3(0,0,0) , modifiedUV100 ) ) , 0.0 , 1.0 );
			float distanceMask112 = ( 1.0 - clampResult26 );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 worldToObjDir151 = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) ).xyz;
			float smoothstepResult82 = smoothstep( -0.001 , 0.0 , worldToObjDir151.y);
			float topMask143 = smoothstepResult82;
			float combinedMask144 = ( distanceMask112 * topMask143 );
			float _VertexCrackStrengthFactor_Instance = UNITY_ACCESS_INSTANCED_PROP(_VertexCrackStrengthFactor_arr, _VertexCrackStrengthFactor);
			v.vertex.xyz += ( ase_vertexNormal * temp_output_34_0 * combinedMask144 * (_MinVertexCrackStrength + (_VertexCrackStrengthFactor_Instance - 0.0) * (_MaxVertexCrackStrength - _MinVertexCrackStrength) / (1.0 - 0.0)) );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float time1 = 0.0;
			float2 voronoiSmoothId1 = 0;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 appendResult118 = (float4(0.0 , 1.0 , 0.0 , 0.0));
			float3 worldToObjDir117 = mul( unity_WorldToObject, float4( appendResult118.xyz, 0 ) ).xyz;
			float3 temp_output_6_0_g1 = worldToObjDir117;
			float dotResult1_g1 = dot( ase_vertex3Pos , temp_output_6_0_g1 );
			float dotResult2_g1 = dot( temp_output_6_0_g1 , temp_output_6_0_g1 );
			float3 _CenterOffset_Instance = UNITY_ACCESS_INSTANCED_PROP(_CenterOffset_arr, _CenterOffset);
			float3 modifiedUV100 = ( ( ase_vertex3Pos - ( ( dotResult1_g1 / dotResult2_g1 ) * temp_output_6_0_g1 ) ) + _CenterOffset_Instance );
			float2 coords1 = modifiedUV100.xy * _VertexCrackScale;
			float2 id1 = 0;
			float2 uv1 = 0;
			float voroi1 = voronoi1( coords1, time1, id1, uv1, 0, voronoiSmoothId1 );
			float temp_output_3_0_g138 = ( ( 1.0 - voroi1 ) - ( 1.0 - _VertexCrackThickness ) );
			float temp_output_34_0 = saturate( ( temp_output_3_0_g138 / fwidth( temp_output_3_0_g138 ) ) );
			float vertexCracks97 = temp_output_34_0;
			float clampResult26 = clamp( ( 0.5 * distance( float3(0,0,0) , modifiedUV100 ) ) , 0.0 , 1.0 );
			float distanceMask112 = ( 1.0 - clampResult26 );
			float blendOpSrc21 = vertexCracks97;
			float blendOpDest21 = step( ( 1.0 - _DebugVertexCrack ) , distanceMask112 );
			float4 temp_cast_2 = (( 1.0 - ( saturate( ( blendOpSrc21 * blendOpDest21 ) )) )).xxxx;
			float4 blendOpSrc37 = _Albedo;
			float4 blendOpDest37 = temp_cast_2;
			float4 blendOpSrc77 = _CrackAlbedo;
			float4 blendOpDest77 = ( saturate( ( blendOpSrc37 * blendOpDest37 ) ));
			float _CrackStrengthFactor_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackStrengthFactor_arr, _CrackStrengthFactor);
			float3 ase_worldNormal = i.worldNormal;
			float3 worldToObjDir151 = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) ).xyz;
			float smoothstepResult82 = smoothstep( -0.001 , 0.0 , worldToObjDir151.y);
			float topMask143 = smoothstepResult82;
			float combinedMask144 = ( distanceMask112 * topMask143 );
			float time95 = 0.0;
			float2 voronoiSmoothId95 = 0;
			float2 coords95 = modifiedUV100.xy * _CrackScale;
			float2 id95 = 0;
			float2 uv95 = 0;
			float voroi95 = voronoi95( coords95, time95, id95, uv95, 0, voronoiSmoothId95 );
			float temp_output_147_0 = ( combinedMask144 * voroi95 );
			float clampResult135 = clamp( ( ( (_MinCrackStrength + (_CrackStrengthFactor_Instance - 0.0) * (_MaxCrackStrength - _MinCrackStrength) / (1.0 - 0.0)) * combinedMask144 ) - temp_output_147_0 ) , 0.0 , 1.0 );
			float smoothstepResult137 = smoothstep( _Float1 , _Float2 , temp_output_147_0);
			float4 lerpBlendMode77 = lerp(blendOpDest77,min( blendOpSrc77 , blendOpDest77 ),( round( clampResult135 ) + ( smoothstepResult137 * _Float3 ) ));
			o.Albedo = ( saturate( lerpBlendMode77 )).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
1102;244;1693;1016;1669.206;-107.0812;1;True;True
Node;AmplifyShaderEditor.DynamicAppendNode;118;-2080,-352;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PosVertexDataNode;5;-1920,-512;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;117;-1952,-352;Inherit;False;World;Object;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FunctionNode;120;-1728,-448;Inherit;False;Projection;-1;;1;3249e2c8638c9ef4bbd1902a2d38a67c;0;2;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;127;-1568,-352;Inherit;False;InstancedProperty;_CenterOffset;Center Offset;12;0;Create;True;0;0;0;False;0;False;0,0,0;-0.26,-0.81,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;125;-1568,-512;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-1408,-512;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;100;-1280,-512;Inherit;False;modifiedUV;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;111;-1462.086,302.2205;Inherit;False;Constant;_HitPoint;Hit Point;12;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;102;-1457.103,503.9467;Inherit;False;100;modifiedUV;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1220.134,392.2138;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;22;-1220.134,488.2137;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;96;-1024,768;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformDirectionNode;151;-1007.614,920.071;Inherit;False;World;Object;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1060.134,392.2138;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;80;-832,768;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ClampOpNode;26;-836.1335,392.2138;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;82;-704,768;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;-0.001;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;23;-612.1335,392.2138;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;112;-384,448;Inherit;False;distanceMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;143;-384,832;Inherit;False;topMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;142;-128,704;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1272,156;Inherit;False;Property;_VertexCrackScale;Vertex Crack Scale;3;0;Create;True;0;0;0;False;0;False;0.56;0.36;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;-1262.358,0.6077805;Inherit;False;100;modifiedUV;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VoronoiNode;1;-1024,0;Inherit;True;0;0;1;2;1;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0.95;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.RangedFloatNode;35;-1072,-96;Inherit;False;Property;_VertexCrackThickness;Vertex Crack Thickness;2;0;Create;True;0;0;0;False;0;False;0.37;0.26;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-1040.265,-830.0291;Inherit;False;Property;_MinCrackStrength;Min Crack Strength;11;0;Create;True;0;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-992,-608;Inherit;False;Property;_CrackScale;Crack Scale;4;0;Create;True;0;0;0;False;0;False;2;1.7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-1040.265,-734.0291;Inherit;False;Property;_MaxCrackStrength;Max Crack Strength;9;0;Create;True;0;0;0;False;0;False;6;2.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;106;-1040.265,-926.029;Inherit;False;InstancedProperty;_CrackStrengthFactor;Crack Strength Factor;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;144;32,704;Inherit;False;combinedMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-848,0;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-864,224;Inherit;False;Property;_DebugVertexCrack;Debug Vertex Crack;5;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;109;-752.2649,-926.029;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-733,-722;Inherit;False;144;combinedMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;36;-832,-96;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;95;-768,-608;Inherit;True;0;0;1;4;1;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-515,-845;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;44;-576,224;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;-505.8819,-638.444;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;34;-672,-96;Inherit;True;Step Antialiasing;-1;;138;2a825e80dfb3290468194f83380797bd;0;2;1;FLOAT;0.94;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;138;-669.6787,-337.3978;Inherit;False;Property;_Float1;Float 1;13;0;Create;True;0;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;93;-273.8158,-721.7497;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-650.6787,-250.3978;Inherit;False;Property;_Float2;Float 2;14;0;Create;True;0;0;0;False;0;False;0;-1.86;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;97;-433.0083,-99.74791;Inherit;False;vertexCracks;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;42;-240,160;Inherit;True;2;0;FLOAT;0.74;False;1;FLOAT;0.81;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;135;-134.4581,-791.6185;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;137;-466.0967,-444.6071;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.21;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;21;-133.6875,-206.4275;Inherit;True;Multiply;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;-233.6787,-308.3978;Inherit;False;Property;_Float3;Float 3;15;0;Create;True;0;0;0;False;0;False;0;-9.53;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;376.5393,-812.9259;Inherit;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;0.4039216,0.2980392,0.07843138,1;0.3679245,0.3679245,0.3679245,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;38;376.5393,-620.9261;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;90;-67,-598;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;432,560;Inherit;False;Property;_MaxVertexCrackStrength;Max Vertex Crack Strength;8;0;Create;True;0;0;0;False;0;False;-3;-1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;-77.67871,-443.3978;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;105;432,464;Inherit;False;Property;_MinVertexCrackStrength;Min Vertex Crack Strength;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;70;432,368;Inherit;False;InstancedProperty;_VertexCrackStrengthFactor;Vertex Crack Strength Factor;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;653.9899,256.3004;Inherit;False;144;combinedMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;103;720,368;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;37;608,-704;Inherit;True;Multiply;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;136;88.90332,-509.6071;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;76;656,-464;Inherit;False;Property;_CrackAlbedo;Crack Albedo;1;0;Create;True;0;0;0;False;0;False;0.4039216,0.2980392,0.07843138,1;0.2339999,0.2339999,0.2339999,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;67;648.588,75.98205;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;1031.668,125.5068;Inherit;True;4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldToObjectMatrix;148;-160.0864,962.1072;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.BlendOpsNode;77;896,-544;Inherit;True;Darken;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0.2924528,0.2580149,0.2002238,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1548.377,-250.3481;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Shader_Ore;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;1;False;-1;1;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;117;0;118;0
WireConnection;120;5;5;0
WireConnection;120;6;117;0
WireConnection;125;0;5;0
WireConnection;125;1;120;0
WireConnection;126;0;125;0
WireConnection;126;1;127;0
WireConnection;100;0;126;0
WireConnection;22;0;111;0
WireConnection;22;1;102;0
WireConnection;151;0;96;0
WireConnection;24;0;25;0
WireConnection;24;1;22;0
WireConnection;80;0;151;0
WireConnection;26;0;24;0
WireConnection;82;0;80;1
WireConnection;23;0;26;0
WireConnection;112;0;23;0
WireConnection;143;0;82;0
WireConnection;142;0;112;0
WireConnection;142;1;143;0
WireConnection;1;0;101;0
WireConnection;1;2;40;0
WireConnection;144;0;142;0
WireConnection;28;0;1;0
WireConnection;109;0;106;0
WireConnection;109;3;108;0
WireConnection;109;4;107;0
WireConnection;36;0;35;0
WireConnection;95;0;100;0
WireConnection;95;2;99;0
WireConnection;110;0;109;0
WireConnection;110;1;114;0
WireConnection;44;0;43;0
WireConnection;147;0;114;0
WireConnection;147;1;95;0
WireConnection;34;1;36;0
WireConnection;34;2;28;0
WireConnection;93;0;110;0
WireConnection;93;1;147;0
WireConnection;97;0;34;0
WireConnection;42;0;44;0
WireConnection;42;1;112;0
WireConnection;135;0;93;0
WireConnection;137;0;147;0
WireConnection;137;1;138;0
WireConnection;137;2;139;0
WireConnection;21;0;97;0
WireConnection;21;1;42;0
WireConnection;38;0;21;0
WireConnection;90;0;135;0
WireConnection;141;0;137;0
WireConnection;141;1;140;0
WireConnection;103;0;70;0
WireConnection;103;3;105;0
WireConnection;103;4;104;0
WireConnection;37;0;2;0
WireConnection;37;1;38;0
WireConnection;136;0;90;0
WireConnection;136;1;141;0
WireConnection;68;0;67;0
WireConnection;68;1;34;0
WireConnection;68;2;113;0
WireConnection;68;3;103;0
WireConnection;77;0;76;0
WireConnection;77;1;37;0
WireConnection;77;2;136;0
WireConnection;0;0;77;0
WireConnection;0;11;68;0
ASEEND*/
//CHKSM=FA15BF1199C3975CB92342DE0A074B9C86BADC31