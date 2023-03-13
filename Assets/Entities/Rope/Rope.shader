// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Rope"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Roughness("Roughness", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_AO("AO", 2D) = "white" {}
		_Tiling("Tiling", Vector) = (1,1,0,0)
		_Endcapcolor("End cap color", Color) = (0.7924528,0.5790551,0.4154503,1)
		_Lighten("Lighten", Color) = (0,0,0,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _Normal;
		uniform float2 _Tiling;
		uniform float4 _Endcapcolor;
		uniform float4 _Lighten;
		uniform sampler2D _Albedo;
		uniform sampler2D _Roughness;
		uniform sampler2D _AO;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float4 appendResult21 = (float4(0.0 , 1.0 , 0.0 , 0.0));
			float dotResult20 = dot( float4( ase_vertexNormal , 0.0 ) , appendResult21 );
			float temp_output_33_0 = abs( dotResult20 );
			float2 uv_TexCoord6 = i.uv_texcoord * _Tiling;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
			float4 appendResult10 = (float4(uv_TexCoord6.x , ( _Tiling.y * ( ase_vertex3Pos.y * ase_objectScale.y ) ) , 0.0 , 0.0));
			float3 ifLocalVar27 = 0;
			if( temp_output_33_0 >= 0.99 )
				ifLocalVar27 = ase_vertexNormal;
			else
				ifLocalVar27 = UnpackNormal( tex2D( _Normal, appendResult10.xy ) );
			o.Normal = ifLocalVar27;
			float4 blendOpSrc37 = _Lighten;
			float4 blendOpDest37 = tex2D( _Albedo, appendResult10.xy );
			float4 ifLocalVar22 = 0;
			if( temp_output_33_0 >= 0.99 )
				ifLocalVar22 = _Endcapcolor;
			else
				ifLocalVar22 = ( saturate( 	max( blendOpSrc37, blendOpDest37 ) ));
			o.Albedo = ifLocalVar22.rgb;
			float4 color29 = IsGammaSpace() ? float4(0,0,0,1) : float4(0,0,0,1);
			float4 ifLocalVar28 = 0;
			if( temp_output_33_0 >= 0.99 )
				ifLocalVar28 = color29;
			else
				ifLocalVar28 = ( 1.0 - tex2D( _Roughness, appendResult10.xy ) );
			o.Smoothness = ifLocalVar28.r;
			float4 color31 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
			float4 ifLocalVar30 = 0;
			if( temp_output_33_0 >= 0.99 )
				ifLocalVar30 = color31;
			else
				ifLocalVar30 = tex2D( _AO, appendResult10.xy );
			o.Occlusion = ifLocalVar30.r;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
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
-392.9143;-837.2572;1097.829;625.6286;702.0526;536.183;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;15;-1438.203,130.7268;Inherit;False;755.1428;515.8572;Adjust for object y-scale;7;11;12;6;8;10;14;36;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ObjectScaleNode;11;-1196.203,468.7268;Inherit;False;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;8;-1196.203,308.7268;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;14;-1388.203,180.7268;Inherit;False;Property;_Tiling;Tiling;4;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-1004.203,372.7267;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-1196.203,180.7268;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-864,480;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;25;-828.5403,-437.5946;Inherit;False;509.9289;391.3092;Comment;4;33;20;19;21;End caps;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;10;-844.2029,308.7268;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;21;-762.5403,-227.5947;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NormalVertexDataNode;19;-778.5403,-387.5947;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;20;-586.5403,-387.5947;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;38;-238.5052,-454.2854;Inherit;False;Property;_Lighten;Lighten;6;0;Create;True;0;0;0;False;0;False;0,0,0,1;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-512,0;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;-1;b54caaad00123b84d84a8cd5c36e8fe7;b54caaad00123b84d84a8cd5c36e8fe7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-512,384;Inherit;True;Property;_Roughness;Roughness;1;0;Create;True;0;0;0;False;0;False;-1;95e887ec4ca33a44db6c2ba30accf2d1;95e887ec4ca33a44db6c2ba30accf2d1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;37;6.884216,-326.5413;Inherit;True;Lighten;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.AbsOpNode;33;-455.6914,-391.3214;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;31;-208,576;Inherit;False;Constant;_Color2;Color 2;6;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.7924528,0.5790551,0.4154503,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;32;-208,96;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-208,-96;Inherit;False;Property;_Endcapcolor;End cap color;5;0;Create;True;0;0;0;False;0;False;0.7924528,0.5790551,0.4154503,1;0.7924528,0.5790551,0.4154503,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;29;-208,288;Inherit;False;Constant;_Color1;Color 1;6;0;Create;True;0;0;0;False;0;False;0,0,0,1;0.7924528,0.5790551,0.4154503,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;-512,576;Inherit;True;Property;_AO;AO;3;0;Create;True;0;0;0;False;0;False;-1;95e887ec4ca33a44db6c2ba30accf2d1;95e887ec4ca33a44db6c2ba30accf2d1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;18;-224,480;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-512,192;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;0;False;0;False;-1;6dc23c514b940cb4c83e4df46a04b766;6dc23c514b940cb4c83e4df46a04b766;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;22;16,-96;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0.99;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;27;16,96;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0.99;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ConditionalIfNode;28;16,288;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0.99;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;30;16,576;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0.99;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;256,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Rope;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;8;2
WireConnection;12;1;11;2
WireConnection;6;0;14;0
WireConnection;36;0;14;2
WireConnection;36;1;12;0
WireConnection;10;0;6;1
WireConnection;10;1;36;0
WireConnection;20;0;19;0
WireConnection;20;1;21;0
WireConnection;1;1;10;0
WireConnection;2;1;10;0
WireConnection;37;0;38;0
WireConnection;37;1;1;0
WireConnection;33;0;20;0
WireConnection;5;1;10;0
WireConnection;18;0;2;0
WireConnection;3;1;10;0
WireConnection;22;0;33;0
WireConnection;22;2;23;0
WireConnection;22;3;23;0
WireConnection;22;4;37;0
WireConnection;27;0;33;0
WireConnection;27;2;32;0
WireConnection;27;3;32;0
WireConnection;27;4;3;0
WireConnection;28;0;33;0
WireConnection;28;2;29;0
WireConnection;28;3;29;0
WireConnection;28;4;18;0
WireConnection;30;0;33;0
WireConnection;30;2;31;0
WireConnection;30;3;31;0
WireConnection;30;4;5;0
WireConnection;0;0;22;0
WireConnection;0;1;27;0
WireConnection;0;4;28;0
WireConnection;0;5;30;0
ASEEND*/
//CHKSM=5E9AB6DC6509214BCF66EAEF60D71A8C622CEC94