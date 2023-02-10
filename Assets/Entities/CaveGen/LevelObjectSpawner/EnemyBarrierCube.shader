// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "EnemyBarrierCube"
{
	Properties
	{
		_Albedo("Albedo", Color) = (0.5686275,0.9568627,1,1)
		_IntersectionOffset("Intersection Offset", Float) = 0.1
		_IntersectionIntensity("Intersection Intensity", Range( 0 , 3)) = 1
		_NoiseScale("Noise Scale", Float) = 1
		_NoiseIntensity("Noise Intensity", Float) = 1
		_IntersectionNoiseIntensity("Intersection Noise Intensity", Float) = 0
		_IntersectionNoiseScale("Intersection Noise Scale", Float) = 0
		_NoiseSpeed("Noise Speed", Float) = 1
		_VoronoiIntensity("Voronoi Intensity", Float) = 1
		_VoronoiScale("Voronoi Scale", Float) = 5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard alpha:fade keepalpha noshadow novertexlights 
		struct Input
		{
			float4 screenPos;
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float4 _Albedo;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _NoiseSpeed;
		uniform float _NoiseScale;
		uniform float _NoiseIntensity;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _IntersectionOffset;
		uniform float _IntersectionIntensity;
		uniform float _IntersectionNoiseScale;
		uniform float _IntersectionNoiseIntensity;
		uniform float _VoronoiScale;
		uniform float _VoronoiIntensity;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }

		inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }

		inline float valueNoise (float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac( uv );
			f = f* f * (3.0 - 2.0 * f);
			uv = abs( frac(uv) - 0.5);
			float2 c0 = i + float2( 0.0, 0.0 );
			float2 c1 = i + float2( 1.0, 0.0 );
			float2 c2 = i + float2( 0.0, 1.0 );
			float2 c3 = i + float2( 1.0, 1.0 );
			float r0 = noise_randomValue( c0 );
			float r1 = noise_randomValue( c1 );
			float r2 = noise_randomValue( c2 );
			float r3 = noise_randomValue( c3 );
			float bottomOfGrid = noise_interpolate( r0, r1, f.x );
			float topOfGrid = noise_interpolate( r2, r3, f.x );
			float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
			return t;
		}


		float SimpleNoise(float2 UV)
		{
			float t = 0.0;
			float freq = pow( 2.0, float( 0 ) );
			float amp = pow( 0.5, float( 3 - 0 ) );
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3-1));
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3-2));
			t += valueNoise( UV/freq )*amp;
			return t;
		}


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		float2 voronoihash74( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi74( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -2; j <= 2; j++ )
			{
				for ( int i = -2; i <= 2; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash74( n + g );
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
			return F2;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float temp_output_66_0 = ( _SinTime.x * _NoiseSpeed );
			float4 appendResult62 = (float4(( temp_output_66_0 + ase_grabScreenPosNorm.r ) , ( temp_output_66_0 + ase_grabScreenPosNorm.g ) , 0.0 , 0.0));
			float simpleNoise23 = SimpleNoise( appendResult62.xy*_NoiseScale );
			float4 screenColor20 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( ase_grabScreenPosNorm + (( -1.0 * _NoiseIntensity ) + (simpleNoise23 - 0.0) * (_NoiseIntensity - ( -1.0 * _NoiseIntensity )) / (1.0 - 0.0)) ).xy);
			float4 blendOpSrc34 = _Albedo;
			float4 blendOpDest34 = screenColor20;
			float4 lerpBlendMode34 = lerp(blendOpDest34,2.0f*blendOpDest34*blendOpSrc34 + blendOpDest34*blendOpDest34*(1.0f - 2.0f*blendOpSrc34),_Albedo.a);
			o.Albedo = ( saturate( lerpBlendMode34 )).rgb;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float eyeDepth4 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float smoothstepResult14 = smoothstep( 0.0 , 1.0 , ( 1.0 - ( eyeDepth4 - ( ase_screenPos.w - _IntersectionOffset ) ) ));
			float time67 = temp_output_66_0;
			float temp_output_73_0 = ( 10.0 * time67 );
			float3 ase_worldPos = i.worldPos;
			float4 appendResult69 = (float4(( temp_output_73_0 + ase_worldPos.x ) , ( temp_output_73_0 + ase_worldPos.y ) , ( temp_output_73_0 + ase_worldPos.z ) , 0.0));
			float simplePerlin3D52 = snoise( appendResult69.xyz*_IntersectionNoiseScale );
			simplePerlin3D52 = simplePerlin3D52*0.5 + 0.5;
			float4 temp_cast_4 = ((0.0 + (simplePerlin3D52 - 0.0) * (_IntersectionNoiseIntensity - 0.0) / (1.0 - 0.0))).xxxx;
			float time74 = ( 20.0 * time67 );
			float2 voronoiSmoothId74 = 0;
			float2 coords74 = i.uv_texcoord * _VoronoiScale;
			float2 id74 = 0;
			float2 uv74 = 0;
			float voroi74 = voronoi74( coords74, time74, id74, uv74, 0, voronoiSmoothId74 );
			o.Emission = ( max( float4( 0,0,0,1 ) , ( ( smoothstepResult14 * _IntersectionIntensity * _Albedo ) - temp_cast_4 ) ) + ( voroi74 * _VoronoiIntensity * _Albedo ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1454.4;366.1714;1214.4;901.9714;774.6841;-621.5863;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;65;-1788.56,-382.3369;Inherit;False;Property;_NoiseSpeed;Noise Speed;7;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;60;-1792,-544;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;17;-1751.7,34.40001;Inherit;False;1285.286;676.8286;Intersection from depth;7;10;9;12;13;14;15;16;Intersection from depth;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-1600,-448;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;9;-1703.7,338.4001;Inherit;False;440.1714;372.8286;Object depth with offset;3;8;5;7;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;67;-1414.802,-318.8072;Inherit;False;time;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;5;-1655.7,386.4001;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;10;-1703.7,82.39999;Inherit;False;264.1714;164.8286;Camera depth;1;4;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1655.7,594.4;Inherit;False;Property;_IntersectionOffset;Intersection Offset;1;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;-1893.664,937.1645;Inherit;False;67;time;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;53;-1609.948,757.4395;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;7;-1431.7,386.4001;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1657.303,927.6334;Inherit;False;2;2;0;FLOAT;10;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;21;-1656.589,-768;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;4;-1655.7,130.4;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;12;-1223.7,82.39999;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-1385.922,-525.263;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-1387.267,-629.0796;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-1401.648,929.9911;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;70;-1404.068,829.8041;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-1398.648,1031.991;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;62;-1184,-704;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1024,-320;Inherit;False;Property;_NoiseIntensity;Noise Intensity;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1287.8,-470.4001;Inherit;False;Property;_NoiseScale;Noise Scale;3;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1378.034,1121.684;Inherit;False;Property;_IntersectionNoiseScale;Intersection Noise Scale;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;13;-999.7,82.39999;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;69;-1231.068,739.8041;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-1122.034,915.6841;Inherit;False;Property;_IntersectionNoiseIntensity;Intersection Noise Intensity;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-493.668,923.411;Inherit;False;67;time;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;23;-1024,-560;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-784.5676,-338.8873;Inherit;False;2;2;0;FLOAT;-1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;14;-967.7,178.4;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1031.7,434.4001;Inherit;False;Property;_IntersectionIntensity;Intersection Intensity;2;0;Create;True;0;0;0;False;0;False;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;52;-1056.948,799.4395;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-691.5188,-244.0646;Inherit;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;0.5686275,0.9568627,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;76;-331.6952,762.2924;Inherit;False;Property;_VoronoiScale;Voronoi Scale;9;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-661.1594,377.0502;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-240.668,911.411;Inherit;False;2;2;0;FLOAT;20;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;54;-833.0341,762.6841;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;25;-736,-544;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;58;-390.0012,456.3819;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-340.6952,646.2924;Inherit;False;Property;_VoronoiIntensity;Voronoi Intensity;8;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;74;-82.58569,642.7992;Inherit;True;1;0;1;1;1;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-768,-768;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;57;-207.1216,168.5131;Inherit;False;2;0;COLOR;0,0,0,1;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenColorNode;20;-329,-537;Inherit;False;Global;_GrabScreen0;Grab Screen 0;3;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-187.2934,392.598;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;84;96.87049,452.5123;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;34;-347.2198,-303.0277;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-2,-2;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;EnemyBarrierCube;False;False;False;False;False;True;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;66;0;60;1
WireConnection;66;1;65;0
WireConnection;67;0;66;0
WireConnection;7;0;5;4
WireConnection;7;1;8;0
WireConnection;73;1;68;0
WireConnection;12;0;4;0
WireConnection;12;1;7;0
WireConnection;64;0;66;0
WireConnection;64;1;21;2
WireConnection;63;0;66;0
WireConnection;63;1;21;1
WireConnection;71;0;73;0
WireConnection;71;1;53;2
WireConnection;70;0;73;0
WireConnection;70;1;53;1
WireConnection;72;0;73;0
WireConnection;72;1;53;3
WireConnection;62;0;63;0
WireConnection;62;1;64;0
WireConnection;13;0;12;0
WireConnection;69;0;70;0
WireConnection;69;1;71;0
WireConnection;69;2;72;0
WireConnection;23;0;62;0
WireConnection;23;1;19;0
WireConnection;26;1;33;0
WireConnection;14;0;13;0
WireConnection;52;0;69;0
WireConnection;52;1;56;0
WireConnection;15;0;14;0
WireConnection;15;1;16;0
WireConnection;15;2;1;0
WireConnection;81;1;80;0
WireConnection;54;0;52;0
WireConnection;54;4;55;0
WireConnection;25;0;23;0
WireConnection;25;3;26;0
WireConnection;25;4;33;0
WireConnection;58;0;15;0
WireConnection;58;1;54;0
WireConnection;74;1;81;0
WireConnection;74;2;76;0
WireConnection;24;0;21;0
WireConnection;24;1;25;0
WireConnection;57;1;58;0
WireConnection;20;0;24;0
WireConnection;83;0;74;0
WireConnection;83;1;75;0
WireConnection;83;2;1;0
WireConnection;84;0;57;0
WireConnection;84;1;83;0
WireConnection;34;0;1;0
WireConnection;34;1;20;0
WireConnection;34;2;1;4
WireConnection;0;0;34;0
WireConnection;0;2;84;0
ASEEND*/
//CHKSM=7A6BD5A2C00BA0F6EA9BE9D8EF97914F38CA5ED0