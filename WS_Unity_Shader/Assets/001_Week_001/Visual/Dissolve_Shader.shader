Shader "MyShader/Shader Workshop/Week_001/Dissolve"
{
    Properties
    {
        _MainTexture ("Main Texture", 2D) = "white" {}
        _TextureEdge ("Texture Edge", 2D) = "white" {}
        _TextureNoise ("Texture Noise", 2D) = "white" {}

        _DissolveAmount ("Dissolve Amount", range(0, 1)) = 0

        _EdgeWidth ("Edge Width", range(0, 0.1)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
			#pragma vertex vertexFunc
			#pragma fragment fragmentFunc

			#include "UnityCG.cginc"

            struct appdata
            {
                float4 pos   : POSITION;
				float4 color : COLOR;
				float2 uv    : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            v2f vertexFunc(appdata IN)
            {
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(IN.pos);
                OUT.color = IN.color;
                OUT.uv = IN.uv;

                return OUT;
            }

            sampler2D _MainTexture;
            sampler2D _TextureEdge;
            sampler2D _TextureNoise;

            float _DissolveAmount;

            float _EdgeWidth;

            float4 fragmentFunc(v2f IN) : SV_Target
            {
                float4 OUT = tex2D(_MainTexture, IN.uv);

                float4 edgeColor1 = tex2D(_TextureEdge, IN.uv);
                float4 edgeColor2 = float4(1.0, 0.0, 0.0, 1.0);
                
                float4 noise = tex2D(_TextureNoise, IN.uv);

                float cutout = noise.r;

                //_DissolveAmount = cos( _Time.y) * 0.5 + 0.5;

                clip(cutout - _DissolveAmount);
                //if(cutout < _DissolveAmount) { discard; }

                if (cutout > _EdgeWidth && cutout < _DissolveAmount + _EdgeWidth)
                {
                    OUT = lerp(edgeColor1, edgeColor2, (cutout - _DissolveAmount) / _EdgeWidth);
                    //OUT = edgeColor;
                }

                return OUT;
            }
            ENDCG
        }
    }
}
