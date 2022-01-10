// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AlanZucconi/Voronoi Diagram"
{
	Properties
	{
		_HeatTex("Texture", 2D) = "white" {}
		_Radius("Radius", Range(0,0.25)) = 0.03
		_P("P", Range(1,2)) = 2
		[Toggle] _D("Distance", Float) = 0
	}

	SubShader
	{
		Tags {"Queue" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert             
			#pragma fragment frag
			uniform float _D;
			uniform half _P;
			uniform half _Radius;
			uniform int _Length = 0;
			uniform half2 _Points[100];
			uniform fixed3 _Colors[100];

			sampler2D _HeatTex;

			struct vertInput {
				float4 pos : POSITION;
			};

			struct vertOutput {
				float4 pos : POSITION;
				fixed3 worldPos : TEXCOORD1;
			};

			vertOutput vert(vertInput input) {
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.worldPos = mul(unity_ObjectToWorld, input.pos).xyz;
				return o;
			}

			half distance_manhattan(float2 a, float2 b)
			{
				return abs(a.x - b.x) + abs(a.y - b.y);
			}
			half distance_chebyshev(float2 a, float2 b)
			{
				return max(abs(a.x - b.x) , abs(a.y - b.y)	);
			}
			half distance_minkowski(float2 a, float2 b)
			{
				return pow(pow(abs(a.x - b.x),_P) + pow(abs(a.y - b.y),_P),1/_P);
			}

			fixed4 frag(vertOutput output) : COLOR {
				half minDist = 10000;
				int minI = 0;	// Index of min
				for (int i = 0; i < _Length; i++)
				{
					half dist = distance_minkowski(output.worldPos.xy, _Points[i].xy);
					if (dist < _Radius)
						return fixed4(0, 0, 0, 1);

					if (dist < minDist)
					{
						minDist = dist;
						minI = i;
					}
				}

				if (_D == 0)
				{
					return fixed4(_Colors[minI], 1);
				}
				else
				{
					half c = minDist;
					half4 color = tex2D(_HeatTex, fixed2(c, 0.5));
					color.a = 1;
					return color;
				}
			}
		  ENDCG
		  }
	}
	Fallback "Diffuse"
}