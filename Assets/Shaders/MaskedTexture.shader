Shader "MaskedTexture" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Mask ("Alpha Mask (R)", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        
//        ZWrite Off // don't write to depth buffer 
        // in order not to occlude other objects
        
//            Blend SrcAlpha OneMinusSrcAlpha // use alpha blending
    
//        CGPROGRAM
//        #pragma surface surf Lambert fullforwardshadows alpha:fade
//        struct Input {
//            float2 uv_MainTex;
//        };
//        sampler2D _MainTex;
//        float _Cutoff;
//        void surf (Input IN, inout SurfaceOutput o) {
//            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
//            o.Alpha = _Cutoff;
//        }
//        ENDCG

        Cull Off // display texture on boths sites of a face

        // write all pixels without transparency (alpha = 1)
        Pass {
            ZWrite On
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            sampler2D _Mask;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_Mask, i.uv);
                if (mask.r != 1) {
                    // don't write any transparent pixels in this pass
                    discard;
                }
                return col;
            }
            ENDCG
        }
        
        // write all pixels with transparency (alpha != 1)
        // TODO this is working mostly fine but far faces are still sometimes in front of near faces
        Pass {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            sampler2D _Mask;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_Mask, i.uv);
                if (mask.r == 1) {
                    discard;
                }
                return fixed4(col.r, col.g, col.b, mask.r); // red channel of grayscale mask as alpha
            }
            ENDCG
        }
    } 
    Fallback "Diffuse"
}
