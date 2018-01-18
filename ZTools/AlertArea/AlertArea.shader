// Upgrade NOTE: upgraded instancing buffer 'ZToolsAlertArea' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ZTools/AlertArea"
{
	Properties
	{
		_MainColor("MainColor", Color) = (0,0,0,0)
		_IntersectColor("IntersectColor", Color) = (0,0,0,0)
		_Intersected("Intersected", Range( 0 , 1)) = 0
		_Texture0("Texture 0", 2D) = "white" {}
		_Glow("Glow", Float) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "DisableBatching" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Unlit alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap noforwardadd vertex:vertexDataFunc 
		struct Input
		{
			float2 texcoord_0;
			float2 texcoord_1;
		};

		uniform sampler2D _Texture0;
		uniform half _Glow;

		UNITY_INSTANCING_BUFFER_START(ZToolsAlertArea)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _MainColor)
#define _MainColor_arr ZToolsAlertArea
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _IntersectColor)
#define _IntersectColor_arr ZToolsAlertArea
			UNITY_DEFINE_INSTANCED_PROP(fixed, _Intersected)
#define _Intersected_arr ZToolsAlertArea
		UNITY_INSTANCING_BUFFER_END(ZToolsAlertArea)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.texcoord_0.xy = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
			o.texcoord_1.xy = v.texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
		}

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			fixed4 _MainColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_MainColor_arr, _MainColor);
			fixed4 _IntersectColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_IntersectColor_arr, _IntersectColor);
			fixed _Intersected_Instance = UNITY_ACCESS_INSTANCED_PROP(_Intersected_arr, _Intersected);
			float4 lerpResult62 = lerp( _MainColor_Instance , _IntersectColor_Instance , _Intersected_Instance);
			float4 temp_output_34_0 = ( tex2D( _Texture0, i.texcoord_0 ) * lerpResult62 );
			float temp_output_42_0 = ( (temp_output_34_0).a * ( 1.0 - i.texcoord_1.y ) );
			float clampResult56 = clamp( ( 1.0 - temp_output_42_0 ) , 0.5 , 1.0 );
			o.Emission = ( ( (temp_output_34_0).rgb * _Glow ) * clampResult56 );
			o.Alpha = temp_output_42_0;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13401
7;29;1906;1004;1293.975;360.875;1;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;32;-744.3746,285.1251;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;59;-752.975,-356.875;Fixed;False;InstancedProperty;_IntersectColor;IntersectColor;1;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;8;-751.5,-177.5;Fixed;False;InstancedProperty;_MainColor;MainColor;0;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.TexturePropertyNode;31;-747.3746,2.125061;Float;True;Property;_Texture0;Texture 0;3;0;None;False;white;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.RangedFloatNode;61;-488.975,-434.875;Fixed;False;InstancedProperty;_Intersected;Intersected;2;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;62;-187.975,-283.875;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;33;-384.3746,49.12506;Float;True;Property;_TextureSample0;Texture Sample 0;3;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-114.3746,-99.37494;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.TextureCoordinatesNode;50;-746.175,419.025;Float;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ComponentMaskNode;36;-67.3746,154.6251;Float;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.OneMinusNode;43;-488.375,560.6251;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-297.375,584.6251;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ComponentMaskNode;35;177.6254,-22.37494;Float;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.RangedFloatNode;27;-737.375,183.625;Half;False;Property;_Glow;Glow;4;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;58;-94.97501,730.125;Float;False;Constant;_Float1;Float 1;3;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;57;-293.975,724.125;Float;False;Constant;_Float0;Float 0;3;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.OneMinusNode;55;-229.975,464.125;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-347.375,252.625;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.ClampOpNode;56;19.02502,380.125;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;101.825,268.025;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;311,313;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;ZTools/AlertArea;False;False;False;False;True;True;True;True;True;False;False;True;False;True;True;True;True;Off;2;1;False;-1;-1;Transparent;0.5;True;False;0;False;Transparent;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;False;2;SrcAlpha;OneMinusSrcAlpha;4;One;One;Max;Max;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;14;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;62;0;8;0
WireConnection;62;1;59;0
WireConnection;62;2;61;0
WireConnection;33;0;31;0
WireConnection;33;1;32;0
WireConnection;34;0;33;0
WireConnection;34;1;62;0
WireConnection;36;0;34;0
WireConnection;43;0;50;2
WireConnection;42;0;36;0
WireConnection;42;1;43;0
WireConnection;35;0;34;0
WireConnection;55;0;42;0
WireConnection;30;0;35;0
WireConnection;30;1;27;0
WireConnection;56;0;55;0
WireConnection;56;1;57;0
WireConnection;56;2;58;0
WireConnection;51;0;30;0
WireConnection;51;1;56;0
WireConnection;0;2;51;0
WireConnection;0;9;42;0
ASEEND*/
//CHKSM=4E3DB1E455FC813311BD50949EA9B52C4F2FCF76