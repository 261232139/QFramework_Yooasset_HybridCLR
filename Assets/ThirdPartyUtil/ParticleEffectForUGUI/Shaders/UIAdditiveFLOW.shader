Shader "UI/IconFlowAdvanced_Optimized"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // 流光效果参数
        _FlowTex ("流光贴图", 2D) = "white" {}
        _FlowColor ("流光颜色", Color) = (1,1,1,1)
        _TimeOffset ("时间偏移量", Range(0, 10)) = 1
        _FlowSpeed ("流光速度", Range(0, 5)) = 1
        _FlowIntensity ("流光强度", Range(0, 5)) = 1
        _FlowDirection ("流光方向", Vector) = (1, 0, 0, 0)
        _FlowRotation ("纹理旋转", Range(0, 360)) = 0
        [Enum(Add,0,AlphaBlend,1,Multiply,2)] _FlowBlendMode ("混合模式", Float) = 0

        // Unity UI 必备
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [_UnityGUIZTestMode]
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex         : SV_POSITION;
                fixed4 color          : COLOR;
                half2 texcoord        : TEXCOORD0;
                half2 flowUV          : TEXCOORD1;
                float4 worldPosition  : TEXCOORD2;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _FlowTex;
            float4 _FlowTex_ST;
            fixed4 _FlowColor;
            half  _FlowSpeed;
            half  _TimeOffset;
            half  _FlowIntensity;
            half2 _FlowDirection;
            half  _FlowRotation;
            half  _FlowBlendMode;

            float4 _ClipRect;

            // 旋转 UV 函数
            float2 rotateUV(float2 uv, half rotation)
            {
                float2 center = float2(0.5, 0.5);
                uv -= center;
                half s = sin(rotation);
                half c = cos(rotation);
                float2x2 rMatrix = float2x2(c, -s, s, c);
                uv = mul(uv, rMatrix);
                uv += center;
                return uv;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);

                // 流光 UV：旋转 + 位移
                OUT.flowUV = TRANSFORM_TEX(IN.texcoord, _FlowTex);
                half rad = radians(_FlowRotation);
                OUT.flowUV = rotateUV(OUT.flowUV, rad);
                // OUT.flowUV += _FlowDirection * (_Time.y + _TimeOffset) * _FlowSpeed;
                // 限制 _Time.y 的范围，防止累积过大
                float safeTime = fmod(_Time.y + _TimeOffset, 2.45); // 2.45 秒循环
                OUT.flowUV += _FlowDirection * safeTime * _FlowSpeed;

                #ifdef UNITY_HALF_TEXEL_OFFSET
                OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1,1);
                #endif

                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 主纹理
                half4 baseColor = tex2D(_MainTex, IN.texcoord) * IN.color;

                // 只采一次 Flow 贴图的 R 通道
                half flowR = tex2D(_FlowTex, IN.flowUV).r;
                half flowMix = saturate(flowR * _FlowIntensity);

                // 无分支的混合模式选择
                half isAdd   = step(_FlowBlendMode, 0.5);
                half isAlpha = step(0.5, _FlowBlendMode) - step(1.5, _FlowBlendMode);
                half isMul   = 1.0 - isAdd - isAlpha;

                // 三种结果
                half3 addRes   = saturate(baseColor.rgb + _FlowColor.rgb * flowMix);
                half3 alphaRes = lerp(baseColor.rgb, _FlowColor.rgb, flowMix);
                half3 mulRes   = baseColor.rgb * lerp(1.0, _FlowColor.rgb, flowMix);

                // 线性组合，避免分支
                half3 finalRGB = isAdd * addRes + isAlpha * alphaRes + isMul * mulRes;

                // UI 裁剪
                half alpha = baseColor.a * UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.01);
                #endif

                return half4(finalRGB, alpha);
            }
            ENDCG
        }
    }
}
