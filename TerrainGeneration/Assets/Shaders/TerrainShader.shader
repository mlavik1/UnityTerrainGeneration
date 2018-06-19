Shader "Custom/TerrainShader"
{
	Properties
    {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _GrassTexture("Grass texture", 2D) = "white" {}
        _HighlandTexture("Highland texture", 2D) = "white" {}
        _RockTexture("Rock texture", 2D) = "white" {}
        _SnowTexture("Snow texture", 2D) = "white" {}
	}
	SubShader
    {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert


		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
        sampler2D _GrassTexture;
        sampler2D _HighlandTexture;
        sampler2D _RockTexture;
        sampler2D _SnowTexture;
        float _MaxHeight;

		struct Input
        {
			float2 uv_MainTex;
            float3 vp;
            float3 vn;
		};

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vp = v.vertex.xyz;
            o.vn = v.normal.xyz;
        }

		void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 grassColour = tex2D(_GrassTexture, IN.vp.xz * 0.01f);
            fixed4 highlandColour = tex2D(_HighlandTexture, IN.vp.xz * 0.01f);
            fixed4 rockColour = tex2D(_RockTexture, IN.vp.xz * 0.01f);
            fixed4 snowColour = tex2D(_SnowTexture, IN.vp.xz * 0.01f);
            //rockColour = fixed4(1.0f, 0.0f, 0.0f, 1.0f);
            
            float tGround = min(IN.vp.y / _MaxHeight, 1.0f);
            
            float NdotUp = dot(IN.vn.xyz, fixed3(0.0f, 1.0f, 0.0f));
            float tRock = max(1.0f - NdotUp * 1.0f, 0);
            tRock = smoothstep(0.85f, 1.0f, tRock);
            tRock = tRock > 0.0f ? lerp(0.0f, 1.0f, tRock) : 0.0f;

            fixed4 cGround = grassColour;
            if (IN.vp.y < 2000.0f)
            {
                float tHighland = smoothstep(0.0f, 2000.0f, IN.vp.y);
                cGround = highlandColour * tHighland + grassColour * (1.0f - tHighland);
            }
            else if (IN.vp.y >= 2000.0f)
            {
                float tSnow = smoothstep(2000.0f, 2500.0f, IN.vp.y);
                cGround = snowColour * tSnow + highlandColour * (1.0f - tSnow);
            }
            fixed4 c = cGround * (1.0f - tRock) + rockColour * tRock;
            //c.xyz = fixed3(IN.vn.x, IN.vn.y, IN.vn.z);

            //c.xyz = grassColour;

            o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
