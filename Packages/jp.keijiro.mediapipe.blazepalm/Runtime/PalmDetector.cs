using Unity.Barracuda;
using UnityEngine;
using Klak.NNUtils;
using Klak.NNUtils.Extensions;

namespace MediaPipe.BlazePalm {

//
// Palm detector class
//
public sealed partial class PalmDetector : System.IDisposable
{
    #region Public methods/properties

    public PalmDetector(ResourceSet resources)
      => AllocateObjects(resources);

    public void Dispose()
      => DeallocateObjects();

    public void ProcessInput(float threshold = 0.75f)
      => RunModel(threshold);

    public void ProcessImage(Texture image, float threshold = 0.75f)
      => RunModel(image, threshold);

    public int ImageSize
      => _size;

    public bool InputIsNCHW
      => _preprocess.IsNCHW;

    public System.ReadOnlySpan<Detection> Detections
      => _readCache.Cached;

    public ComputeBuffer InputBuffer
      => _preprocess.Buffer;

    public GraphicsBuffer DetectionBuffer
      => _output.post2;

    public GraphicsBuffer CountBuffer
      => _output.count;

    public void SetIndirectDrawCount(GraphicsBuffer drawArgs)
      => GraphicsBuffer.CopyCount(_output.post2, drawArgs, sizeof(uint));

    #endregion

    #region Private objects

    ResourceSet _resources;
    int _size;
    IWorker _worker;
    ImagePreprocess _preprocess;
    (GraphicsBuffer post1, GraphicsBuffer post2, GraphicsBuffer count) _output;
    CountedBufferReader<Detection> _readCache;

    void AllocateObjects(ResourceSet resources)
    {
        _resources = resources;

        // NN model
        var model = ModelLoader.Load(_resources.model);
        _size = model.inputs[0].GetTensorShape().GetWidth();

        // GPU worker
        _worker = model.CreateWorker(WorkerFactory.Device.GPU);

        // Preprocess
        _preprocess = new ImagePreprocess(_size, _size, nchwFix: true);

        // Output buffers
        _output.post1 = BufferUtil.NewAppend<Detection>(Detection.Max);
        _output.post2 = BufferUtil.NewAppend<Detection>(Detection.Max);
        _output.count = BufferUtil.NewRaw(1);

        // Detection data read cache
        _readCache = new CountedBufferReader<Detection>(_output.post2, _output.count, Detection.Max);
    }

    void DeallocateObjects()
    {
        _worker?.Dispose();
        _worker = null;

        _preprocess?.Dispose();
        _preprocess = null;

        _output.post1?.Dispose();
        _output.post2?.Dispose();
        _output.count?.Dispose();
        _output = (null, null, null);
    }

    #endregion

    #region Neural network inference function

    void RunModel(Texture source, float threshold)
    {
        _preprocess.Dispatch(source, _resources.preprocess);
        RunModel(threshold);
    }

    void RunModel(float threshold)
    {
        // Reset the compute buffer counters.
        _output.post1.SetCounterValue(0);
        _output.post2.SetCounterValue(0);

        // Run the BlazePalm model.
        _worker.Execute(_preprocess.Tensor);

        // 1st postprocess (bounding box aggregation)
        var post1 = _resources.postprocess1;
        post1.SetFloat("_ImageSize", _size);
        post1.SetFloat("_Threshold", threshold);

        post1.SetBuffer(0, "_Scores", _worker.PeekOutputBuffer("classificators"));
        post1.SetBuffer(0, "_Boxes", _worker.PeekOutputBuffer("regressors"));
        post1.SetBuffer(0, "_Output", _output.post1);
        post1.Dispatch(0, 1, 1, 1);

        post1.SetBuffer(1, "_Scores", _worker.PeekOutputBuffer("classificators"));
        post1.SetBuffer(1, "_Boxes", _worker.PeekOutputBuffer("regressors"));
        post1.SetBuffer(1, "_Output", _output.post1);
        post1.Dispatch(1, 1, 1, 1);

        // Retrieve the bounding box count.
        GraphicsBuffer.CopyCount(_output.post1, _output.count, 0);

        // 2nd postprocess (overlap removal)
        var post2 = _resources.postprocess2;
        post2.SetBuffer(0, "_Input", _output.post1);
        post2.SetBuffer(0, "_Count", _output.count);
        post2.SetBuffer(0, "_Output", _output.post2);
        post2.Dispatch(0, 1, 1, 1);

        // Retrieve the bounding box count after removal.
        GraphicsBuffer.CopyCount(_output.post2, _output.count, 0);

        // Cache data invalidation
        _readCache.InvalidateCache();
    }

    #endregion
}

} // namespace MediaPipe.BlazePalm
