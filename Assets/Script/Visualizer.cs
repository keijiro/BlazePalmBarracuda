using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;
using MediaPipe.BlazePalm;

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] RawImage _previewUI = null;
    [SerializeField] Shader _shader = null;

    #endregion

    #region Private members

    PalmDetector _detector;
    Material _material;

    GraphicsBuffer _boxDrawArgs;
    GraphicsBuffer _keyDrawArgs;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _detector = new PalmDetector(_resources);
        _material = new Material(_shader);

        var target = GraphicsBuffer.Target.IndirectArguments;
        _boxDrawArgs = new GraphicsBuffer(target, 4, sizeof(uint));
        _keyDrawArgs = new GraphicsBuffer(target, 4, sizeof(uint));
        _boxDrawArgs.SetData(new [] {6, 0, 0, 0});
        _keyDrawArgs.SetData(new [] {24, 0, 0, 0});
    }

    void OnDestroy()
    {
        _detector.Dispose();
        Destroy(_material);

        _boxDrawArgs.Dispose();
        _keyDrawArgs.Dispose();
    }

    void LateUpdate()
    {
        _detector.ProcessImage(_source.Texture);
        _previewUI.texture = _source.Texture;
    }

    void OnRenderObject()
    {
        // Detection buffer
        _material.SetBuffer("_Detections", _detector.DetectionBuffer);

        // Copy the detection count into the indirect draw args.
        _detector.SetIndirectDrawCount(_boxDrawArgs);
        _detector.SetIndirectDrawCount(_keyDrawArgs);

        // Bounding box
        _material.SetPass(0);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, _boxDrawArgs, 0);

        // Key points
        _material.SetPass(1);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Lines, _keyDrawArgs, 0);
    }

    #endregion
}
