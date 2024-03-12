// Upgrade NOTE: upgraded instancing buffer 'MushroomCap' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MushroomCap"
{
	Properties
	{
		_Mask("Mask", 2D) = "white" {}
		_CrackAlbedo("Crack Albedo", Color) = (0.5960785,0.5372549,0.4666667,1)
		_EmissionStrength("EmissionStrength", Range( 0 , 100)) = 15
		_EdgeColor("Edge Color", Color) = (0.1981132,0.1132669,0.06448024,1)
		_VoronoiScale("Voronoi Scale", Float) = 5
		_CritCracksSpread("CritCracksSpread", Range( 0 , 1)) = 0.05
		_CritPosition("Crit Position", Vector) = (0,0,0,0)
		_CrackPosition0("Crack Position 0", Vector) = (0,0,0,0)
		_CrackPosition1("Crack Position 1", Vector) = (0,0,0,0)
		_CrackPosition2("Crack Position 2", Vector) = (0,0,0,0)
		_Color1("Color 1", Color) = (1,0,0,1)
		_Color2("Color 2", Color) = (1,1,1,1)
		_Smoothness("Smoothness", Float) = 0
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
		uniform sampler2D _Mask;
		uniform float _VoronoiScale;
		uniform float4 _EdgeColor;
		uniform float _CritCracksSpread;
		uniform float _EmissionStrength;
		uniform float _Smoothness;

		UNITY_INSTANCING_BUFFER_START(MushroomCap)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color2)
#define _Color2_arr MushroomCap
			UNITY_DEFINE_INSTANCED_PROP(float4, _Mask_ST)
#define _Mask_ST_arr MushroomCap
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color1)
#define _Color1_arr MushroomCap
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition0)
#define _CrackPosition0_arr MushroomCap
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition2)
#define _CrackPosition2_arr MushroomCap
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition1)
#define _CrackPosition1_arr MushroomCap
			UNITY_DEFINE_INSTANCED_PROP(float3, _CritPosition)
#define _CritPosition_arr MushroomCap
		UNITY_INSTANCING_BUFFER_END(MushroomCap)


		float2 voronoihash50_g12( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi50_g12( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
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
			 		float2 o = voronoihash50_g12( n + g );
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
float2 o = voronoihash50_g12( n + g );
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
			float3 _CrackPosition0_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition0_arr, _CrackPosition0);
			float3 _CrackPosition2_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition2_arr, _CrackPosition2);
			float3 _CrackPosition1_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition1_arr, _CrackPosition1);
			float4 _Color2_Instance = UNITY_ACCESS_INSTANCED_PROP(_Color2_arr, _Color2);
			float4 _Mask_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_Mask_ST_arr, _Mask_ST);
			float2 uv_Mask = i.uv_texcoord * _Mask_ST_Instance.xy + _Mask_ST_Instance.zw;
			float4 tex2DNode1 = tex2D( _Mask, uv_Mask );
			float4 _Color1_Instance = UNITY_ACCESS_INSTANCED_PROP(_Color1_arr, _Color1);
			float4 blendOpSrc68_g12 = _CrackAlbedo;
			float4 blendOpDest68_g12 = ( ( _Color2_Instance * tex2DNode1 ) + ( ( 1.0 - tex2DNode1.r ) * _Color1_Instance ) );
			float4 transform38_g12 = mul(unity_ObjectToWorld,float4( 0,1,0,0 ));
			float dotResult4_g13 = dot( transform38_g12.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g13 = lerp( 0.0 , 20.0 , frac( ( sin( dotResult4_g13 ) * 43758.55 ) ));
			float time50_g12 = lerpResult10_g13;
			float2 voronoiSmoothId50_g12 = 0;
			float2 coords50_g12 = i.uv_texcoord * _VoronoiScale;
			float2 id50_g12 = 0;
			float2 uv50_g12 = 0;
			float voroi50_g12 = voronoi50_g12( coords50_g12, time50_g12, id50_g12, uv50_g12, 0, voronoiSmoothId50_g12 );
			float temp_output_51_0_g12 = ( 1.0 - voroi50_g12 );
			float clampResult63_g12 = clamp( temp_output_51_0_g12 , 0.0 , 1.0 );
			float4 lerpBlendMode68_g12 = lerp(blendOpDest68_g12,min( blendOpSrc68_g12 , blendOpDest68_g12 ),clampResult63_g12);
			float div75_g12=256.0/float(2);
			float4 posterize75_g12 = ( floor( ( saturate( lerpBlendMode68_g12 )) * div75_g12 ) / div75_g12 );
			float4 blendOpSrc78_g12 = posterize75_g12;
			float4 blendOpDest78_g12 = _EdgeColor;
			float clampResult72_g12 = clamp( (-1.3 + (distance( i.uv_texcoord , float2( 0.5,0.5 ) ) - 1.0) * (1.0 - -1.3) / (0.0 - 1.0)) , 0.0 , 1.0 );
			float4 lerpBlendMode78_g12 = lerp(blendOpDest78_g12,	max( blendOpSrc78_g12, blendOpDest78_g12 ),clampResult72_g12);
			float temp_output_110_0_g12 = (0.0 + (temp_output_51_0_g12 - 0.9) * (1.0 - 0.0) / (1.0 - 0.9));
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 normalizeResult103_g12 = normalize( ase_vertex3Pos );
			float3 temp_output_43_0_g12 = ( normalizeResult103_g12 * float3( 1,1,1 ) );
			float3 normalizeResult42_g12 = normalize( _CrackPosition0_Instance );
			float dotResult47_g12 = dot( temp_output_43_0_g12 , normalizeResult42_g12 );
			float clampResult56_g12 = clamp( (0.0 + (dotResult47_g12 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float3 normalizeResult45_g12 = normalize( _CrackPosition1_Instance );
			float dotResult46_g12 = dot( temp_output_43_0_g12 , normalizeResult45_g12 );
			float clampResult58_g12 = clamp( (0.0 + (dotResult46_g12 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float3 normalizeResult44_g12 = normalize( _CrackPosition2_Instance );
			float dotResult48_g12 = dot( temp_output_43_0_g12 , normalizeResult44_g12 );
			float clampResult57_g12 = clamp( (0.0 + (dotResult48_g12 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float clampResult67_g12 = clamp( ( clampResult56_g12 + clampResult58_g12 + clampResult57_g12 ) , 0.0 , 3.0 );
			float clampResult73_g12 = clamp( ( temp_output_110_0_g12 * clampResult67_g12 ) , 0.0 , 1.0 );
			o.Albedo = ( ( saturate( lerpBlendMode78_g12 )) * ( 1.0 - clampResult73_g12 ) ).rgb;
			float4 color120_g12 = IsGammaSpace() ? float4(1,0.9137255,0.4392157,1) : float4(1,0.8148467,0.1620294,1);
			float smoothstepResult115_g12 = smoothstep( 0.9 , 1.0 , temp_output_110_0_g12);
			float3 _CritPosition_Instance = UNITY_ACCESS_INSTANCED_PROP(_CritPosition_arr, _CritPosition);
			float3 normalizeResult108_g12 = normalize( _CritPosition_Instance );
			float dotResult109_g12 = dot( normalizeResult103_g12 , normalizeResult108_g12 );
			float clampResult118_g12 = clamp( ( smoothstepResult115_g12 * (0.0 + (dotResult109_g12 - ( 1.0 - _CritCracksSpread )) * (1.0 - 0.0) / (1.0 - ( 1.0 - _CritCracksSpread ))) ) , 0.0 , 1.0 );
			float mulTime106_g12 = _Time.y * 0.01;
			float4 appendResult111_g12 = (float4(mulTime106_g12 , 0.0 , 0.0 , 0.0));
			float2 uv_TexCoord113_g12 = i.uv_texcoord + appendResult111_g12.xy;
			float simplePerlin2D116_g12 = snoise( uv_TexCoord113_g12*30.0 );
			simplePerlin2D116_g12 = simplePerlin2D116_g12*0.5 + 0.5;
			float4 temp_cast_3 = (( clampResult118_g12 - (0.0 + (simplePerlin2D116_g12 - 0.0) * (0.3 - 0.0) / (1.0 - 0.0)) )).xxxx;
			float4 blendOpSrc122_g12 = color120_g12;
			float4 blendOpDest122_g12 = temp_cast_3;
			o.Emission = ( ( saturate( ( blendOpSrc122_g12 * blendOpDest122_g12 ) )) * _EmissionStrength ).rgb;
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
-1693.333;-362;1680.667;989.6667;1066.241;380.186;1.101623;True;True
Node;AmplifyShaderEditor.SamplerNode;1;-640,-34;Inherit;True;Property;_Mask;Mask;0;0;Create;True;0;0;0;False;0;False;-1;a723da023fe193849afa3789ee33c0b1;a723da023fe193849afa3789ee33c0b1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;6;-262,74;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-638,-257;Inherit;False;InstancedProperty;_Color2;Color 2;14;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.972549,0.9686274,0.9411765,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;5;-640,224;Inherit;False;InstancedProperty;_Color1;Color 1;13;0;Create;True;0;0;0;False;0;False;1,0,0,1;0.8313726,0.2156862,0.2156862,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-240,-128;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-256,160;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-32,16;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;336,400;Inherit;False;Property;_Smoothness;Smoothness;15;0;Create;True;0;0;0;False;0;False;0;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;368,256;Inherit;False;Property;_Metallic;Metallic;16;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;16;258.2032,62.84546;Inherit;False;ASF_Mineable;1;;12;4528dfc5529fa3d42a2c3d5f864cb74b;0;1;127;COLOR;0,0,0,0;False;2;COLOR;0;COLOR;126
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;640,32;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;MushroomCap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;1;1
WireConnection;3;0;2;0
WireConnection;3;1;1;0
WireConnection;7;0;6;0
WireConnection;7;1;5;0
WireConnection;8;0;3;0
WireConnection;8;1;7;0
WireConnection;16;127;8;0
WireConnection;0;0;16;0
WireConnection;0;2;16;126
WireConnection;0;4;9;0
ASEEND*/
//CHKSM=3FD5F4164E3057155295DE45EE0CFB0538B7AA77