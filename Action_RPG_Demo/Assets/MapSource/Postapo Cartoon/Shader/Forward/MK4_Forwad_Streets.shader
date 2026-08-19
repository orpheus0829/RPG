// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:2865,x:33224,y:32700,varname:node_2865,prsc:2|diff-395-OUT,spec-2872-R,gloss-5477-OUT,normal-5964-RGB;n:type:ShaderForge.SFN_Multiply,id:6343,x:31955,y:32728,varname:node_6343,prsc:2|A-7736-RGB,B-6665-RGB;n:type:ShaderForge.SFN_Color,id:6665,x:31620,y:32883,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5019608,c2:0.5019608,c3:0.5019608,c4:1;n:type:ShaderForge.SFN_Tex2d,id:7736,x:31620,y:32714,ptovrint:True,ptlb:Traffic,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:aeb745ed7c49d5c4c8d3139c39632ad8,ntxv:0,isnm:False|UVIN-6816-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:5964,x:32003,y:32935,ptovrint:True,ptlb:Normal Map,ptin:_BumpMap,varname:_BumpMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c16101dc3f8e3e0458ba52dc7bfe46a6,ntxv:3,isnm:True|UVIN-1888-OUT;n:type:ShaderForge.SFN_Slider,id:5780,x:30674,y:32618,ptovrint:False,ptlb:Asphalt scale,ptin:_Asphaltscale,varname:node_1555,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.2,max:1;n:type:ShaderForge.SFN_RemapRange,id:6363,x:31055,y:32581,varname:node_6363,prsc:2,frmn:0,frmx:1,tomn:0.05,tomx:0.5|IN-5780-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:1857,x:30925,y:32303,varname:node_1857,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7452,x:31261,y:32448,varname:node_7452,prsc:2|A-1857-XYZ,B-6363-OUT;n:type:ShaderForge.SFN_Append,id:1888,x:31629,y:32470,varname:node_1888,prsc:2|A-6577-B,B-6577-R;n:type:ShaderForge.SFN_Tex2d,id:1524,x:31936,y:32453,ptovrint:False,ptlb:Aldebo,ptin:_Aldebo,varname:node_1524,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:cad4fcf2ba398dd4c98c65186a533c03,ntxv:0,isnm:False|UVIN-1888-OUT;n:type:ShaderForge.SFN_Add,id:7787,x:32417,y:32637,varname:node_7787,prsc:2|A-1970-OUT,B-1524-RGB;n:type:ShaderForge.SFN_TexCoord,id:6816,x:31371,y:32728,varname:node_6816,prsc:2,uv:0;n:type:ShaderForge.SFN_Tex2d,id:2872,x:32524,y:32839,ptovrint:False,ptlb:Metallic Gloss,ptin:_MetallicGloss,varname:node_2872,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:564aa1ae0b65dc941a3ce1a787f5c093,ntxv:0,isnm:False|UVIN-1888-OUT;n:type:ShaderForge.SFN_Tex2d,id:3847,x:31941,y:32195,varname:node_3847,prsc:2,tex:d4675ebdff8f5e440a56a32968f261ea,ntxv:0,isnm:False|UVIN-1888-OUT,TEX-2298-TEX;n:type:ShaderForge.SFN_Multiply,id:1970,x:32177,y:32592,varname:node_1970,prsc:2|A-3847-RGB,B-6343-OUT;n:type:ShaderForge.SFN_ComponentMask,id:6577,x:31428,y:32448,varname:node_6577,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-7452-OUT;n:type:ShaderForge.SFN_Tex2d,id:631,x:32315,y:32219,ptovrint:False,ptlb:Alpha Details,ptin:_AlphaDetails,varname:node_631,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e5f4d1e5f11059b47b4daca2dd100891,ntxv:0,isnm:False|UVIN-1888-OUT;n:type:ShaderForge.SFN_Lerp,id:758,x:32682,y:32417,varname:node_758,prsc:2|A-7787-OUT,B-631-RGB,T-9731-OUT;n:type:ShaderForge.SFN_Multiply,id:395,x:32789,y:32520,varname:node_395,prsc:2|A-3847-RGB,B-758-OUT;n:type:ShaderForge.SFN_Multiply,id:9731,x:32401,y:32409,varname:node_9731,prsc:2|A-631-A,B-3847-A;n:type:ShaderForge.SFN_OneMinus,id:8311,x:32606,y:33055,varname:node_8311,prsc:2|IN-2807-OUT;n:type:ShaderForge.SFN_Add,id:5477,x:32863,y:32869,varname:node_5477,prsc:2|A-8311-OUT,B-2872-A;n:type:ShaderForge.SFN_Tex2d,id:2990,x:32785,y:32211,ptovrint:False,ptlb:node_2990,ptin:_node_2990,varname:node_2990,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2dAsset,id:2298,x:31771,y:32136,ptovrint:False,ptlb:Cracks,ptin:_Cracks,varname:node_2298,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:d4675ebdff8f5e440a56a32968f261ea,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:7374,x:32361,y:33068,varname:node_7374,prsc:2,tex:d4675ebdff8f5e440a56a32968f261ea,ntxv:0,isnm:False|UVIN-5403-OUT,TEX-2298-TEX;n:type:ShaderForge.SFN_Clamp01,id:7637,x:32807,y:33055,varname:node_7637,prsc:2|IN-8311-OUT;n:type:ShaderForge.SFN_Power,id:2807,x:32547,y:33242,varname:node_2807,prsc:2|VAL-7374-R,EXP-984-OUT;n:type:ShaderForge.SFN_Multiply,id:5403,x:32144,y:33246,varname:node_5403,prsc:2|A-1888-OUT,B-8641-OUT;n:type:ShaderForge.SFN_Vector1,id:984,x:32253,y:33429,varname:node_984,prsc:2,v1:2;n:type:ShaderForge.SFN_Vector1,id:8641,x:31867,y:33443,varname:node_8641,prsc:2,v1:0.5;proporder:1524-5964-6665-7736-2872-5780-631-2298;pass:END;sub:END;*/

Shader "MK4/Forward/MK4_Streets" {
    Properties {
        _Aldebo ("Aldebo", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Color ("Color", Color) = (0.5019608,0.5019608,0.5019608,1)
        _MainTex ("Traffic", 2D) = "white" {}
        _MetallicGloss ("Metallic Gloss", 2D) = "white" {}
        _Asphaltscale ("Asphalt scale", Range(0, 1)) = 0.2
        _AlphaDetails ("Alpha Details", 2D) = "white" {}
        _Cracks ("Cracks", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _Asphaltscale;
            uniform sampler2D _Aldebo; uniform float4 _Aldebo_ST;
            uniform sampler2D _MetallicGloss; uniform float4 _MetallicGloss_ST;
            uniform sampler2D _AlphaDetails; uniform float4 _AlphaDetails_ST;
            uniform sampler2D _Cracks; uniform float4 _Cracks_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 node_6577 = (i.posWorld.rgb*(_Asphaltscale*0.45+0.05)).rgb;
                float2 node_1888 = float2(node_6577.b,node_6577.r);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(node_1888, _BumpMap)));
                float3 normalLocal = _BumpMap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float2 node_5403 = (node_1888*0.5);
                float4 node_7374 = tex2D(_Cracks,TRANSFORM_TEX(node_5403, _Cracks));
                float node_8311 = (1.0 - pow(node_7374.r,2.0));
                float4 _MetallicGloss_var = tex2D(_MetallicGloss,TRANSFORM_TEX(node_1888, _MetallicGloss));
                float gloss = (node_8311+_MetallicGloss_var.a);
                float specPow = exp2( gloss * 10.0+1.0);
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                d.boxMax[0] = unity_SpecCube0_BoxMax;
                d.boxMin[0] = unity_SpecCube0_BoxMin;
                d.probePosition[0] = unity_SpecCube0_ProbePosition;
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.boxMax[1] = unity_SpecCube1_BoxMax;
                d.boxMin[1] = unity_SpecCube1_BoxMin;
                d.probePosition[1] = unity_SpecCube1_ProbePosition;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float4 node_3847 = tex2D(_Cracks,TRANSFORM_TEX(node_1888, _Cracks));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _Aldebo_var = tex2D(_Aldebo,TRANSFORM_TEX(node_1888, _Aldebo));
                float4 _AlphaDetails_var = tex2D(_AlphaDetails,TRANSFORM_TEX(node_1888, _AlphaDetails));
                float3 diffuseColor = (node_3847.rgb*lerp(((node_3847.rgb*(_MainTex_var.rgb*_Color.rgb))+_Aldebo_var.rgb),_AlphaDetails_var.rgb,(_AlphaDetails_var.a*node_3847.a))); // Need this for specular when using metallic
                float specularMonochrome;
                float3 specularColor;
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, _MetallicGloss_var.r, specularColor, specularMonochrome );
                specularMonochrome = 1-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
                float specularPBL = max(0, (NdotL*visTerm*normTerm) * (UNITY_PI / 4) );
                float3 directSpecular = 1 * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _Asphaltscale;
            uniform sampler2D _Aldebo; uniform float4 _Aldebo_ST;
            uniform sampler2D _MetallicGloss; uniform float4 _MetallicGloss_ST;
            uniform sampler2D _AlphaDetails; uniform float4 _AlphaDetails_ST;
            uniform sampler2D _Cracks; uniform float4 _Cracks_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 node_6577 = (i.posWorld.rgb*(_Asphaltscale*0.45+0.05)).rgb;
                float2 node_1888 = float2(node_6577.b,node_6577.r);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(node_1888, _BumpMap)));
                float3 normalLocal = _BumpMap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float2 node_5403 = (node_1888*0.5);
                float4 node_7374 = tex2D(_Cracks,TRANSFORM_TEX(node_5403, _Cracks));
                float node_8311 = (1.0 - pow(node_7374.r,2.0));
                float4 _MetallicGloss_var = tex2D(_MetallicGloss,TRANSFORM_TEX(node_1888, _MetallicGloss));
                float gloss = (node_8311+_MetallicGloss_var.a);
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float4 node_3847 = tex2D(_Cracks,TRANSFORM_TEX(node_1888, _Cracks));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _Aldebo_var = tex2D(_Aldebo,TRANSFORM_TEX(node_1888, _Aldebo));
                float4 _AlphaDetails_var = tex2D(_AlphaDetails,TRANSFORM_TEX(node_1888, _AlphaDetails));
                float3 diffuseColor = (node_3847.rgb*lerp(((node_3847.rgb*(_MainTex_var.rgb*_Color.rgb))+_Aldebo_var.rgb),_AlphaDetails_var.rgb,(_AlphaDetails_var.a*node_3847.a))); // Need this for specular when using metallic
                float specularMonochrome;
                float3 specularColor;
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, _MetallicGloss_var.r, specularColor, specularMonochrome );
                specularMonochrome = 1-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithBeckmannVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, NDFBlinnPhongNormalizedTerm(NdotH, RoughnessToSpecPower(1.0-gloss)));
                float specularPBL = max(0, (NdotL*visTerm*normTerm) * (UNITY_PI / 4) );
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float3 directDiffuse = ((1 +(fd90 - 1)*pow((1.00001-NdotL), 5)) * (1 + (fd90 - 1)*pow((1.00001-NdotV), 5)) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Asphaltscale;
            uniform sampler2D _Aldebo; uniform float4 _Aldebo_ST;
            uniform sampler2D _MetallicGloss; uniform float4 _MetallicGloss_ST;
            uniform sampler2D _AlphaDetails; uniform float4 _AlphaDetails_ST;
            uniform sampler2D _Cracks; uniform float4 _Cracks_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
                float3 node_6577 = (i.posWorld.rgb*(_Asphaltscale*0.45+0.05)).rgb;
                float2 node_1888 = float2(node_6577.b,node_6577.r);
                float4 node_3847 = tex2D(_Cracks,TRANSFORM_TEX(node_1888, _Cracks));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 _Aldebo_var = tex2D(_Aldebo,TRANSFORM_TEX(node_1888, _Aldebo));
                float4 _AlphaDetails_var = tex2D(_AlphaDetails,TRANSFORM_TEX(node_1888, _AlphaDetails));
                float3 diffColor = (node_3847.rgb*lerp(((node_3847.rgb*(_MainTex_var.rgb*_Color.rgb))+_Aldebo_var.rgb),_AlphaDetails_var.rgb,(_AlphaDetails_var.a*node_3847.a)));
                float specularMonochrome;
                float3 specColor;
                float4 _MetallicGloss_var = tex2D(_MetallicGloss,TRANSFORM_TEX(node_1888, _MetallicGloss));
                diffColor = DiffuseAndSpecularFromMetallic( diffColor, _MetallicGloss_var.r, specColor, specularMonochrome );
                float2 node_5403 = (node_1888*0.5);
                float4 node_7374 = tex2D(_Cracks,TRANSFORM_TEX(node_5403, _Cracks));
                float node_8311 = (1.0 - pow(node_7374.r,2.0));
                float roughness = 1.0 - (node_8311+_MetallicGloss_var.a);
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
