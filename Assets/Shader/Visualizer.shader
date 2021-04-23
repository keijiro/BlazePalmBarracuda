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
        x =  2 * x - 1;
        y = -2 * y + 1;

        // Aspect ratio compensation
        x = x * _ScreenParams.y / _ScreenParams.x;

        return float4(x, y, 1, 1);
    }

    float4 FragmentBox(float4 position : SV_Position) : SV_Target
    {
        return float4(1, 0, 0, 1);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex VertexBox
            #pragma fragment FragmentBox
            ENDCG
        }
    }
}
