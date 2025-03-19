// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UseCard"
{
	Properties
	{
		_MainTex1("MainTex", 2D) = "white" {}
		_Gradient1("Gradient", 2D) = "white" {}
		_ChageAmount1("ChageAmount", Range( 0 , 1)) = 0.5976379
		_EdgeWidth1("EdgeWidth", Range( 0 , 2)) = 0.2694113
		_EdgeIntensity1("EdgeIntensity", Float) = 2
		_NoiseSpeed1("NoiseSpeed", Vector) = (0,0,0,0)
		[Toggle(_MANNULCONTROL1_ON)] _MANNULCONTROL1("MANNULCONTROL", Float) = 1
		_Spread1("Spread", Range( 0 , 1)) = 0.172512
		_Speed1("Speed", Range( 0 , 1)) = 0
		_Noise1("Noise", 2D) = "white" {}
		_RampTex1("RampTex", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
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

		uniform sampler2D _MainTex1;
		uniform float4 _MainTex1_ST;
		uniform sampler2D _RampTex1;
		uniform sampler2D _Gradient1;
		SamplerState sampler_Gradient1;
		uniform float4 _Gradient1_ST;
		uniform float _Speed1;
		uniform float _ChageAmount1;
		uniform float _Spread1;
		uniform sampler2D _Noise1;
		SamplerState sampler_Noise1;
		uniform float2 _NoiseSpeed1;
		uniform float _EdgeWidth1;
		uniform float _EdgeIntensity1;
		SamplerState sampler_MainTex1;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex1 = i.uv_texcoord * _MainTex1_ST.xy + _MainTex1_ST.zw;
			float4 tex2DNode36 = tex2D( _MainTex1, uv_MainTex1 );
			float2 uv_Gradient1 = i.uv_texcoord * _Gradient1_ST.xy + _Gradient1_ST.zw;
			float mulTime3 = _Time.y * _Speed1;
			#ifdef _MANNULCONTROL1_ON
				float staticSwitch8 = _ChageAmount1;
			#else
				float staticSwitch8 = frac( mulTime3 );
			#endif
			float Gradient20 = ( ( ( tex2D( _Gradient1, uv_Gradient1 ).r - (-_Spread1 + (staticSwitch8 - 0.0) * (1.0 - -_Spread1) / (1.0 - 0.0)) ) / _Spread1 ) * 2.0 );
			float2 panner14 = ( 1.0 * _Time.y * _NoiseSpeed1 + i.uv_texcoord);
			float Noise19 = tex2D( _Noise1, panner14 ).r;
			float temp_output_24_0 = ( Gradient20 - Noise19 );
			float clampResult30 = clamp( ( 1.0 - ( distance( temp_output_24_0 , 0.5 ) / _EdgeWidth1 ) ) , 0.0 , 1.0 );
			float2 appendResult32 = (float2(( 1.0 - clampResult30 ) , 0.5));
			float4 RampColor34 = tex2D( _RampTex1, appendResult32 );
			float4 lerpResult41 = lerp( tex2DNode36 , ( tex2DNode36 * RampColor34 * _EdgeIntensity1 ) , clampResult30);
			o.Emission = lerpResult41.rgb;
			o.Alpha = ( tex2DNode36.a * step( 0.5 , temp_output_24_0 ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows 

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
153.6;175.2;1172.8;544.6;2217.607;445.9341;2.703483;True;False
Node;AmplifyShaderEditor.CommentaryNode;1;-1959.428,-397.3717;Inherit;False;1951.373;714.2258;Grandient;14;20;18;16;15;11;10;9;8;7;6;5;4;3;2;Grandient;0.4164992,0.8943396,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-1864.848,-141.6513;Inherit;False;Property;_Speed1;Speed;8;0;Create;True;0;0;False;0;False;0;0.209;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;3;-1604.09,-135.3164;Inherit;False;1;0;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-1619.978,117.2829;Inherit;False;Property;_Spread1;Spread;7;0;Create;True;0;0;False;0;False;0.172512;0.645;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;5;-1412.851,-123.0284;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1623.369,8.223419;Inherit;False;Property;_ChageAmount1;ChageAmount;2;0;Create;True;0;0;False;0;False;0.5976379;0.431;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;7;-1245.578,34.08292;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;8;-1301.891,-89.42178;Inherit;False;Property;_MANNULCONTROL1;MANNULCONTROL;6;0;Create;True;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-1393.242,-363.6703;Inherit;True;Property;_Gradient1;Gradient;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;10;-1028.972,-102.0751;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;13;-1609.441,547.9559;Inherit;False;Property;_NoiseSpeed1;NoiseSpeed;5;0;Create;True;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;11;-789.522,-113.5635;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;12;-1665.185,381.5089;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;14;-1395.079,382.9009;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;15;-612.22,17.4429;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-684.5471,178.0859;Inherit;False;Constant;_Float1;Float 0;12;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-1046.93,349.2069;Inherit;True;Property;_Noise1;Noise;9;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-480.0861,128.9118;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-299.561,109.9706;Inherit;False;Gradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-452.0601,439.2687;Inherit;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;23;396.932,-157.3704;Inherit;False;19;Noise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;379.915,-244.0303;Inherit;False;20;Gradient;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;22;551.4001,88.62952;Inherit;False;1897.528;538.9257;EdgeColor;10;34;33;32;31;30;29;28;27;26;25;;0.2055179,0.780411,0.9471698,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;24;666.5323,-222.1704;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;637.2208,155.6631;Inherit;False;Constant;_Float2;Float 1;11;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;26;891.6491,142.5257;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;666.2,294.0063;Inherit;False;Property;_EdgeWidth1;EdgeWidth;3;0;Create;True;0;0;False;0;False;0.2694113;1.15;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;1039.113,158.9944;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;29;1251.966,184.4063;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;30;1440.347,182.2955;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;31;1628.794,237.0514;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;32;1774.826,221.4389;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;33;1958.026,203.8391;Inherit;True;Property;_RampTex1;RampTex;10;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;2234.395,248.2513;Inherit;False;RampColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;1498.394,-826.1486;Inherit;False;34;RampColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;36;1020.205,-587.4237;Inherit;True;Property;_MainTex1;MainTex;0;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;37;1484.146,-695.6599;Inherit;False;Property;_EdgeIntensity1;EdgeIntensity;4;0;Create;True;0;0;False;0;False;2;18.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;38;1520.093,-158.1353;Inherit;False;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;1868.516,-647.3896;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;41;2164.219,-437.431;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;1895.367,-319.462;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2471.45,-513.7919;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;UseCard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;2;0
WireConnection;5;0;3;0
WireConnection;7;0;4;0
WireConnection;8;1;5;0
WireConnection;8;0;6;0
WireConnection;10;0;8;0
WireConnection;10;3;7;0
WireConnection;11;0;9;1
WireConnection;11;1;10;0
WireConnection;14;0;12;0
WireConnection;14;2;13;0
WireConnection;15;0;11;0
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
WireConnection;38;1;24;0
WireConnection;39;0;36;0
WireConnection;39;1;35;0
WireConnection;39;2;37;0
WireConnection;41;0;36;0
WireConnection;41;1;39;0
WireConnection;41;2;30;0
WireConnection;40;0;36;4
WireConnection;40;1;38;0
WireConnection;0;2;41;0
WireConnection;0;9;40;0
ASEEND*/
//CHKSM=A7225A05BEF9E80EB232845A1B008704637F78A6