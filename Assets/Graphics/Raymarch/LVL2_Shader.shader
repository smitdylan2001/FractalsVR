Shader "FractalGame/Level2"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

        SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

            CGINCLUDE
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "DistanceFunctions.cginc"

            // Main Camera Texture
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            half4 _MainTex_ST;
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);

            /// Default RayMarch properties
            // Camera variables
            uniform float4x4 _InverseProjectionMatrixMiddle;
            uniform float4x4 _CamFrustum, _CamToWorld;
            uniform float _maxDistance;
            uniform float _maxSteps;     

            // Light & color
            uniform fixed4 _mainColor;
            uniform float3 _LightDir, _LightCol;
            uniform float _LightIntensity, _ShadowIntensity, _ShadowPenumbra;

            // Shadow
            uniform float2 _ShadowDist;

            // Mod interval
            uniform float3 _modInterval;

            // Shape variables
            uniform float4 _Sphere;
            uniform float pulsation;
            uniform float changeShape;
            uniform float _Displacement;
            uniform float _Random;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 ray : TEXCOORD1;

                float4 projPos : TEXCOORD2;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float mapShapes(float3 p)
            {
                float3 mod = float3(0, 0, 0);

                mod.x = rep2(p.x, _modInterval.x);
                mod.y = rep2(p.y, _modInterval.y);
                mod.z = rep2(p.z, _modInterval.z);

                p = lerp(mod, p, changeShape);                

                float sphere = sdSphere(p, _Sphere.w);


                sphere = opDisplace(sphere, p, _Displacement);

                float gyroid = dot(cos(p), sin(p.zxy)) - .05f;

                float bio = gyroid + 1.3f + dot(sin(p * 1. + pulsation * 6.283 + sin(p.yzx * .5)), float3(.033, .033, .033));


                return lerp(sphere, bio, changeShape);
            }

            float3 getNormal(float3 p, float df)
            {
                const float2 offset = float2(0.001, 0.0);
                float3 n = df - float3(
                    mapShapes(p - offset.xyy),
                    mapShapes(p - offset.yxy),
                    mapShapes(p - offset.yyx));
                return normalize(n);
            }

            float SoftShadows(float3 ro, float3 rd, float mint, float maxt, float k)
            {
                float result = 1.0;

                for (float t = mint; t < maxt;)
                {
                    float h = mapShapes(ro + rd * t);

                    if (h < 0.0000001f) return 0;

                    result = min(result, k * h / t);
                    t += h;
                }

                return result;
            }

            float HardShadows(float3 ro, float3 rd, float mint, float maxt)
            {

                float distAlongRay = 0.1f;
                for (float t = mint; t < maxt;)
                {
                    float h = mapShapes(ro + rd * t);

                    if (h < 0.0000001f) return 0;

                    t += h;
                }

                return 1.0f;
            }

            uniform float _AOStepSize, _AOIntensity;
            uniform int _AOIterations;

            float4 AmbientOcclusion(float3 p, float3 n)
            {
                float step = _AOStepSize;
                float4 totao = float4(0.0, 0.0, 0.0, 0.0);
                float dist = 1.0f;
                for (int i = 0; i < 5; i++)
                {
                    float hr = 0.01f + 0.02f * float(i * i);
                    float3 aopos = n * hr;
                    float dd = mapShapes(aopos);
                    float ao = clamp(-(dd - hr), 0.0, 1.0);
                    totao += ao * 1.0 * float4(1.0, 1.0, 1.0, 1.0);

                    dist *= 0.75f;
                }

                const float aoCoef = 0.5f;
                totao.w = 1.0 - clamp(aoCoef * totao.w, 0.0, 1.0);

                return totao;// (1.0 - ao * _AOIntensity);
            }

            float3 Shading(float3 p, float3 n)
            {
                // Directional Light
                //float3 light = (_LightCol * dot(-_LightDir, n) * .5f + .5f) * _LightIntensity;
                float3 light = dot(-_LightDir, n);

                /* float dif = clamp(dot(n, light), 0.f, 1.f);
                 float d = mapShapes(p + -_LightDir * 2.0f);
                 if (d < 0.001f) dif *= .1f;

                 // Shadows
                 float shadow = SoftShadows(p, -_LightDir, _ShadowDist.x, _ShadowDist.y, _ShadowPenumbra) * 0.5f + 0.5f;
                 shadow = max(0.0, pow(shadow, _ShadowIntensity));
                 light *= shadow;
                 */

                return light;
            }

            fixed4 raymarching(float3 ro, float3 rd, float depth)
            {
                fixed4 result = fixed4(1, 1, 1, 1);
                float d = 0; //current distance
                for (int i = 0; i < _maxSteps; i++)
                {

                    //  float fog = lerp(0, 1, step(_maxDistance, d));
                    //  result = lerp(result, fixed4(0.1, 0.1, 0.1, 1), fog * .8);

                    if (d > _maxDistance || d >= depth)
                    {
                        result = fixed4(rd, 0);
                        break;
                    }

                    float3 p = ro + rd * d;

                    float df = mapShapes(p);

                    if (abs(df) < 0.01)
                    {
                        // shading
                        float3 n = getNormal(p, df);
                        float3 light = dot(-_LightDir, n);

                        float4 totao = AmbientOcclusion(p + n * 0.0001, n);

                        float fog = lerp(1, 0, step(_maxDistance - 400, d));
                        fog += 1. - (float(i) / 100);

                        float shininess = 20.0f;
                        float specular_intensity = pow(max(dot(n, light), 0.0), shininess);
                        float3 specular_color = float3(1, 1, 1);
                        specular_color *= specular_intensity;

                        _mainColor.rgb -= totao.xyz * totao.w;
                        result = fixed4(((_mainColor.rgb * light) + specular_color)  * fog, 1.0);

                        break;
                    }

                    d += df;

                }

                return result;
            }

                ENDCG

                Pass // 0 : Switch 3 & 4
                {
                        CGPROGRAM

                         v2f vert(appdata v)
                        {
                            v2f o;
                            UNITY_SETUP_INSTANCE_ID(v);
                            UNITY_INITIALIZE_OUTPUT(v2f, o);
                            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                            half index = v.vertex.z;
                            v.vertex.z = 0;
                            o.vertex = UnityObjectToClipPos(v.vertex);
                            o.uv = v.uv;

                            /////////////////////////////// PROJECTION MATRIX MIDDLE ////////////
                            float4 viewSpace = mul(_InverseProjectionMatrixMiddle, float4(o.uv, 1, 1));
                            float3 rdCam = viewSpace.xyz / viewSpace.w;

                            float4 clip = float4((v.uv.xy * 2.0f - 1.0f) * float2(1, -1), 0.0f, 1.0f);

                            o.ray = _CamFrustum[(int)index].xyz;

                            o.ray /= abs(o.ray.z);

                            o.ray = mul(_CamToWorld, clip) - _WorldSpaceCameraPos;

                            o.projPos = ComputeScreenPos(o.vertex);
                            COMPUTE_EYEDEPTH(o.projPos.z);

                            return o;
                        }

                        fixed4 frag(v2f i) : SV_Target
                        {
                                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r);
                                depth *= length(i.ray);
                                fixed3 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));

                                float3 rayDirection = normalize(i.ray);
                                float3 rayOrigin = _WorldSpaceCameraPos;

                                fixed4 result = raymarching(rayOrigin, rayDirection, depth);

                                return fixed4(col * (1.0 - result.w) + result.xyz * result.w, 1.0);

                        }
                        ENDCG
                }
    }
}