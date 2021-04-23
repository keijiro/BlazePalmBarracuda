using UnityEngine;
using UnityEngine.UI;
using MediaPipe.BlazePalm;

namespace MediaPipe {

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [SerializeField] RawImage _previewUI = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _shader = null;

    #endregion

    #region Private members

    PalmDetector _detector;
    Material _material;
    ComputeBuffer _drawArgs;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _detector = new PalmDetector(_resources);
        _material = new Material(_shader);
        _drawArgs = new ComputeBuffer(4, sizeof(uint),
                                      ComputeBufferType.IndirectArguments);
        _drawArgs.SetData(new [] {6, 0, 0, 0});
    }

    void OnDestroy()
    {
        _detector.Dispose();
        Destroy(_material);
        _drawArgs.Dispose();
    }

    void LateUpdate()
    {
        // Palm detection
        _detector.ProcessImage(_webcam.Texture);

        // UI update
        _previewUI.texture = _webcam.Texture;
    }

    void OnRenderObject()
    {
        _detector.SetIndirectDrawCount(_drawArgs);
        _material.SetBuffer("_Detections", _detector.DetectionBuffer);

        // Bounding box
        _material.SetPass(0);
        Graphics.DrawProceduralIndirectNow
          (MeshTopology.Triangles, _drawArgs, 0);
    }

    #endregion
}

} // namespace MediaPipe
