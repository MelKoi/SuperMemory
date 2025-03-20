// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UseCard"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Gradient("Gradient", 2D) = "white" {}
		_ChangeAmount("ChangeAmount", Range( 0 , 1)) = 0.5976379
		_EdgeWidth("EdgeWidth", Range( 0 , 2)) = 0.2694113
		_EdgeIntensity("EdgeIntensity", Float) = 2
		_NoiseSpeed("NoiseSpeed", Vector) = (0,0,0,0)
		[Toggle(_MANNULCONTROL1_ON)] _MANNULCONTROL1("MANNULCONTROL", Float) = 1
		_Spread("Spread", Range( 0 , 1)) = 0.172512
		_Speed("Speed", Range( 0 , 1)) = 0
		_Noise("Noise", 2D) = "white" {}
		_RampTex("RampTex", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Overlay"  "Queue" = "Overlay+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _MANNULCONTROL1_ON
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _RampTex;
		uniform sampler2D _Gradient;
		SamplerState sampler_Gradient;
		uniform float4 _Gradient_ST;
		uniform float _Speed;
		uniform float _ChangeAmount;
		uniform float _Spread;
		uniform sampler2D _Noise;
		SamplerState sampler_Noise;
		uniform float2 _NoiseSpeed;
		uniform float _EdgeWidth;
		uniform float _EdgeIntensity;
		SamplerState sampler_MainTex;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode36 = tex2D( _MainTex, uv_MainTex );
			float2 uv_Gradient = i.uv_texcoord * _Gradient_ST.xy + _Gradient_ST.zw;
			float mulTime3 = _Time.y * _Speed;
			#ifdef _MANNULCONTROL1_ON
				float staticSwitch8 = _ChangeAmount;
			#else
				float staticSwitch8 = frac( mulTime3 );
			#endif
			float Gradient20 = ( ( ( tex2D( _Gradient, uv_Gradient ).r - (-_Spread + (staticSwitch8 - 0.0) * (1.0 - -_Spread) / (1.0 - 0.0)) ) / _Spread ) * 2.0 );
			float2 panner14 = ( 1.0 * _Time.y * _NoiseSpeed + i.uv_texcoord);
			float Noise19 = tex2D( _Noise, panner14 ).r;
			float temp_output_24_0 = ( Gradient20 - Noise19 );
			float clampResult30 = clamp( ( 1.0 - ( distance( temp_output_24_0 , 0.5 ) / _EdgeWidth ) ) , 0.0 , 1.0 );
			float2 appendResult32 = (float2(( 1.0 - clampResult30 ) , 0.5));
			float4 RampColor34 = tex2D( _RampTex, appendResult32 );
			float4 lerpResult41 = lerp( tex2DNode36 , ( tex2DNode36 * RampColor34 * _EdgeIntensity ) , clampResult30);
			o.Emission = lerpResult41.rgb;
			o.Alpha = ( tex2DNode36.a * step( 0.5 , temp_output_24_0 ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
6.4;222.4;1172.8;539;3009.553;128.7519;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;1;-2736.228,-364.9717;Inherit;False;1951.373;714.2258;Grandient;14;20;18;16;15;13;10;9;8;7;6;5;4;3;2;Grandient;0.4164992,0.8943396,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-2641.648,-109.2513;Inherit;False;Property;_Speed;Speed;9;0;Create;True;0;0;False;0;False;0;0.209;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;3;-2380.89,-102.9164;Inherit;False;1;0;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-2396.778,149.6829;Inherit;False;Property;_Spread;Spread;8;0;Create;True;0;0;False;0;False;0.172512;0.645;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-2400.169,39.82341;Inherit;False;Property;_ChangeAmount;ChangeAmount;3;0;Create;False;0;0;False;0;False;0.5976379;0.431;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;5;-2189.651,-90.62839;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;7;-2022.378,66.48291;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;8;-2078.691,-57.02179;Inherit;False;Property;_MANNULCONTROL1;MANNULCONTROL;7;0;Create;True;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;10;-1805.772,-69.67511;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-2170.042,-331.2703;Inherit;True;Property;_Gradient;Gradient;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;13;-1566.322,-81.16351;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-2441.985,413.9089;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;12;-2386.241,580.356;Inherit;False;Property;_NoiseSpeed;NoiseSpeed;6;0;Create;True;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;14;-2171.879,415.3009;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;15;-1389.02,49.8429;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1461.347,210.4859;Inherit;False;Constant;_Float1;Float 0;12;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-1823.73,381.6069;Inherit;True;Property;_Noise;Noise;10;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1256.886,161.3118;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-1076.361,142.3706;Inherit;False;Gradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-1228.86,471.6687;Inherit;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;22;-225.3998,121.0295;Inherit;False;1897.528;538.9257;EdgeColor;10;34;33;32;31;30;29;28;27;26;25;;0.2055179,0.780411,0.9471698,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;-396.8849,-211.6303;Inherit;False;20;Gradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;23;-379.8679,-124.9704;Inherit;False;19;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;24;-110.2676,-189.7704;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-139.5792,188.0631;Inherit;False;Constant;_Float2;Float 1;11;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;26;114.8492,174.9257;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-110.5999,326.4063;Inherit;False;Property;_EdgeWidth;EdgeWidth;4;0;Create;True;0;0;False;0;False;0.2694113;1.15;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;262.3127,191.3944;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;29;475.1659,216.8063;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;30;663.5466,214.6955;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;31;851.9944,269.4514;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;32;998.0258,253.8389;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;33;1181.226,236.2391;Inherit;True;Property;_RampTex;RampTex;11;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;1457.594,280.6513;Inherit;False;RampColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;36;241.8848,-555.0237;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;37;707.3464,-663.2599;Inherit;False;Property;_EdgeIntensity;EdgeIntensity;5;0;Create;True;0;0;False;0;False;2;18.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;721.5945,-793.7486;Inherit;False;34;RampColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;1091.716,-614.9896;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;38;743.2933,-125.7353;Inherit;False;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;41;1424.437,-435.8805;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;1118.567,-287.062;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;47;1697.367,-479.1758;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;UseCard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;2;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Overlay;;Overlay;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;1;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;2;0
WireConnection;5;0;3;0
WireConnection;7;0;4;0
WireConnection;8;1;5;0
WireConnection;8;0;6;0
WireConnection;10;0;8;0
WireConnection;10;3;7;0
WireConnection;13;0;9;1
WireConnection;13;1;10;0
WireConnection;14;0;11;0
WireConnection;14;2;12;0
WireConnection;15;0;13;0
WireConnection;15;1;4;0
WireConnection;17;1;14;0
WireConnection;18;0;15;0
WireConnection;18;1;16;0
WireConnection;20;0;18;0
WireConnection;19;0;17;1
WireConnection;24;0;21;0
WireConnection;24;1;23;0
WireConnection;26;0;24;0
WireConnection;26;1;25;0
WireConnection;28;0;26;0
WireConnection;28;1;27;0
WireConnection;29;0;28;0
WireConnection;30;0;29;0
WireConnection;31;0;30;0
WireConnection;32;0;31;0
WireConnection;33;1;32;0
WireConnection;34;0;33;0
WireConnection;39;0;36;0
WireConnection;39;1;35;0
WireConnection;39;2;37;0
WireConnection;38;1;24;0
WireConnection;41;0;36;0
WireConnection;41;1;39;0
WireConnection;41;2;30;0
WireConnection;40;0;36;4
WireConnection;40;1;38;0
WireConnection;47;2;41;0
WireConnection;47;9;40;0
ASEEND*/
//CHKSM=EC868F9D4AD0F2BD7FA5A9B83FC28BCFA6CE4D56