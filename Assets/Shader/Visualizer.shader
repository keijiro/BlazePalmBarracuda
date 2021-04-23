Shader "Hidden/MediaPipe/BlazePalm/Visualizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Packages/jp.keijiro.mediapipe.blazepalm/Shader/Struct.hlsl"

    StructuredBuffer<PalmDetection> _Detections;

    float4 VertexBox(uint vid : SV_VertexID,
                     uint iid : SV_InstanceID) : SV_Position
    {
        PalmDetection d = _Detections[iid];

        // Bounding box
        float x = d.center.x + d.extent.x * lerp(-0.5, 0.5, vid & 1);
        float y = d.center.y + d.extent.y * lerp(-0.5, 0.5, vid < 2 || vid == 5);

        // Clip space to screen space
        x = (2 * x - 1) * _ScreenParams.y / _ScreenParams.x;
        y =  2 * y - 1;

        return float4(x, y, 1, 1);
    }

    float4 FragmentBox(float4 position : SV_Position) : SV_Target
    {
        return float4(1, 0, 0, 0.5);
    }

    float4 VertexKey(uint vid : SV_VertexID,
                     uint iid : SV_InstanceID) : SV_Position
    {
        PalmDetection d = _Detections[iid];

        // Key point
        float2 p = d.keyPoints[vid / 4];

        // Marker shape (+)
        const float size = 0.01;
        uint vtid = vid & 3;
        p.x += size * lerp(-1, 1, vtid > 0) * (vtid < 2);
        p.y += size * lerp(-1, 1, vtid > 2) * (vtid > 1);

        // Clip space to screen space
        p = p * 2 - 1;
        p.x *= _ScreenParams.y / _ScreenParams.x;

        return float4(p, 1, 1);
    }

    float4 FragmentKey(float4 position : SV_Position) : SV_Target
    {
        return float4(0, 0, 1, 0.9);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex VertexBox
            #pragma fragment FragmentBox
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex VertexKey
            #pragma fragment FragmentKey
            ENDCG
        }
    }
}
