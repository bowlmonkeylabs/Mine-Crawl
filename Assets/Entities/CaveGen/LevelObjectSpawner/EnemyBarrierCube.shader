// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "EnemyBarrierCube"
{
	Properties
	{
		_Color("Color", Color) = (0.5686275,0.9568627,1,1)
		_IntersectionOffset("Intersection Offset", Float) = 0.1
		_IntersectionIntensity("Intersection Intensity", Range( 0 , 3)) = 1
		_NoiseIntensity("NoiseIntensity", Range( 0 , 1)) = 0
		_NoiseScale("Noise Scale", Float) = 0
		_NoiseSpeed("Noise Speed", Float) = 1
		_VoronoiIntensity("Voronoi Intensity", Float) = 1
		_VoronoiScale("Voronoi Scale", Float) = 5
		_PixelResolution("PixelResolution", Int) = 100
		_PlayerPosition("Player Position", Vector) = (0,0,0,0)
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
		#pragma surface surf Standard alpha:fade keepalpha addshadow fullforwardshadows novertexlights 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float4 screenPos;
		};

		uniform float4 _Color;
		uniform int _PixelResolution;
		uniform float _NoiseSpeed;
		uniform float _NoiseScale;
		uniform float3 _PlayerPosition;
		uniform float _VoronoiIntensity;
		uniform float _VoronoiScale;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _IntersectionOffset;
		uniform float _IntersectionIntensity;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _NoiseIntensity;


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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float pixelWidth116 =  1.0f / (float)_PixelResolution;
			float pixelHeight116 = 1.0f / (float)_PixelResolution;
			half2 pixelateduv116 = half2((int)(i.uv_texcoord.x / pixelWidth116) * pixelWidth116, (int)(i.uv_texcoord.y / pixelHeight116) * pixelHeight116);
			float2 break101 = pixelateduv116;
			float mulTime104 = _Time.y * ( 0.0625 * _NoiseSpeed );
			float time67 = mulTime104;
			float4 appendResult100 = (float4(break101.x , break101.y , time67 , 0.0));
			float simplePerlin3D52 = snoise( appendResult100.xyz*_NoiseScale );
			simplePerlin3D52 = simplePerlin3D52*0.5 + 0.5;
			float scrollingNoise108 = simplePerlin3D52;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float clampResult132 = clamp( ( ( 0.01 * scrollingNoise108 ) + distance( ase_vertex3Pos , _PlayerPosition ) ) , 0.0 , 1.0 );
			float smoothstepResult142 = smoothstep( 0.0 , 0.2 , clampResult132);
			float time74 = ( 20.0 * time67 );
			float2 voronoiSmoothId74 = 0;
			float2 coords74 = pixelateduv116 * _VoronoiScale;
			float2 id74 = 0;
			float2 uv74 = 0;
			float voroi74 = voronoi74( coords74, time74, id74, uv74, 0, voronoiSmoothId74 );
			float temp_output_83_0 = ( _VoronoiIntensity * voroi74 );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float eyeDepth4 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float smoothstepResult14 = smoothstep( 0.0 , 1.0 , ( 1.0 - ( eyeDepth4 - ( ase_screenPos.w - _IntersectionOffset ) ) ));
			float clampResult149 = clamp( ( (( smoothstepResult142 >= 0.4 && smoothstepResult142 <= ( 0.4 + 0.07 ) ) ? 1.0 :  0.0 ) + ( smoothstepResult14 * _IntersectionIntensity ) ) , 0.0 , 1.0 );
			float clampResult111 = clamp( ( ( smoothstepResult142 >= 0.4 ? 1.0 : 0.0 ) * ( temp_output_83_0 + max( 0.0 , ( clampResult149 - scrollingNoise108 ) ) ) ) , 0.0 , 1.0 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 screenColor20 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( ase_grabScreenPosNorm + (( -1.0 * _NoiseIntensity ) + (simplePerlin3D52 - 0.0) * (_NoiseIntensity - ( -1.0 * _NoiseIntensity )) / (1.0 - 0.0)) ).xy);
			float4 blendOpSrc34 = ( _Color * clampResult111 );
			float4 blendOpDest34 = screenColor20;
			float4 lerpBlendMode34 = lerp(blendOpDest34,( 1.0 - ( 1.0 - blendOpSrc34 ) * ( 1.0 - blendOpDest34 ) ),_Color.a);
			o.Emission = ( saturate( lerpBlendMode34 )).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
1474;218;2064;1142;3124.428;907.2988;1.3;True;True
Node;AmplifyShaderEditor.CommentaryNode;87;-2637.371,-1501.303;Inherit;False;445.1978;269.6632;Comment;4;67;104;106;65;Time;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-2607.931,-1433.64;Inherit;False;Property;_NoiseSpeed;Noise Speed;5;0;Create;True;0;0;0;False;0;False;1;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;-2577.035,-1348.356;Inherit;False;2;2;0;FLOAT;0.0625;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;122;-2876.562,-1125.288;Inherit;False;509.0537;305.6786;Comment;3;116;118;121;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;118;-2826.562,-1074.801;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.IntNode;121;-2787.513,-935.6096;Inherit;False;Property;_PixelResolution;PixelResolution;8;0;Create;True;0;0;0;False;0;False;100;70;False;0;1;INT;0
Node;AmplifyShaderEditor.SimpleTimeNode;104;-2415.663,-1431.725;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;94;-1971.829,-1428.791;Inherit;False;1039.3;425.7184;Scrolling 3D noise;9;54;107;55;108;52;100;56;102;101;Scrolling 3D noise;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;67;-2414.173,-1349.11;Inherit;False;time;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCPixelate;116;-2583.509,-1075.288;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;100;False;2;FLOAT;100;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;102;-1908.192,-1218.862;Inherit;False;67;time;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;101;-1928.609,-1375.45;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;100;-1598.086,-1376.939;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1640.913,-1202.546;Inherit;False;Property;_NoiseScale;Noise Scale;4;0;Create;True;0;0;0;False;0;False;0;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;52;-1425.827,-1376.791;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;17;-3352.342,25.66751;Inherit;False;952.7275;679.9164;Intersection from depth;7;15;16;14;13;12;10;9;Intersection Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;9;-3315.342,267.6676;Inherit;False;408.1714;336.8286;Object depth with offset;3;7;8;5;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;108;-1143.545,-1374.773;Inherit;False;scrollingNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;159;-3439.806,-743.7471;Inherit;False;1047;688;Comment;13;145;146;147;123;126;127;155;157;153;132;142;143;144;Player Distance;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;123;-3385.806,-439.7472;Inherit;False;Property;_PlayerPosition;Player Position;9;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ScreenPosInputsNode;5;-3288.342,317.6676;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-3287.342,506.6677;Inherit;False;Property;_IntersectionOffset;Intersection Offset;1;0;Create;True;0;0;0;False;0;False;0.1;0.38;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;10;-3312.342,76.66755;Inherit;False;264.1714;164.8286;Camera depth;1;4;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;127;-3388.806,-597.7471;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;155;-3388.806,-693.7471;Inherit;False;108;scrollingNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;126;-3180.806,-485.7471;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;157;-3180.806,-693.7471;Inherit;False;2;2;0;FLOAT;0.01;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;4;-3264.342,124.6676;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;7;-3063.342,444.6677;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;12;-2874.341,90.66758;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;153;-2977.806,-668.7471;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;13;-2812.341,318.6675;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-3389.806,-267.7472;Inherit;False;Constant;_DistanceThreshold;Distance Threshold;10;0;Create;True;0;0;0;False;0;False;0.4;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;146;-3389.806,-171.7472;Inherit;False;Constant;_BorderThickness;Border Thickness;10;0;Create;True;0;0;0;False;0;False;0.07;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;132;-2988.806,-565.7471;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;147;-3101.806,-235.7472;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-3292.342,611.6677;Inherit;False;Property;_IntersectionIntensity;Intersection Intensity;2;0;Create;True;0;0;0;False;0;False;1;1.4;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;14;-2879.341,407.6674;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;142;-2844.806,-613.7471;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;158;-2284.037,-52.10595;Inherit;False;437.6754;304;Combine player distance mask with geometry intersection mask;2;149;148;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;96;-1725.946,-325.2408;Inherit;False;703.8168;472.2269;Voronoi texture;6;75;76;80;81;74;83;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-2632.301,411.5177;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCCompareWithRange;143;-2636.806,-309.7472;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.7;False;2;FLOAT;0.8;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-1674.968,-62.36224;Inherit;False;67;time;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;148;-2234.037,-2.105953;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;95;-1704.957,195.696;Inherit;False;672.6926;290.8;Noisy intersection;3;109;57;58;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;149;-2017.361,30.97756;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-1673.707,-157.418;Inherit;False;Property;_VoronoiScale;Voronoi Scale;7;0;Create;True;0;0;0;False;0;False;5;13.7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-1637.902,11.98603;Inherit;False;2;2;0;FLOAT;20;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-1682.319,391.8418;Inherit;False;108;scrollingNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;74;-1451.915,-184.9754;Inherit;True;1;0;1;1;1;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;58;-1478.45,248.572;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-1675.945,-255.3863;Inherit;False;Property;_VoronoiIntensity;Voronoi Intensity;6;0;Create;True;0;0;0;False;0;False;1;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-1640.913,-1107.546;Inherit;False;Property;_NoiseIntensity;NoiseIntensity;3;0;Create;True;0;0;0;False;0;False;0;0.004;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-1261.847,-255.4089;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;57;-1250.995,247.4285;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;90;-1123.168,-952.8689;Inherit;False;1103.511;291.4141;Sample screen color with noisy texture coordinates to create transparency with added distortion.;4;34;20;24;21;Transparency;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;-1296.107,-1153.217;Inherit;False;2;2;0;FLOAT;-1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;144;-2636.806,-469.7472;Inherit;False;3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;84;-935.8232,-77.25582;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;150;-978.6128,-439.2974;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;54;-1141.913,-1192.546;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;21;-1073.167,-901.8685;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-656.2779,-898.8684;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;1;-917.4191,-640.0269;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;0.5686275,0.9568627,1,1;0.4315879,0.7262589,0.759,0.2;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;111;-729.8281,-462.5818;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-563.1241,-604.2471;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenColorNode;20;-499.4384,-900.2723;Inherit;False;Global;_GrabScreen0;Grab Screen 0;3;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;-357.7615,-207.8162;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;34;-297.4694,-898.3767;Inherit;True;Screen;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;106.3717,-941.8444;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;EnemyBarrierCube;False;False;False;False;False;True;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;106;1;65;0
WireConnection;104;0;106;0
WireConnection;67;0;104;0
WireConnection;116;0;118;0
WireConnection;116;1;121;0
WireConnection;116;2;121;0
WireConnection;101;0;116;0
WireConnection;100;0;101;0
WireConnection;100;1;101;1
WireConnection;100;2;102;0
WireConnection;52;0;100;0
WireConnection;52;1;56;0
WireConnection;108;0;52;0
WireConnection;126;0;127;0
WireConnection;126;1;123;0
WireConnection;157;1;155;0
WireConnection;7;0;5;4
WireConnection;7;1;8;0
WireConnection;12;0;4;0
WireConnection;12;1;7;0
WireConnection;153;0;157;0
WireConnection;153;1;126;0
WireConnection;13;0;12;0
WireConnection;132;0;153;0
WireConnection;147;0;145;0
WireConnection;147;1;146;0
WireConnection;14;0;13;0
WireConnection;142;0;132;0
WireConnection;15;0;14;0
WireConnection;15;1;16;0
WireConnection;143;0;142;0
WireConnection;143;1;145;0
WireConnection;143;2;147;0
WireConnection;148;0;143;0
WireConnection;148;1;15;0
WireConnection;149;0;148;0
WireConnection;81;1;80;0
WireConnection;74;0;116;0
WireConnection;74;1;81;0
WireConnection;74;2;76;0
WireConnection;58;0;149;0
WireConnection;58;1;109;0
WireConnection;83;0;75;0
WireConnection;83;1;74;0
WireConnection;57;1;58;0
WireConnection;107;1;55;0
WireConnection;144;0;142;0
WireConnection;144;1;145;0
WireConnection;84;0;83;0
WireConnection;84;1;57;0
WireConnection;150;0;144;0
WireConnection;150;1;84;0
WireConnection;54;0;52;0
WireConnection;54;3;107;0
WireConnection;54;4;55;0
WireConnection;24;0;21;0
WireConnection;24;1;54;0
WireConnection;111;0;150;0
WireConnection;93;0;1;0
WireConnection;93;1;111;0
WireConnection;20;0;24;0
WireConnection;114;0;83;0
WireConnection;114;1;109;0
WireConnection;34;0;93;0
WireConnection;34;1;20;0
WireConnection;34;2;1;4
WireConnection;0;2;34;0
ASEEND*/
//CHKSM=42722D0400B1ACEED90A4A927E43F7143B307986