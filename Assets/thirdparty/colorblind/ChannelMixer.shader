// Alan Zucconi
// www.alanzucconi.com
Shader "Hidden/ChannelMixer"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_R("Red Mixing", Color) = (1,0,0,1)
		_G("Green Mixing", Color) = (0,1,0,1)
		_B("Blue Mixing", Color) = (0,0,1,1)
	}

	SubShader
	{
		Pass // 0
		{
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform fixed3 _R;
			uniform fixed3 _G;
			uniform fixed3 _B;

			fixed4 frag(v2f_img i) : COLOR
			{
				fixed4 c = tex2D(_MainTex, i.uv);

				return fixed4
				(
					c.r * _R[0] + c.g * _R[1] + c.b * _R[2],
					c.r * _G[0] + c.g * _G[1] + c.b * _G[2],
					c.r * _B[0] + c.g * _B[1] + c.b * _B[2],
					c.a
				);
			}
			ENDCG
		}

		Pass // 1
		{
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform fixed3 _R;
			uniform fixed3 _G;
			uniform fixed3 _B;

			fixed4 frag(v2f_img i) : COLOR
			{
				fixed4 c = tex2D(_MainTex, i.uv);

				// Color blind
				fixed3 cb = fixed3
				(
					c.r * _R[0] + c.g * _R[1] + c.b * _R[2],
					c.r * _G[0] + c.g * _G[1] + c.b * _G[2],
					c.r * _B[0] + c.g * _B[1] + c.b * _B[2]
				);

				// Difference
				fixed3 diff = abs(c.rgb - cb);

				// Bw
				fixed lum = c.r*.3 + c.g*.59 + c.b*.11;
				fixed3 bw = fixed3(lum, lum, lum);

				//return fixed4(lerp(bw, fixed3(1, 0, 0), diff), c.a);
				//return fixed4(lerp(bw, fixed3(1, 0, 0), (diff.r+diff.g+diff.b)/3), c.a);
				//return fixed4(lerp(c, bw, saturate(	(diff.r + diff.g + diff.b)/1	)), c.a);
				return fixed4(lerp(bw, fixed3(1, 0, 0), saturate((diff.r + diff.g + diff.b) / 3)), c.a);
			}
			ENDCG
		}
	}
}