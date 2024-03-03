Shader "Unlit/InstancedEntityShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            
            
            struct Agent{
                float3 Position;
                float Hash;
                float3 Velocity;
                int Track;
            }; 

            StructuredBuffer<Agent> AgentBuffer;


            v2f vert (appdata v, uint svInstanceID : SV_InstanceID)
            {
                v2f o;

                float4 vertex = v.vertex;
                v.vertex.xyz *= 0.1;
                float4 wPos = mul(unity_ObjectToWorld, v.vertex);
                wPos.xyz += AgentBuffer[svInstanceID].Position;// + float3(0,svInstanceID,0);

                o.vertex = mul(UNITY_MATRIX_VP, wPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
