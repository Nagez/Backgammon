Shader "UI/Gradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.16, 0.11, 0.08, 1)
        _BottomColor ("Bottom Color", Color) = (0.05, 0.03, 0.02, 1)
        _Angle ("Angle", Range(0, 360)) = 90
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            fixed4 _TopColor;
            fixed4 _BottomColor;
            float  _Angle;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float rad = radians(_Angle);
                float2 dir = float2(cos(rad), sin(rad));
                float t = dot(i.uv - 0.5, dir) + 0.5;
                return lerp(_BottomColor, _TopColor, saturate(t));
            }
            ENDCG
        }
    }
}