using UnityEngine;
using Unity.Barracuda;

namespace MediaPipe.BlazePalm {

//
// ScriptableObject class used to hold references to internal assets
//
[CreateAssetMenu(fileName = "BlazePalm",
                 menuName = "ScriptableObjects/MediaPipe/BlazePalm Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public NNModel model;
    public ComputeShader preprocess;
    public ComputeShader postprocess1;
    public ComputeShader postprocess2;
}

} // namespace MediaPipe.BlazePalm
