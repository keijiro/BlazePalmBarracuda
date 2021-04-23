#ifndef _BLAZEPALMBARRACUDA_STRUCT_H_
#define _BLAZEPALMBARRACUDA_STRUCT_H_

// Detection structure: The layout of this structure must be matched with the
// one defined in Detection.cs
struct PalmDetection
{
    float2 center;
    float2 extent;
    float2 keyPoints[6];
    float score;
    float3 pad;
};

#endif
