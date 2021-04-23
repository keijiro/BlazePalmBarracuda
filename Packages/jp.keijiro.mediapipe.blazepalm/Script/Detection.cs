using System.Runtime.InteropServices;
using UnityEngine;

namespace MediaPipe.BlazePalm {

partial class PalmDetector
{
    //
    // Detection structure. The layout of this structure must be matched with
    // the one defined in Common.hlsl.
    //
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Detection
    {
        // Bounding box
        public readonly Vector2 center;
        public readonly Vector2 extent;

        // Key points
        public readonly Vector2 wrist;
        public readonly Vector2 point;
        public readonly Vector2 middle;
        public readonly Vector2 ring;
        public readonly Vector2 pinky;
        public readonly Vector2 thumb;

        // Confidence score [0, 1]
        public readonly float score;

        // Padding
        public readonly float pad1, pad2, pad3;

        // sizeof(Detection)
        public const int Size = 20 * sizeof(float);
    };
}

} // namespace MediaPipe.BlazePalm
