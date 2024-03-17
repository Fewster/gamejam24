Shader "Unlit/InstancedEntityShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float hash : TEXCOORD1;
                float facingDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            
            
            struct Agent{
                float2 position;
                float2 velocity;
                float hash;
                int path;
                int goal;
            }; 

            StructuredBuffer<Agent> AgentBuffer;

#define PI 3.141592

            v2f vert (appdata v, uint svInstanceID : SV_InstanceID)
            {
                v2f o;

                float4 vertex = v.vertex;
                v.vertex.xyz *= 0.5;
                float4 wPos = mul(unity_ObjectToWorld, v.vertex);
                wPos.xy += AgentBuffer[svInstanceID].position;// + float3(0,svInstanceID,0);
                wPos.z += AgentBuffer[svInstanceID].position.y * 0.1;

                o.vertex = mul(UNITY_MATRIX_VP, wPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.hash = AgentBuffer[svInstanceID].hash;

                float2 vel = AgentBuffer[svInstanceID].velocity;
                float normalizedAngle = (atan2(vel.y, vel.x) + PI) / (PI * 2.0);
                o.facingDir = floor(normalizedAngle * 4.0);

              //  o.facingDir = AgentBuffer[svInstanceID].facingDir;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float timeWithOffset = _Time.y + i.hash * 14;
                float2 animFrame = frac( float2(0.1428571 * floor(timeWithOffset * 8), (-i.facingDir + 1) * 0.25));

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv + animFrame);
                //
                clip(col.a - 0.5);

                return col;
            }
            ENDCG
        }
    }
}
