// Upgrade NOTE: upgraded instancing buffer 'Shader_OreNew' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shader_OreNew"
{
	Properties
	{
		_Albedo("Albedo", Color) = (0.5566038,0.4088009,0.2599235,1)
		_CrackAlbedo("Crack Albedo", Color) = (1,1,1,1)
		_CritPosition("Crit Position", Vector) = (0,0,0,0)
		_VoronoiScale("Voronoi Scale", Float) = 5
		_CrackPosition0("Crack Position 0", Vector) = (0,0,0,0)
		_CrackPosition1("Crack Position 1", Vector) = (0,0,0,0)
		_CrackPosition2("Crack Position 2", Vector) = (0,0,0,0)
		_EmissionStrength("EmissionStrength", Range( 0 , 100)) = 9
		_CritCracksSpread("CritCracksSpread", Range( 0 , 1)) = 0.05
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float4 _CrackAlbedo;
		uniform float4 _Albedo;
		uniform float _VoronoiScale;
		uniform float _CritCracksSpread;
		uniform float _EmissionStrength;

		UNITY_INSTANCING_BUFFER_START(Shader_OreNew)
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition0)
#define _CrackPosition0_arr Shader_OreNew
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition1)
#define _CrackPosition1_arr Shader_OreNew
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition2)
#define _CrackPosition2_arr Shader_OreNew
			UNITY_DEFINE_INSTANCED_PROP(float3, _CritPosition)
#define _CritPosition_arr Shader_OreNew
		UNITY_INSTANCING_BUFFER_END(Shader_OreNew)


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
			
F1 = 8.0;
for ( int j = -2; j <= 2; j++ )
{
for ( int i = -2; i <= 2; i++ )
{
float2 g = mg + float2( i, j );
float2 o = voronoihash1( n + g );
		o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
float d = dot( 0.5 * ( r + mr ), normalize( r - mr ) );
F1 = min( F1, d );
}
}
return F1;
		}


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 blendOpSrc3 = _CrackAlbedo;
			float4 blendOpDest3 = _Albedo;
			float4 transform176 = mul(unity_ObjectToWorld,float4( 0,1,0,0 ));
			float dotResult4_g5 = dot( transform176.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g5 = lerp( 0.0 , 20.0 , frac( ( sin( dotResult4_g5 ) * 43758.55 ) ));
			float time1 = lerpResult10_g5;
			float2 voronoiSmoothId1 = 0;
			float2 coords1 = i.uv_texcoord * _VoronoiScale;
			float2 id1 = 0;
			float2 uv1 = 0;
			float voroi1 = voronoi1( coords1, time1, id1, uv1, 0, voronoiSmoothId1 );
			float temp_output_5_0 = ( 1.0 - voroi1 );
			float clampResult6 = clamp( temp_output_5_0 , 0.0 , 1.0 );
			float4 lerpBlendMode3 = lerp(blendOpDest3,min( blendOpSrc3 , blendOpDest3 ),clampResult6);
			float div12=256.0/float(2);
			float4 posterize12 = ( floor( ( saturate( lerpBlendMode3 )) * div12 ) / div12 );
			float4 color76 = IsGammaSpace() ? float4(0.1981132,0.1132669,0.06448024,1) : float4(0.03251993,0.01220649,0.005366667,1);
			float4 blendOpSrc28 = posterize12;
			float4 blendOpDest28 = color76;
			float clampResult153 = clamp( (-1.3 + (distance( i.uv_texcoord , float2( 0.5,0.5 ) ) - 1.0) * (1.0 - -1.3) / (0.0 - 1.0)) , 0.0 , 1.0 );
			float4 lerpBlendMode28 = lerp(blendOpDest28,	max( blendOpSrc28, blendOpDest28 ),clampResult153);
			float temp_output_45_0 = (0.0 + (temp_output_5_0 - 0.9) * (1.0 - 0.0) / (1.0 - 0.9));
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 normalizeResult35 = normalize( ase_vertex3Pos );
			float3 temp_output_87_0 = ( normalizeResult35 * float3( 1,1,1 ) );
			float3 _CrackPosition0_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition0_arr, _CrackPosition0);
			float3 normalizeResult82 = normalize( _CrackPosition0_Instance );
			float dotResult84 = dot( temp_output_87_0 , normalizeResult82 );
			float clampResult99 = clamp( (0.0 + (dotResult84 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float3 _CrackPosition1_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition1_arr, _CrackPosition1);
			float3 normalizeResult93 = normalize( _CrackPosition1_Instance );
			float dotResult91 = dot( temp_output_87_0 , normalizeResult93 );
			float clampResult100 = clamp( (0.0 + (dotResult91 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float3 _CrackPosition2_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition2_arr, _CrackPosition2);
			float3 normalizeResult102 = normalize( _CrackPosition2_Instance );
			float dotResult106 = dot( temp_output_87_0 , normalizeResult102 );
			float clampResult105 = clamp( (0.0 + (dotResult106 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float clampResult97 = clamp( ( clampResult99 + clampResult100 + clampResult105 ) , 0.0 , 3.0 );
			float clampResult110 = clamp( ( temp_output_45_0 * clampResult97 ) , 0.0 , 1.0 );
			o.Albedo = ( ( saturate( lerpBlendMode28 )) * ( 1.0 - clampResult110 ) ).rgb;
			float4 color202 = IsGammaSpace() ? float4(1,0.9137255,0.4392157,1) : float4(1,0.8148467,0.1620294,1);
			float smoothstepResult55 = smoothstep( 0.9 , 1.0 , temp_output_45_0);
			float3 _CritPosition_Instance = UNITY_ACCESS_INSTANCED_PROP(_CritPosition_arr, _CritPosition);
			float3 normalizeResult26 = normalize( _CritPosition_Instance );
			float dotResult16 = dot( normalizeResult35 , normalizeResult26 );
			float clampResult52 = clamp( ( smoothstepResult55 * (0.0 + (dotResult16 - ( 1.0 - _CritCracksSpread )) * (1.0 - 0.0) / (1.0 - ( 1.0 - _CritCracksSpread ))) ) , 0.0 , 1.0 );
			float mulTime196 = _Time.y * 0.01;
			float4 appendResult198 = (float4(mulTime196 , 0.0 , 0.0 , 0.0));
			float2 uv_TexCoord195 = i.uv_texcoord + appendResult198.xy;
			float simplePerlin2D192 = snoise( uv_TexCoord195*30.0 );
			simplePerlin2D192 = simplePerlin2D192*0.5 + 0.5;
			float4 temp_cast_3 = (( clampResult52 - (0.0 + (simplePerlin2D192 - 0.0) * (0.3 - 0.0) / (1.0 - 0.0)) )).xxxx;
			float4 blendOpSrc42 = color202;
			float4 blendOpDest42 = temp_cast_3;
			o.Emission = ( ( saturate( ( blendOpSrc42 * blendOpDest42 ) )) * _EmissionStrength ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
1874;452;1729;943;1218.505;-246.6179;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;66;-2288.18,362.4552;Inherit;False;2190.409;1108.87;Comment;15;197;196;191;42;179;194;52;193;46;192;195;65;64;198;202;Critical cracks;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;64;-2255.463,739.7515;Inherit;False;1175.899;415.2642;Mask towards "Crit Position";9;36;16;26;25;35;31;180;200;201;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;128;-1990.228,1680.734;Inherit;False;1991.234;1102.633;Comment;10;97;94;98;87;146;145;144;101;79;89;Mask Crack Positions;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;63;-2262.797,-461.0663;Inherit;False;1251.027;757.0484;Comment;5;6;5;1;138;62;Voronoi;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;101;-1564.44,2454.807;Inherit;False;1066.948;286.1528;Mask towards "Crack Position 2";5;106;105;104;103;102;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;79;-1557.47,1846.564;Inherit;False;1062.152;287.2551;Mask towards "Crack Position 0";5;99;85;82;80;84;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;62;-2212.797,-411.0663;Inherit;False;507.1282;238;Randomize texture by world "up-axis" direction;2;24;176;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;31;-1988.188,789.7516;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;89;-1563.436,2152.826;Inherit;False;1066.948;284.2551;Mask towards "Crack Position 1";5;90;92;93;91;100;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;103;-1526.15,2513.037;Inherit;False;InstancedProperty;_CrackPosition2;Crack Position 2;7;0;Create;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;80;-1522.976,1904.794;Inherit;False;InstancedProperty;_CrackPosition0;Crack Position 0;5;0;Create;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;176;-2149.569,-357.1526;Inherit;False;1;0;FLOAT4;0,1,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;92;-1525.146,2211.057;Inherit;False;InstancedProperty;_CrackPosition1;Crack Position 1;6;0;Create;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;35;-1787.187,859.1516;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;138;-1896.336,-49.86877;Inherit;False;Property;_VoronoiScale;Voronoi Scale;4;0;Create;True;0;0;0;False;0;False;5;7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;24;-1910.67,-359.3318;Inherit;False;Random Range;-1;;5;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;82;-1318.597,1967.914;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-1350.502,1730.734;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;102;-1321.771,2576.156;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;93;-1320.767,2274.176;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-1701.298,1189.604;Inherit;False;Constant;_Float6;Float 6;9;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;91;-1154.928,2206.509;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;84;-1152.758,1900.246;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;106;-1155.932,2508.489;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-1161.613,1741.332;Inherit;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;0;False;0;False;0.75;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;1;-1665.197,-382.6334;Inherit;True;0;0;1;4;1;False;4;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;5;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.Vector3Node;25;-1981.713,952.2017;Inherit;False;InstancedProperty;_CritPosition;Crit Position;3;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;5;-1466.197,-388.6334;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;196;-1545.569,1191.586;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;90;-933.1182,2204.678;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-1678.036,788.5909;Inherit;False;Property;_CritCracksSpread;CritCracksSpread;9;0;Create;True;0;0;0;False;0;False;0.05;0.04;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;104;-934.1225,2506.658;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;65;-1696.697,412.4552;Inherit;False;615.6573;304.5728;Sharpen cracks;2;45;55;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;85;-930.949,1898.416;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;26;-1788.213,945.1013;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;16;-1610.374,863.4337;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;99;-652.7059,1902.212;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;45;-1646.697,463.0281;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;198;-1342.616,1175.022;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;201;-1406.036,789.5909;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;105;-661.7101,2507.191;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;100;-660.7059,2205.212;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;61;-923.7529,-851.9329;Inherit;False;1871.511;1037.927;Comment;7;28;12;2;9;3;71;129;Base Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;195;-1186.077,1164.316;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;94;-389.1177,2045.622;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;36;-1369.564,863.6038;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.95;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;148;-523.285,-746.3354;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;55;-1335.039,462.4554;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-871.1528,-591.3323;Inherit;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;0.5566038,0.4088009,0.2599235,1;0.794,0.7201173,0.642346,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;6;-1266.716,-387.6334;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;192;-1050.397,916.6709;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;30;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;9;-870.1528,-374.332;Inherit;False;Property;_CrackAlbedo;Crack Albedo;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.595,0.5367935,0.4656522,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;129;1.330471,-158.3274;Inherit;False;914.6672;307.4066;Darken cracks in "Crack Positions" Mask;4;109;108;107;110;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-1040.937,441.7067;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;150;-297.285,-745.3354;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;97;-169.9938,2046.247;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;3;-196.1527,-408.3319;Inherit;True;Darken;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;51.33055,-106.1982;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;152;-76.28503,-742.3354;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;-1.3;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;52;-1050.001,680.7764;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;71;-542.7219,-802.0814;Inherit;False;1155.794;361.0718;Darken edges;1;76;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;193;-809.8801,923.6071;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;153;191.1495,-739.3545;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;110;261.662,-104.9208;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;202;-549.5045,454.6179;Inherit;False;Constant;_EmissionColor1;EmissionColor;2;0;Create;True;0;0;0;False;0;False;1,0.9137255,0.4392157,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;76;342.1132,-738.046;Inherit;False;Constant;_Color1;Color 1;2;0;Create;True;0;0;0;False;0;False;0.1981132,0.1132669,0.06448024,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;194;-781.1864,752.6968;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosterizeNode;12;81.70221,-406.3568;Inherit;True;2;2;1;COLOR;0,0,0,0;False;0;INT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;42;-558.8153,643.3081;Inherit;True;Multiply;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;143;530.7997,926.6227;Inherit;False;1929.92;891.5056;Comment;14;118;120;119;136;121;142;114;141;112;113;126;125;135;122;Vertex Displacement Experiment;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-574.5415,883.9559;Inherit;False;Property;_EmissionStrength;EmissionStrength;8;0;Create;True;0;0;0;False;0;False;9;10;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;127;1328.238,-87.299;Inherit;False;617.5722;357.2172;Tesselation;4;115;54;117;116;;1,1,1,1;0;0
Node;AmplifyShaderEditor.BlendOpsNode;28;372.0377,-405.4874;Inherit;True;Lighten;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;108;495.3158,-106.1627;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosterizeNode;135;1286.738,1134.071;Inherit;True;3;2;1;COLOR;0,0,0,0;False;0;INT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;146;-1713.565,2529.957;Inherit;False;FLOAT4;4;0;FLOAT;-0.7;False;1;FLOAT;0.3;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;2291.973,1232.172;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;126;2086.822,1073.477;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;180;-2156.581,962.3496;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0.4;False;2;FLOAT;-1;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;-267.4262,787.5975;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;117;1514.296,153.9183;Inherit;False;Constant;_Float5;Float 5;3;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;580.7997,1496.767;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;142;1405.632,976.6229;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;144;-1717.565,1947.958;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;115;1378.238,-37.29879;Inherit;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;0;False;0;False;32;0;1;32;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;125;1889.394,1074.1;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;680.9976,-108.3274;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;116;1515.306,57.15995;Inherit;False;Constant;_Float4;Float 4;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;2298.72,1419.579;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;1680.707,1248.173;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;119;985.7963,1338.129;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;145;-1718.565,2208.958;Inherit;False;FLOAT4;4;0;FLOAT;0.7;False;1;FLOAT;0.3;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SmoothstepOpNode;121;981.7963,1564.129;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;112;1907.794,1461.582;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCCompareWithRange;136;1291.132,1410.036;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.86;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;114;1923.953,1356.94;Inherit;False;Property;_Float2;Float 2;2;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;120;809.7962,1501.129;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DistanceBasedTessNode;54;1697.811,24.57692;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;190;2313.181,345.8943;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Shader_OreNew;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;35;0;31;0
WireConnection;24;1;176;0
WireConnection;82;0;80;0
WireConnection;87;0;35;0
WireConnection;102;0;103;0
WireConnection;93;0;92;0
WireConnection;91;0;87;0
WireConnection;91;1;93;0
WireConnection;84;0;87;0
WireConnection;84;1;82;0
WireConnection;106;0;87;0
WireConnection;106;1;102;0
WireConnection;1;1;24;0
WireConnection;1;2;138;0
WireConnection;5;0;1;0
WireConnection;196;0;197;0
WireConnection;90;0;91;0
WireConnection;90;1;98;0
WireConnection;104;0;106;0
WireConnection;104;1;98;0
WireConnection;85;0;84;0
WireConnection;85;1;98;0
WireConnection;26;0;25;0
WireConnection;16;0;35;0
WireConnection;16;1;26;0
WireConnection;99;0;85;0
WireConnection;45;0;5;0
WireConnection;198;0;196;0
WireConnection;201;0;200;0
WireConnection;105;0;104;0
WireConnection;100;0;90;0
WireConnection;195;1;198;0
WireConnection;94;0;99;0
WireConnection;94;1;100;0
WireConnection;94;2;105;0
WireConnection;36;0;16;0
WireConnection;36;1;201;0
WireConnection;55;0;45;0
WireConnection;6;0;5;0
WireConnection;192;0;195;0
WireConnection;46;0;55;0
WireConnection;46;1;36;0
WireConnection;150;0;148;0
WireConnection;97;0;94;0
WireConnection;3;0;9;0
WireConnection;3;1;2;0
WireConnection;3;2;6;0
WireConnection;107;0;45;0
WireConnection;107;1;97;0
WireConnection;152;0;150;0
WireConnection;52;0;46;0
WireConnection;193;0;192;0
WireConnection;153;0;152;0
WireConnection;110;0;107;0
WireConnection;194;0;52;0
WireConnection;194;1;193;0
WireConnection;12;1;3;0
WireConnection;42;0;202;0
WireConnection;42;1;194;0
WireConnection;28;0;12;0
WireConnection;28;1;76;0
WireConnection;28;2;153;0
WireConnection;108;0;110;0
WireConnection;135;1;118;0
WireConnection;113;0;110;0
WireConnection;113;1;114;0
WireConnection;113;2;112;0
WireConnection;126;0;125;0
WireConnection;191;0;42;0
WireConnection;191;1;179;0
WireConnection;118;0;1;1
WireConnection;118;1;97;0
WireConnection;142;0;1;0
WireConnection;109;0;28;0
WireConnection;109;1;108;0
WireConnection;122;0;141;0
WireConnection;122;1;114;0
WireConnection;122;2;112;0
WireConnection;141;0;142;0
WireConnection;141;1;136;0
WireConnection;119;0;120;0
WireConnection;121;0;120;1
WireConnection;136;0;119;0
WireConnection;120;0;118;0
WireConnection;54;0;115;0
WireConnection;54;1;116;0
WireConnection;54;2;117;0
WireConnection;190;0;109;0
WireConnection;190;2;191;0
ASEEND*/
//CHKSM=40B9B5E155974ECE799EE0EEE97CD67CDFDFC4D7