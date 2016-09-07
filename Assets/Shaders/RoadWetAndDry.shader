/*
* Copyright (C) 2016, Jaguar Land Rover
* This program is licensed under the terms and conditions of the
* Mozilla Public License, version 2.0.  The full text of the
* Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
*/

Shader "Custom/Road Wet and Dry" {
    Properties {
    	_Color ("Main Color", Color) = (1,1,1,1)
    	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_WetColor("Wet Color", Color) = (1,1,1,1)
		_WetSpecColor ("Wet Specular Color", Color) = (1,1,1,1)
   	 	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
    	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
    	_BumpMap ("Normalmap", 2D) = "bump" {}
		_Gloss ("Gloss", Range(0, 8)) = 0
    }
    SubShader {
    	Tags { "RenderType"="Opaque" }
//    	Tags { "Queue" = "Geometry+10" }
    	LOD 400
		
     	Offset -1,-1
		
     	
	    CGPROGRAM
//		#include Lighting.cginc
	    #pragma surface surf CustomBlinnPhong
	    sampler2D _MainTex;
	    sampler2D _BumpMap;
	    fixed4 _Color;
		fixed4 _WetColor;
		fixed4 _WetSpecColor;
	    half _Shininess;
		float _WetRoads;
		float _Gloss;

		inline fixed4 LightingCustomBlinnPhong (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			fixed4 specColor = lerp(_SpecColor, _WetSpecColor, _WetRoads); 
			half3 h = normalize (lightDir + viewDir);
	
			fixed diff = max (0, dot (s.Normal, lightDir));
	
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, s.Specular*128.0) * s.Gloss;
	
			fixed4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * specColor.rgb * spec) * (atten * 2);
			c.a = s.Alpha + _LightColor0.a * specColor.a * spec * atten;
			return c;
		}

		inline fixed4 LightingCustomBlinnPhong_PrePass (SurfaceOutput s, half4 light)
		{
			fixed4 specColor = lerp(_SpecColor, _WetSpecColor, _WetRoads); 
			fixed spec = light.a * s.Gloss;
	
			fixed4 c;
			c.rgb = (s.Albedo * light.rgb + light.rgb * specColor.rgb * spec);
			c.a = s.Alpha + spec * specColor.a;
			return c;
		}

		inline half4 LightingCustomBlinnPhong_DirLightmap (SurfaceOutput s, fixed4 color, fixed4 scale, half3 viewDir, bool surfFuncWritesNormal, out half3 specColor)
		{
			UNITY_DIRBASIS
			half3 scalePerBasisVector;
				
			fixed4 cspecColor = lerp(_SpecColor, _WetSpecColor, _WetRoads); 
			half3 lm = DirLightmapDiffuse (unity_DirBasis, color, scale, s.Normal, surfFuncWritesNormal, scalePerBasisVector);
	
			half3 lightDir = normalize (scalePerBasisVector.x * unity_DirBasis[0] + scalePerBasisVector.y * unity_DirBasis[1] + scalePerBasisVector.z * unity_DirBasis[2]);
			half3 h = normalize (lightDir + viewDir);

			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, s.Specular * 128.0);
	
			// specColor used outside in the forward path, compiled out in prepass
			specColor = lm * cspecColor.rgb * s.Gloss * spec;
	
			// spec from the alpha component is used to calculate specular
			// in the Lighting*_Prepass function, it's not used in forward
			return half4(lm, spec);
		}


	    struct Input {
	    	float2 uv_MainTex;
	    	float2 uv_BumpMap;
	    };
     
    	void surf (Input IN, inout SurfaceOutput o) {
	    	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 calcColor = lerp(_Color, _WetColor, _WetRoads);
	    	o.Albedo = tex.rgb * calcColor.rgb;
	    	o.Gloss = _Gloss;
	    	o.Alpha = tex.a * calcColor.a;
	    	o.Specular = _Shininess;
	    	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	    }
	    ENDCG
    }
    FallBack "Diffuse"
}
