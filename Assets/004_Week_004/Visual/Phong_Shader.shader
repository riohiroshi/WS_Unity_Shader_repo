Shader "MyShader/Shader Workshop/Week_004/Phong"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _AmbientColor("Ambient Color", Color) = (1, 1, 1, 1)
        _Ambient("Ambient", Range(0, 1)) = 0.1
        _Diffuse("Diffuse", Range(0, 1)) = 0.7
        _Specular("Specular", Range(0, 1)) = 0.25
        _Shininess("Shininess", Range(0.1, 256)) = 32

        _AttenuationLinear("Attenuation Linear", float) =  4.5
        _AttenuationQuadratic("Attenuation Quadratic", float) = 75
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vertexFunc
			#pragma fragment fragmentFunc

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD5;
                float3 normalWS : NORMAL;
            };

            struct VertexInfo
            {
                float4 baseColor;
                float3 normal;
                float3 positionWS;
                float3 viewDir;
                float diffuse;
                float specular;
                float shininess;
            };

            struct LightInfo
            {
                float3 color;
                float3 position;
                float3 direction;
                float intensity;
                int type;
                float spotAngle;
                float innerSpotAngle;
                float range;
            };

            float4 _BaseColor;
            float4 _AmbientColor;
            float _Ambient;
            float _Diffuse;
            float _Specular;
            float _Shininess;

            float _AttenuationLinear;
            float _AttenuationQuadratic;
            
            int g_LightCount;

            float4 g_LightColor[8];
            float4 g_LightPosition[8];
            float4 g_LightDirection[8];
            float4 g_LightParameter[8];

            v2f vertexFunc (appdata IN)
            {
                v2f OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);

                //OUT.normalWS = IN.normalOS;  WHY???

                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                return OUT;
            }

            float3 computeLighting(VertexInfo vertexInfo, LightInfo lightInfo)
            {
                // float3 L = normalize(lightInfo.position - vertexInfo.positionWS);
                // float3 N = vertexInfo.normal;
                // float3 R = dot(N, L) * 2 * N - L;
                // float3 V = vertexInfo.viewDir;
                // float Diffuse = vertexInfo.diffuse * max(0.0f, dot(N, L));
                // float Angle = max(0.0f, dot(R,V));
                // float Specular = vertexInfo.specular * pow(Angle, vertexInfo.shininess);


                float3 lightToVertex = lightInfo.type == 1 ?
                                        lightInfo.direction :
                                        (vertexInfo.positionWS - lightInfo.position);
                
                float lightSqrDis = lightInfo.type == 1 ? 0 : dot(lightToVertex, lightToVertex);
                float lightDis = length(lightToVertex);

                float3 lightToVertexDir = normalize(lightToVertex);
                float diffuse = vertexInfo.diffuse * max(dot(vertexInfo.normal, -lightToVertexDir), 0.0f);

                float3 reflectDir = reflect(lightToVertexDir, vertexInfo.normal);
                float specular = vertexInfo.specular * pow(max(dot(reflectDir, vertexInfo.viewDir), 0.0f), vertexInfo.shininess);

                float attenuationLinear = _AttenuationLinear / lightInfo.range;
                float attenuationQuadratic = _AttenuationQuadratic / (lightInfo.range * lightInfo.range);
                float attenuation = 1.0f / (1.0f + attenuationLinear * lightDis + attenuationQuadratic * lightSqrDis);

                //float attenuation = 1 - saturate(lightSqrDis / (lightInfo.range * lightInfo.range)); //WHY???
                
                float intensity = lightInfo.intensity * attenuation;

                if(lightInfo.type == 2) { intensity *= smoothstep(lightInfo.spotAngle, lightInfo.innerSpotAngle, dot(lightInfo.direction, lightToVertexDir)); }

                return vertexInfo.baseColor * lightInfo.color * (diffuse + specular) * intensity;
            }

            float4 fragmentFunc (v2f IN) : SV_Target
            {
                // //Debugger
                // float4 base;
                // base.rgb = IN.normalWS.xyz;
                // base.a = 1.0f;
                // return base;

                float4 OUT = _BaseColor * _AmbientColor * _Ambient;

                VertexInfo vertexInfo;
                vertexInfo.baseColor = _BaseColor;
                vertexInfo.normal = normalize(IN.normalWS);
                vertexInfo.positionWS = IN.positionWS;
                vertexInfo.viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                vertexInfo.diffuse = _Diffuse;
                vertexInfo.specular = _Specular;
                vertexInfo.shininess = _Shininess;

                for(int i = 0; i < g_LightCount; i++)
                {
                    LightInfo lightInfo;

                    lightInfo.color = g_LightColor[i].rgb;
                    lightInfo.intensity = g_LightColor[i].a;

                    lightInfo.position = g_LightPosition[i].xyz;

                    lightInfo.direction = g_LightDirection[i].xyz;
                    // 0: point light; 1: directional light; 2: spot light
                    lightInfo.type = g_LightDirection[i].w == 1 ? 1 : g_LightParameter[i].x == 1 ? 2 : 0;
                    lightInfo.spotAngle = g_LightParameter[i].y;
                    lightInfo.innerSpotAngle = g_LightParameter[i].z;
                    lightInfo.range = g_LightParameter[i].w;

                    OUT.rgb +=  computeLighting(vertexInfo, lightInfo);
                }

                return OUT;
            }
            ENDHLSL
        }
    }
}