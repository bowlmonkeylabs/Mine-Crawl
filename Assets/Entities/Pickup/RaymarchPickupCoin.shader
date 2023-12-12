Shader "Custom/RaymarchPickupCoin"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTexTint ("MainTexTint", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Radius ("Coin Radius", float) = 0.15
        _Thickness ("Coin Thickness", float) = 0.02
        _Steps ("Raymarch Steps", int) = 3
        _MinDistance ("Raymarch Min Distance", float) = 0.3 
        _EnableDebug ("Enable Debug", int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		float4 _MainTex_ST;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
        };

        fixed4 _Color;
        fixed4 _MainTexTint;
        half _Glossiness;
        half _Metallic;
        float _Radius;
        float _Thickness;
        int _Steps;
        float _MinDistance;
        int _EnableDebug;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 project(float3 a, float3 b) {
            return b * dot(a, b) / dot(b, b);
        }

        // Cylinder standing on the xy plane
        float fCylinder(float3 p, float r, float height) {
	        float d = length(p.xy) - r;
	        d = max(d, abs(p.z) - height);
	        return d;
        }

        float fCylinder(float3 p, float3 forward, float r, float height) {
            float3 pOnForward = project(p, forward);
	        float d = length(p - pOnForward) - r;
	        d = max(d, abs(length(pOnForward)) - height);
	        return d;
        }
        
        fixed4 raymarch(float3 position, float3 direction)
        {
            fixed4 c;
            
            const float3 cylinderWorldPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            const float3 cylinderForward = float4(0,0,1,0);
            const float3 cylinderWorldForward = normalize(mul(unity_ObjectToWorld, cylinderForward).xyz);
            
            bool didHit = false;
            float distance;
            int stepsToHit = 0;
            for (int i = 0; i < _Steps; i++)
            {
                distance = fCylinder(position - cylinderWorldPos, cylinderWorldForward, _Radius, _Thickness);
                // distance = fCylinder(position - cylinderWorldPos, _Radius, _Thickness);
                if (distance < _MinDistance)
                {
                    stepsToHit = i;
                    didHit = true;
                    break;
                }
                position += (distance * -1 * direction);
            }

            if (didHit)
            {
                // Albedo comes from a texture tinted by color
                float3 sdfPositionLocal = position - mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
                // float3 sdfPositionLocal = mul(unity_WorldToObject., position).xyz;
                sdfPositionLocal = mul(unity_WorldToObject, sdfPositionLocal).xyz;
                // float3 sdfPositionLocalOnCylinderForward = project(sdfPositionLocal, cylinderWorldForward.xyz);
                float2 sdfUv = (sdfPositionLocal.xy) / _Radius / 2;
                // if (sdfPositionLocal.z > 0.001)
                // {
                //     sdfUv.x *= -1;
                // }
                sdfUv = (sdfUv * _MainTex_ST.xy) + _MainTex_ST.zw;
                sdfUv = clamp(sdfUv, 0, 1);

                float mask = tex2D(_MainTex, sdfUv);
                c.xyz = (mask * _MainTexTint) + ((1 - mask) * _Color);
                c.xyz += (_EnableDebug * stepsToHit / (float)_Steps);
                c.w = 1;
            }
            else
            {
                c.w = 0;
            }

            return c;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // float3 cylinderWorldPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            // float3 localPos = IN.worldPos -  mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            
            fixed4 result = raymarch(IN.worldPos, IN.viewDir);
            if (result.w == 0)
            {
                result.xyz = abs(result.xyz);
            }
            o.Albedo = result.xyz;
            o.Alpha = result.w;

            // o.Albedo = viewDirTransformed;
            
            // o.Albedo = dot(IN.viewDir, IN.worldNormal) / 2;
            
            // o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            // o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
