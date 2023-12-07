// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Candle"
{
	Properties
	{
		_NoiseStrength("Noise Strength", Range( 0 , 1)) = 1
		_Color("Color", Color) = (0.9529412,0.9411765,0.7686275,1)
		_NoiseScale("Noise Scale", Float) = 6.95
		_EmissionStrength("Emission Strength", Range( 0 , 2)) = 0.7
		_FlickerIntensity("Flicker Intensity", Range( 0 , 10)) = 1
		_FlickerSpeed("Flicker Speed", Range( 0 , 1)) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _Color;
		uniform float _NoiseScale;
		uniform float _NoiseStrength;
		uniform float _EmissionStrength;
		uniform float _FlickerSpeed;
		uniform float _FlickerIntensity;


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


		//https://www.shadertoy.com/view/XdXGW8
		float2 GradientNoiseDir( float2 x )
		{
			const float2 k = float2( 0.3183099, 0.3678794 );
			x = x * k + k.yx;
			return -1.0 + 2.0 * frac( 16.0 * k * frac( x.x * x.y * ( x.x + x.y ) ) );
		}
		
		float GradientNoise( float2 UV, float Scale )
		{
			float2 p = UV * Scale;
			float2 i = floor( p );
			float2 f = frac( p );
			float2 u = f * f * ( 3.0 - 2.0 * f );
			return lerp( lerp( dot( GradientNoiseDir( i + float2( 0.0, 0.0 ) ), f - float2( 0.0, 0.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 0.0 ) ), f - float2( 1.0, 0.0 ) ), u.x ),
					lerp( dot( GradientNoiseDir( i + float2( 0.0, 1.0 ) ), f - float2( 0.0, 1.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 1.0 ) ), f - float2( 1.0, 1.0 ) ), u.x ), u.y );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 temp_output_2_0 = _Color;
			o.Albedo = temp_output_2_0.rgb;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 break4 = ase_vertex3Pos;
			float heightFac59 = (0.0 + (break4.y - -1.0) * (1.0 - 0.0) / (1.0 - -1.0));
			float4 appendResult48 = (float4(break4.x , break4.z , 0.0 , 0.0));
			float2 CenteredUV15_g2 = ( appendResult48.xy - float2( 0,0 ) );
			float2 break17_g2 = CenteredUV15_g2;
			float2 appendResult23_g2 = (float2(( length( CenteredUV15_g2 ) * 1.0 * 2.0 ) , ( atan2( break17_g2.x , break17_g2.y ) * ( 1.0 / 6.28318548202515 ) * 1.0 )));
			float2 break50 = appendResult23_g2;
			float circumferenceFac61 = ( 0.5 + break50.y );
			float4 appendResult62 = (float4(circumferenceFac61 , ( 0.5 * heightFac59 ) , 0.0 , 0.0));
			float simplePerlin2D29 = snoise( appendResult62.xy*_NoiseScale );
			simplePerlin2D29 = simplePerlin2D29*0.5 + 0.5;
			float clampResult67 = clamp( ( heightFac59 - ( (0.0 + (simplePerlin2D29 - 0.0) * (_NoiseStrength - 0.0) / (1.0 - 0.0)) * (0.0 + (( 1.0 - heightFac59 ) - 0.1) * (1.0 - 0.0) / (0.3 - 0.1)) ) ) , 0.0 , 1.0 );
			float mulTime84 = _Time.y * _FlickerSpeed;
			float4 appendResult86 = (float4(mulTime84 , 0.0 , 0.0 , 0.0));
			float gradientNoise85 = GradientNoise(appendResult86.xy,10.0);
			gradientNoise85 = gradientNoise85*0.5 + 0.5;
			float temp_output_93_0 = ( 0.3 * _FlickerIntensity );
			o.Emission = ( _Color * (0.0 + (pow( clampResult67 , 10.43 ) - 0.0) * (_EmissionStrength - 0.0) / (1.0 - 0.0)) * (( ( -1.0 * temp_output_93_0 ) + _EmissionStrength ) + (gradientNoise85 - 0.0) * (( temp_output_93_0 + _EmissionStrength ) - ( ( -1.0 * temp_output_93_0 ) + _EmissionStrength )) / (1.0 - 0.0)) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
1016;992;2064;1148;22.59766;390.2502;1.500948;True;True
Node;AmplifyShaderEditor.CommentaryNode;26;-679.6162,-219.4472;Inherit;False;930.156;454.7669;Map vertex positions to height and dist from center;10;59;60;61;6;58;50;49;48;4;5;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;5;-649.6161,-167.4473;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;4;-456.6161,-168.4473;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;48;-516.3792,31.55075;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;49;-368.3372,29.76596;Inherit;False;Polar Coordinates;-1;;2;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;50;-130.279,17.39476;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.TFHCRemapNode;6;-182.2412,-169.4473;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-127.6388,117.5645;Inherit;False;2;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;32.28528,-81.66423;Inherit;False;heightFac;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;75;-795.4707,309.0767;Inherit;False;1046.513;400.1753;Add noise mapped around the outside of the candle;8;76;77;74;34;35;29;62;33;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-773.4303,443.8645;Inherit;False;59;heightFac;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;19.40951,119.5655;Inherit;False;circumferenceFac;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;71;-225.0871,764.0922;Inherit;False;484.7896;290.9552;Mask to only affect the edges of emission area;3;69;64;73;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-633.2327,358.6899;Inherit;False;61;circumferenceFac;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-555.2686,432.0389;Inherit;False;2;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-539.3246,524.4977;Inherit;False;Property;_NoiseScale;Noise Scale;2;0;Create;True;0;0;0;False;0;False;6.95;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;62;-378.4474,359.0767;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-200.7757,818.9871;Inherit;False;59;heightFac;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;29;-208.1363,363.5439;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;6.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-657.0021,608.2766;Inherit;False;Property;_NoiseStrength;Noise Strength;0;0;Create;True;0;0;0;False;0;False;1;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;64;-181.4976,916.7241;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;95;660.8348,-617.2465;Inherit;False;1075.902;522.4398;Comment;10;88;89;90;85;87;86;84;94;92;93;Flicker;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;33;45.04231,364.6472;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;69;-17.95154,822.4419;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0.3;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;81;583.3749,-23.59659;Inherit;False;1256.898;628.7155;Comment;7;83;20;82;2;27;67;66;Mix;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;92;712.7366,-439.2463;Inherit;False;Property;_FlickerIntensity;Flicker Intensity;4;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;310.6373,506.1275;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;710.8348,-350.6651;Inherit;False;Property;_FlickerSpeed;Flicker Speed;5;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;1032.736,-459.2461;Inherit;False;2;2;0;FLOAT;0.3;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;66;633.3749,218.8379;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;84;814.796,-258.7809;Inherit;False;1;0;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;996.2341,492.9512;Inherit;False;Property;_EmissionStrength;Emission Strength;3;0;Create;True;0;0;0;False;0;False;0.7;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;67;858.3899,218.3664;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;86;1031.834,-351.6651;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;1188.736,-567.2465;Inherit;False;2;2;0;FLOAT;-1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;27;1024.896,224.438;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;10.43;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;94;1325.736,-565.2465;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;89;1324.736,-460.2461;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;85;1188.552,-353.8063;Inherit;True;Gradient;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;1024.529,28.98117;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;0.9529412,0.9411765,0.7686275,1;0.9529411,0.9411765,0.7686275,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;82;1281.234,227.9512;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;88;1446.736,-352.2465;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.6;False;4;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;1566.831,153.5739;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;60;16.40951,20.56549;Inherit;False;centerDistanceFac;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2223.497,-17.7686;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Candle;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;5;0
WireConnection;48;0;4;0
WireConnection;48;1;4;2
WireConnection;49;1;48;0
WireConnection;50;0;49;0
WireConnection;6;0;4;1
WireConnection;58;1;50;1
WireConnection;59;0;6;0
WireConnection;61;0;58;0
WireConnection;76;1;77;0
WireConnection;62;0;74;0
WireConnection;62;1;76;0
WireConnection;29;0;62;0
WireConnection;29;1;34;0
WireConnection;64;0;73;0
WireConnection;33;0;29;0
WireConnection;33;4;35;0
WireConnection;69;0;64;0
WireConnection;65;0;33;0
WireConnection;65;1;69;0
WireConnection;93;1;92;0
WireConnection;66;0;59;0
WireConnection;66;1;65;0
WireConnection;84;0;87;0
WireConnection;67;0;66;0
WireConnection;86;0;84;0
WireConnection;90;1;93;0
WireConnection;27;0;67;0
WireConnection;94;0;90;0
WireConnection;94;1;83;0
WireConnection;89;0;93;0
WireConnection;89;1;83;0
WireConnection;85;0;86;0
WireConnection;82;0;27;0
WireConnection;82;4;83;0
WireConnection;88;0;85;0
WireConnection;88;3;94;0
WireConnection;88;4;89;0
WireConnection;20;0;2;0
WireConnection;20;1;82;0
WireConnection;20;2;88;0
WireConnection;60;0;50;0
WireConnection;0;0;2;0
WireConnection;0;2;20;0
ASEEND*/
//CHKSM=81EF8DFD856D31AD758C4748D0492D9AC9C70DC6