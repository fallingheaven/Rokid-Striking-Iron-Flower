Shader "Unlit/Fireworks"
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


            #define PI 3.141592653589793238
            #define TWOPI 6.283185307179586
            #define S(x,y,z) smoothstep(x,y,z)
            #define B(x,y,z,w) S(x-z, x+z, w)*S(y+z, y-z, w)
            #define saturate(x) clamp(x,0.,1.)

            #define NUM_EXPLOSIONS 8.
            #define NUM_PARTICLES 70.


            // Noise functions by Dave Hoskins 
            #define MOD3 float3(.1031,.11369,.13787)
            float3 hash31(float p) {
               float3 p3 = frac(p * MOD3);
               p3 += dot(p3, p3.yzx + 19.19);
               return frac(float3((p3.x + p3.y)*p3.z, (p3.x+p3.z)*p3.y, (p3.y+p3.z)*p3.x));
            }
            float hash12(float2 p){
	            float3 p3  = frac(float3(p.xyx) * MOD3);
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.x + p3.y) * p3.z);
            }

            float circ(float2 uv, float2 pos, float size) {
	            uv -= pos;
    
                size *= size;
                return S(size*1.1, size, dot(uv, uv));
            }

            float light(float2 uv, float2 pos, float size) {
	            uv -= pos;
    
                size *= size;
                return size/dot(uv, uv);
            }

            float3 explosion(float2 uv, float2 p, float seed, float t) {
	
                float3 col = 0.0;
    
                float3 en = hash31(seed);
                float3 baseCol = en;
                for(float i=0.; i<NUM_PARTICLES; i++) {
    	            float3 n = hash31(i)-.5;
       
		            float2 startP = p-float2(0., t*t*.1);        
                    float2 endP = startP+normalize(n.xy)*n.z;
        
        
                    float pt = 1.-pow(t-1., 2.);
                    float2 pos = lerp(p, endP, pt);    
                    float size = lerp(.01, .005, S(0., .1, pt));
                    size *= S(1., .1, pt);
        
                    float sparkle = (sin((pt+n.z)*100.)*.5+.5);
                    sparkle = pow(sparkle, pow(en.x, 3.)*50.)*lerp(0.01, .01, en.y*n.y);
      
    	            //size += sparkle*B(.6, 1., .1, t);
                    size += sparkle*B(en.x, en.y, en.z, t);
        
                    col += baseCol*light(uv, pos, size);
                }
    
                return col;
            }

            float3 Rainbow(float3 c) {
	
                float t=_Time.y;
    
                float avg = (c.r+c.g+c.b)/3.;
                c = avg + (c-avg)*sin(float3(0., .333, .666)+t);
    
                c += sin(float3(.4, .3, .3)*t + float3(1.1244,3.43215,6.435))*float3(.4, .1, .5);
    
                return c;
            }

            fixed4 frag (v2f i) : SV_Target
            {
	            float2 uv = i.uv;
	            uv.x -= .5;
                //uv.x *= iResolution.x/iResolution.y;
    
                float n = hash12(uv+10.);
                float t = _Time.y*.5;
    
                float3 c = 0.0;
    
                for(float i=0.; i<NUM_EXPLOSIONS; i++) {
    	            float et = t+i*1234.45235;
                    float id = floor(et);
                    et -= id;
        
                    float2 p = hash31(id).xy;
                    p.x -= .5;
                    p.x *= 1.6;
                    c += explosion(uv, p, id, et);
                }
                c = Rainbow(c);
    
                return float4(c, 1.);
            }


            ENDCG
        }
    }
}
