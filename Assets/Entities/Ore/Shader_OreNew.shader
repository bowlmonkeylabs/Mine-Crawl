// Upgrade NOTE: upgraded instancing buffer 'Shader_OreNew' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shader_OreNew"
{
	Properties
	{
		_Albedo("Albedo", Color) = (0.5566038,0.4088009,0.2599235,1)
		_CrackAlbedo("Crack Albedo", Color) = (1,1,1,1)
		_Float2("Float 2", Float) = -1
		_CritPosition("Crit Position", Vector) = (0,0,0,0)
		_VoronoiScale("Voronoi Scale", Float) = 5
		_CrackPosition0("Crack Position 0", Vector) = (0,0,0,0)
		_CrackPosition1("Crack Position 1", Vector) = (0,0,0,0)
		_CrackPosition2("Crack Position 2", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float _VoronoiScale;
		uniform float _Float2;
		uniform float4 _CrackAlbedo;
		uniform float4 _Albedo;

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


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 transform176 = mul(unity_ObjectToWorld,float4( 0,1,0,0 ));
			float dotResult4_g5 = dot( transform176.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g5 = lerp( 0.0 , 20.0 , frac( ( sin( dotResult4_g5 ) * 43758.55 ) ));
			float time1 = lerpResult10_g5;
			float2 voronoiSmoothId1 = 0;
			float2 coords1 = v.texcoord.xy * _VoronoiScale;
			float2 id1 = 0;
			float2 uv1 = 0;
			float voroi1 = voronoi1( coords1, time1, id1, uv1, 0, voronoiSmoothId1 );
			float temp_output_5_0 = ( 1.0 - voroi1 );
			float temp_output_45_0 = (0.0 + (temp_output_5_0 - 0.9) * (1.0 - 0.0) / (1.0 - 0.9));
			float3 ase_vertex3Pos = v.vertex.xyz;
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
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( clampResult110 * _Float2 * ase_vertexNormal );
			v.vertex.w = 1;
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
			float4 color43 = IsGammaSpace() ? float4(0.936,0.8744736,0.013104,1) : float4(0.8605397,0.7378414,0.001014241,1);
			float smoothstepResult55 = smoothstep( 0.9 , 1.0 , temp_output_45_0);
			float3 _CritPosition_Instance = UNITY_ACCESS_INSTANCED_PROP(_CritPosition_arr, _CritPosition);
			float3 normalizeResult26 = normalize( _CritPosition_Instance );
			float dotResult16 = dot( normalizeResult35 , normalizeResult26 );
			float clampResult52 = clamp( ( smoothstepResult55 * (0.0 + (dotResult16 - 0.95) * (1.0 - 0.0) / (1.0 - 0.95)) ) , 0.0 , 1.0 );
			float4 temp_cast_2 = (clampResult52).xxxx;
			float4 blendOpSrc42 = color43;
			float4 blendOpDest42 = temp_cast_2;
			o.Emission = ( saturate( ( blendOpSrc42 * blendOpDest42 ) )).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
841;247;1729;955;2824.569;592.1526;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;66;-1590.128,325.1008;Inherit;False;1965.974;811.125;Comment;6;42;43;52;46;65;64;Critical cracks;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;64;-1540.128,702.3971;Inherit;False;958.6241;400.45;Mask towards "Crit Position";6;31;16;35;26;25;36;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;128;-1979.118,1283;Inherit;False;1991.234;1102.633;Comment;10;97;94;98;87;146;145;144;101;79;89;Mask Crack Positions;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;31;-1490.128,752.3972;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;89;-1552.326,1755.092;Inherit;False;1066.948;284.2551;Mask towards "Crack Position 1";5;90;92;93;91;100;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;79;-1546.36,1448.83;Inherit;False;1062.152;287.2551;Mask towards "Crack Position 0";5;99;85;82;80;84;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;101;-1553.33,2057.073;Inherit;False;1066.948;286.1528;Mask towards "Crack Position 2";5;106;105;104;103;102;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;63;-2262.797,-461.0663;Inherit;False;1251.027;757.0484;Comment;5;6;5;1;138;62;Voronoi;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;103;-1515.04,2115.303;Inherit;False;InstancedProperty;_CrackPosition2;Crack Position 2;7;0;Create;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;80;-1511.866,1507.06;Inherit;False;InstancedProperty;_CrackPosition0;Crack Position 0;5;0;Create;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;35;-1289.127,821.7971;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;92;-1514.036,1813.323;Inherit;False;InstancedProperty;_CrackPosition1;Crack Position 1;6;0;Create;True;0;0;0;True;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-1339.392,1333;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;93;-1309.657,1876.442;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;82;-1307.487,1570.18;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;102;-1310.661,2178.422;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;62;-2212.797,-411.0663;Inherit;False;507.1282;238;Randomize texture by world "up-axis" direction;2;24;176;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;106;-1144.822,2110.755;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-1150.503,1343.598;Inherit;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;0;False;0;False;0.75;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;84;-1141.648,1502.512;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;91;-1143.818,1808.775;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;176;-2149.569,-357.1526;Inherit;False;1;0;FLOAT4;0,1,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;24;-1910.67,-359.3318;Inherit;False;Random Range;-1;;5;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;85;-919.8392,1500.682;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;138;-1896.336,-49.86877;Inherit;False;Property;_VoronoiScale;Voronoi Scale;4;0;Create;True;0;0;0;False;0;False;5;7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;104;-923.0127,2108.924;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;90;-922.0084,1806.944;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;1;-1665.197,-382.6334;Inherit;True;0;0;1;4;1;False;4;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;5;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.Vector3Node;25;-1483.653,914.8473;Inherit;False;InstancedProperty;_CritPosition;Crit Position;3;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ClampOpNode;99;-641.5961,1504.478;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;100;-649.5961,1807.478;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;105;-650.6003,2109.457;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;148;-523.285,-746.3354;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;5;-1466.197,-388.6334;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;65;-1198.637,375.1008;Inherit;False;615.6573;304.5728;Sharpen cracks;2;45;55;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalizeNode;26;-1290.153,907.7469;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;61;-923.7529,-851.9329;Inherit;False;1871.511;1037.927;Comment;7;28;12;2;9;3;71;129;Base Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;94;-378.0079,1647.888;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-871.1528,-591.3323;Inherit;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;0.5566038,0.4088009,0.2599235,1;0.794,0.7201173,0.642346,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-870.1528,-374.332;Inherit;False;Property;_CrackAlbedo;Crack Albedo;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.595,0.5367935,0.4656522,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;97;-158.8839,1648.513;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;16;-1112.314,826.0793;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;150;-297.285,-745.3354;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;129;1.330471,-158.3274;Inherit;False;914.6672;307.4066;Darken cracks in "Crack Positions" Mask;4;109;108;107;110;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;45;-1148.637,425.6737;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;6;-1266.716,-387.6334;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;152;-76.28503,-742.3354;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;-1.3;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;71;-542.7219,-802.0814;Inherit;False;1155.794;361.0718;Darken edges;1;76;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;51.33055,-106.1982;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;55;-836.9797,425.101;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;3;-196.1527,-408.3319;Inherit;True;Darken;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;36;-871.5046,826.2494;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.95;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;153;191.1495,-739.3545;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosterizeNode;12;81.70221,-406.3568;Inherit;True;2;2;1;COLOR;0,0,0,0;False;0;INT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-447.7533,611.8972;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;110;261.662,-104.9208;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;143;530.7997,926.6227;Inherit;False;1929.92;891.5056;Comment;14;118;120;119;136;121;142;114;141;112;113;126;125;135;122;Vertex Displacement Experiment;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;76;342.1132,-738.046;Inherit;False;Constant;_Color1;Color 1;2;0;Create;True;0;0;0;False;0;False;0.1981132,0.1132669,0.06448024,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;127;1328.238,-87.299;Inherit;False;617.5722;357.2172;Tesselation;4;115;54;117;116;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;43;-212.836,418.6025;Inherit;False;Constant;_Color0;Color 0;2;0;Create;True;0;0;0;False;0;False;0.936,0.8744736,0.013104,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;28;372.0377,-405.4874;Inherit;True;Lighten;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;112;1907.794,1461.582;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;114;1923.953,1356.94;Inherit;False;Property;_Float2;Float 2;2;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;108;495.3158,-106.1627;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;52;-230.5354,608.8312;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;2291.973,1232.172;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosterizeNode;135;1286.738,1134.071;Inherit;True;3;2;1;COLOR;0,0,0,0;False;0;INT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;144;-1881.756,1510.719;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;680.9976,-108.3274;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;580.7997,1496.767;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldNormalVector;125;1889.394,1074.1;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;2298.72,1419.579;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;146;-1877.756,2092.719;Inherit;False;FLOAT4;4;0;FLOAT;-0.7;False;1;FLOAT;0.3;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;1680.707,1248.173;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;120;809.7962,1501.129;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ClampOpNode;142;1405.632,976.6229;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCCompareWithRange;136;1291.132,1410.036;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.86;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;1514.296,153.9183;Inherit;False;Constant;_Float5;Float 5;3;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceBasedTessNode;54;1697.811,24.57692;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SmoothstepOpNode;119;985.7963,1338.129;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;1378.238,-37.29879;Inherit;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;0;False;0;False;32;0;1;32;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;116;1515.306,57.15995;Inherit;False;Constant;_Float4;Float 4;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;121;981.7963,1564.129;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;126;2086.822,1073.477;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;145;-1882.756,1771.719;Inherit;False;FLOAT4;4;0;FLOAT;0.7;False;1;FLOAT;0.3;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.IntNode;123;-272.597,1164.019;Inherit;False;Constant;_Health;Health;3;0;Create;True;0;0;0;False;0;False;1;0;False;0;1;INT;0
Node;AmplifyShaderEditor.BlendOpsNode;42;45.89881,523.8005;Inherit;True;Multiply;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2313.181,345.8943;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;Shader_OreNew;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;35;0;31;0
WireConnection;87;0;35;0
WireConnection;93;0;92;0
WireConnection;82;0;80;0
WireConnection;102;0;103;0
WireConnection;106;0;87;0
WireConnection;106;1;102;0
WireConnection;84;0;87;0
WireConnection;84;1;82;0
WireConnection;91;0;87;0
WireConnection;91;1;93;0
WireConnection;24;1;176;0
WireConnection;85;0;84;0
WireConnection;85;1;98;0
WireConnection;104;0;106;0
WireConnection;104;1;98;0
WireConnection;90;0;91;0
WireConnection;90;1;98;0
WireConnection;1;1;24;0
WireConnection;1;2;138;0
WireConnection;99;0;85;0
WireConnection;100;0;90;0
WireConnection;105;0;104;0
WireConnection;5;0;1;0
WireConnection;26;0;25;0
WireConnection;94;0;99;0
WireConnection;94;1;100;0
WireConnection;94;2;105;0
WireConnection;97;0;94;0
WireConnection;16;0;35;0
WireConnection;16;1;26;0
WireConnection;150;0;148;0
WireConnection;45;0;5;0
WireConnection;6;0;5;0
WireConnection;152;0;150;0
WireConnection;107;0;45;0
WireConnection;107;1;97;0
WireConnection;55;0;45;0
WireConnection;3;0;9;0
WireConnection;3;1;2;0
WireConnection;3;2;6;0
WireConnection;36;0;16;0
WireConnection;153;0;152;0
WireConnection;12;1;3;0
WireConnection;46;0;55;0
WireConnection;46;1;36;0
WireConnection;110;0;107;0
WireConnection;28;0;12;0
WireConnection;28;1;76;0
WireConnection;28;2;153;0
WireConnection;108;0;110;0
WireConnection;52;0;46;0
WireConnection;113;0;110;0
WireConnection;113;1;114;0
WireConnection;113;2;112;0
WireConnection;135;1;118;0
WireConnection;109;0;28;0
WireConnection;109;1;108;0
WireConnection;118;0;1;1
WireConnection;118;1;97;0
WireConnection;122;0;141;0
WireConnection;122;1;114;0
WireConnection;122;2;112;0
WireConnection;141;0;142;0
WireConnection;141;1;136;0
WireConnection;120;0;118;0
WireConnection;142;0;1;0
WireConnection;136;0;119;0
WireConnection;54;0;115;0
WireConnection;54;1;116;0
WireConnection;54;2;117;0
WireConnection;119;0;120;0
WireConnection;121;0;120;1
WireConnection;126;0;125;0
WireConnection;42;0;43;0
WireConnection;42;1;52;0
WireConnection;0;0;109;0
WireConnection;0;2;42;0
WireConnection;0;11;113;0
ASEEND*/
//CHKSM=D0299198AB4D202F46EA60B91B67B23BE8765D92