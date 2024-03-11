// Upgrade NOTE: upgraded instancing buffer 'Shader_Mineable' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shader_Mineable"
{
	Properties
	{
		_Color("Color", Color) = (0.7921569,0.7215686,0.6431373,1)
		_CrackAlbedo("Crack Albedo", Color) = (0.5960785,0.5372549,0.4666667,1)
		_EdgeColor("Edge Color", Color) = (0.1981132,0.1132669,0.06448024,1)
		_EmissionStrength("EmissionStrength", Range( 0 , 100)) = 15
		_VoronoiScale("Voronoi Scale", Float) = 5
		_CritCracksSpread("CritCracksSpread", Range( 0 , 1)) = 0.05
		_CritPosition("Crit Position", Vector) = (0,0,0,0)
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
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float4 _CrackAlbedo;
		uniform float4 _Color;
		uniform float _VoronoiScale;
		uniform float4 _EdgeColor;
		uniform float _CritCracksSpread;
		uniform float _EmissionStrength;

		UNITY_INSTANCING_BUFFER_START(Shader_Mineable)
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition1)
#define _CrackPosition1_arr Shader_Mineable
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition2)
#define _CrackPosition2_arr Shader_Mineable
			UNITY_DEFINE_INSTANCED_PROP(float3, _CrackPosition0)
#define _CrackPosition0_arr Shader_Mineable
			UNITY_DEFINE_INSTANCED_PROP(float3, _CritPosition)
#define _CritPosition_arr Shader_Mineable
		UNITY_INSTANCING_BUFFER_END(Shader_Mineable)


		float2 voronoihash50_g30( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi50_g30( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
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
			 		float2 o = voronoihash50_g30( n + g );
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
float2 o = voronoihash50_g30( n + g );
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
			float3 _CrackPosition1_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition1_arr, _CrackPosition1);
			float3 _CrackPosition2_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition2_arr, _CrackPosition2);
			float3 _CrackPosition0_Instance = UNITY_ACCESS_INSTANCED_PROP(_CrackPosition0_arr, _CrackPosition0);
			float4 blendOpSrc68_g30 = _CrackAlbedo;
			float4 blendOpDest68_g30 = _Color;
			float4 transform38_g30 = mul(unity_ObjectToWorld,float4( 0,1,0,0 ));
			float dotResult4_g31 = dot( transform38_g30.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g31 = lerp( 0.0 , 20.0 , frac( ( sin( dotResult4_g31 ) * 43758.55 ) ));
			float time50_g30 = lerpResult10_g31;
			float2 voronoiSmoothId50_g30 = 0;
			float2 coords50_g30 = i.uv_texcoord * _VoronoiScale;
			float2 id50_g30 = 0;
			float2 uv50_g30 = 0;
			float voroi50_g30 = voronoi50_g30( coords50_g30, time50_g30, id50_g30, uv50_g30, 0, voronoiSmoothId50_g30 );
			float temp_output_51_0_g30 = ( 1.0 - voroi50_g30 );
			float clampResult63_g30 = clamp( temp_output_51_0_g30 , 0.0 , 1.0 );
			float4 lerpBlendMode68_g30 = lerp(blendOpDest68_g30,min( blendOpSrc68_g30 , blendOpDest68_g30 ),clampResult63_g30);
			float div75_g30=256.0/float(2);
			float4 posterize75_g30 = ( floor( ( saturate( lerpBlendMode68_g30 )) * div75_g30 ) / div75_g30 );
			float4 blendOpSrc78_g30 = posterize75_g30;
			float4 blendOpDest78_g30 = _EdgeColor;
			float clampResult72_g30 = clamp( (-1.3 + (distance( i.uv_texcoord , float2( 0.5,0.5 ) ) - 1.0) * (1.0 - -1.3) / (0.0 - 1.0)) , 0.0 , 1.0 );
			float4 lerpBlendMode78_g30 = lerp(blendOpDest78_g30,	max( blendOpSrc78_g30, blendOpDest78_g30 ),clampResult72_g30);
			float temp_output_110_0_g30 = (0.0 + (temp_output_51_0_g30 - 0.9) * (1.0 - 0.0) / (1.0 - 0.9));
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 normalizeResult103_g30 = normalize( ase_vertex3Pos );
			float3 temp_output_43_0_g30 = ( normalizeResult103_g30 * float3( 1,1,1 ) );
			float3 normalizeResult42_g30 = normalize( _CrackPosition0_Instance );
			float dotResult47_g30 = dot( temp_output_43_0_g30 , normalizeResult42_g30 );
			float clampResult56_g30 = clamp( (0.0 + (dotResult47_g30 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float3 normalizeResult45_g30 = normalize( _CrackPosition1_Instance );
			float dotResult46_g30 = dot( temp_output_43_0_g30 , normalizeResult45_g30 );
			float clampResult58_g30 = clamp( (0.0 + (dotResult46_g30 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float3 normalizeResult44_g30 = normalize( _CrackPosition2_Instance );
			float dotResult48_g30 = dot( temp_output_43_0_g30 , normalizeResult44_g30 );
			float clampResult57_g30 = clamp( (0.0 + (dotResult48_g30 - 0.75) * (1.0 - 0.0) / (1.0 - 0.75)) , 0.0 , 1.0 );
			float clampResult67_g30 = clamp( ( clampResult56_g30 + clampResult58_g30 + clampResult57_g30 ) , 0.0 , 3.0 );
			float clampResult73_g30 = clamp( ( temp_output_110_0_g30 * clampResult67_g30 ) , 0.0 , 1.0 );
			o.Albedo = ( ( saturate( lerpBlendMode78_g30 )) * ( 1.0 - clampResult73_g30 ) ).rgb;
			float4 color120_g30 = IsGammaSpace() ? float4(1,0.9137255,0.4392157,1) : float4(1,0.8148467,0.1620294,1);
			float smoothstepResult115_g30 = smoothstep( 0.9 , 1.0 , temp_output_110_0_g30);
			float3 _CritPosition_Instance = UNITY_ACCESS_INSTANCED_PROP(_CritPosition_arr, _CritPosition);
			float3 normalizeResult108_g30 = normalize( _CritPosition_Instance );
			float dotResult109_g30 = dot( normalizeResult103_g30 , normalizeResult108_g30 );
			float clampResult118_g30 = clamp( ( smoothstepResult115_g30 * (0.0 + (dotResult109_g30 - ( 1.0 - _CritCracksSpread )) * (1.0 - 0.0) / (1.0 - ( 1.0 - _CritCracksSpread ))) ) , 0.0 , 1.0 );
			float mulTime106_g30 = _Time.y * 0.01;
			float4 appendResult111_g30 = (float4(mulTime106_g30 , 0.0 , 0.0 , 0.0));
			float2 uv_TexCoord113_g30 = i.uv_texcoord + appendResult111_g30.xy;
			float simplePerlin2D116_g30 = snoise( uv_TexCoord113_g30*30.0 );
			simplePerlin2D116_g30 = simplePerlin2D116_g30*0.5 + 0.5;
			float4 temp_cast_3 = (( clampResult118_g30 - (0.0 + (simplePerlin2D116_g30 - 0.0) * (0.3 - 0.0) / (1.0 - 0.0)) )).xxxx;
			float4 blendOpSrc122_g30 = color120_g30;
			float4 blendOpDest122_g30 = temp_cast_3;
			o.Emission = ( ( saturate( ( blendOpSrc122_g30 * blendOpDest122_g30 ) )) * _EmissionStrength ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1693.333;-362;1680.667;989.6667;1129.336;409.0125;1;True;True
Node;AmplifyShaderEditor.ColorNode;7;-765.3362,-28.01245;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;0.7921569,0.7215686,0.6431373,1;0.794,0.7201173,0.642346,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;15;-404.5,-17;Inherit;False;ASF_Mineable;1;;30;4528dfc5529fa3d42a2c3d5f864cb74b;0;1;127;COLOR;0,0,0,0;False;2;COLOR;0;COLOR;126
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Shader_Mineable;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;127;7;0
WireConnection;0;0;15;0
WireConnection;0;2;15;126
ASEEND*/
//CHKSM=4D22330FBC3BC1B12663A7CC1449C0FFC888657D