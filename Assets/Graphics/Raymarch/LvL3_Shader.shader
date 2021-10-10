Shader "FractalGame/Level3"
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

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            half4 _MainTex_ST;
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);

            // Default RayMarch properties
            uniform float4x4 _InverseProjectionMatrixMiddle;
            uniform float4x4 _CamFrustum, _CamToWorld;
            uniform float _maxDistance;
            uniform float _maxSteps;
            uniform float3 _LightDir, _LightCol;
            uniform float _LightIntensity, _ShadowIntensity, _ShadowPenumbra;
            uniform float2 _ShadowDist;
            uniform float4 _Sphere;
            uniform float4 _Box;
            uniform float3 _modInterval;
            uniform float _Displacement;

            // Capsule and Link properties
            uniform float3 _CapsuleBegin, _CapsuleEnd, _LinkSize;
            uniform fixed4 _mainColor;
            uniform float _Amount, _CapsuleThickness, _OpSmoothness;
            uniform float3 _BoxRotation;
            uniform float pulsation;
            uniform float changeShape;
            uniform float changeShapeAgain;
            uniform float changeShapeAgainAgain;


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

                //p = lerp(p, lastP, changeShape);

                float gyroid = dot(cos(p), sin(p.zxy)) - .05f;

                float3 mod = float3(0, 0, 0);

                float bio = gyroid + 1.3f + dot(sin(p * 1. + pulsation * 6.283 + sin(p.yzx * .5)), float3(.033, .033, .033));

                mod.x = rep2(p.x, 450);
                mod.y = rep2(p.y, 450);
                mod.z = rep2(p.z, 450);

                p = lerp(p, mod, step(0.00001f, changeShape));

                float bioToMenger = lerp(bio, sdMengerSpone(p), changeShape);

                float scale = 1.0;

                float4 orb = float4(0, 0, 0, 0);

                for (int i = 0; i < 8; i++)
                {
                    p = -1.0 + 2.0 * frac(0.5 * p + 0.5); 

                    float r2 = dot(p, p);

                    orb = min(orb, float4(abs(p), r2));

                    float k = 1.5f / r2;
                    p *= k;
                    scale *= k;
                }

                float Web =  0.25 * abs(p.y) / scale;
                
                p = lerp(mod, p, changeShapeAgain);
                
                float result = lerp(bioToMenger, Web, changeShapeAgain);  

                float secondResult = lerp(result, 0, changeShapeAgainAgain);

                return secondResult;
            }

            float3 getNormal(float3 p, float df, float3 rd)
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

                    if (h < 0.0000000001f) return 0;

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
                    float h = mapShapes(ro+rd * t);

                    if (h < 0.0000001f) return 0;

                    t += h;
                //    if (distAlongRay > 0.35f) break;
                }

            //    float h = mapShapes(ro + rd * 2.0f);

                //if (h < 0.0000001f) return 0;

                return 1.0f;
            }

            float3 Shading(float3 p, float3 n)
            {
                // Directional Light
                //float light = (_LightCol * dot(-_LightDir, n) * .5f + .5f) * _LightIntensity;
                float3 light = dot(-_LightDir, n);

               /* float dif = clamp(dot(n, light), 0.f, 1.f);
                float d = mapShapes(p + -_LightDir * 2.0f);
                if (d < 0.001f) dif *= .1f;*/

                //// Shadows
             /*   float shadow = HardShadows(p, -_LightDir, _ShadowDist.x, _ShadowDist.y) * 0.5f + 0.5f;
                shadow = max(0.0, pow(shadow, _ShadowIntensity));
                light *= shadow;*/

                return light;
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

            fixed4 raymarching(float3 ro, float3 rd, float depth)
            {
                fixed4 result = fixed4(1, 1, 1, 1);
                float d = 0; //current distance
                for (int i = 0; i < _maxSteps; i++)
                {
                    if (d > _maxDistance || d >= depth)
                    {
                        result = fixed4(rd, 0);
                        break;
                    }

                    float3 p = ro + rd * d;

                    float df = mapShapes(p);

                    if (df < 0.01)
                    {
                        // shading
                        float3 n = getNormal(p, df, ro);
                        float light = Shading(p, n);

                        lerp(float3(0, 0, 0), _mainColor.rgb, p);
                        float fog = lerp(1, 0.2f, step(_maxDistance - 60, d));

                        float4 totao = AmbientOcclusion(p + n * 0.0001, n);

                        float shininess = 20.0f;
                        float specular_intensity = pow(max(dot(n, light), 0.0), shininess);
                        float3 specular_color = float3(1, 1, 1);
                        specular_color *= specular_intensity;

                        _mainColor.rgb -= totao.xyz * totao.w;
                        result = fixed4(((_mainColor.rgb * light) + specular_color) * fog, 1.0);

                        break;
                    }

                    d += df;
                }

                return result;
            }

            ENDCG

                       Pass // 0 : Middle eye
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