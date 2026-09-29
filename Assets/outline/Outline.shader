Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 0.8, 0, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.015
        // 0 = extrude along vertex normal (normal 3D meshes)
        // 1 = extrude radially outward across the surface (flat planes/quads,
        //     where every normal points the same way so normal-extrusion does nothing)
        _FlatMode ("Flat Mode", Float) = 0
        // Object-space offset of the mesh's visual center, only used in Flat Mode.
        // Leave at (0,0,0) if the object's pivot is already at its center.
        _PivotOffset ("Pivot Offset (Flat Mode)", Vector) = (0, 0, 0, 0)
        // Pushes the outline mesh backward along the normal so it doesn't
        // z-fight with the original surface. Needed especially in Flat Mode,
        // where the outline is otherwise perfectly coplanar with the object.
        _DepthBias ("Depth Bias", Range(0.0, 0.05)) = 0.002
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+1" }

        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite On
            ZTest LEqual
            Offset 1, 1

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _FlatMode;
                float4 _PivotOffset;
                float _DepthBias;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 posOS = IN.positionOS.xyz;
                float3 normalOS = normalize(IN.normalOS);
                float3 extrudeDir;

                if (_FlatMode > 0.5)
                {
                    // Direction from the surface's center to this vertex, flattened
                    // onto the plane (i.e. with any component along the normal removed).
                    // This pushes edges outward across the flat surface instead of
                    // straight off its face, which is what a flat plane needs.
                    float3 radial = posOS - _PivotOffset.xyz;
                    radial -= dot(radial, normalOS) * normalOS;
                    float radialLen = length(radial);
                    extrudeDir = (radialLen > 0.0001) ? (radial / radialLen) : normalOS;
                }
                else
                {
                    // Standard inverted-hull extrusion for regular 3D meshes.
                    extrudeDir = normalOS;
                }

                float3 expandedPosOS = posOS + extrudeDir * _OutlineWidth;
                // Nudge backward along the true surface normal to avoid z-fighting
                // with the original mesh, which the radial (Flat Mode) expansion
                // otherwise leaves perfectly coplanar with.
                expandedPosOS -= normalOS * _DepthBias;
                OUT.positionHCS = TransformObjectToHClip(expandedPosOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
