using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AsyncReadbackExample : MonoBehaviour
{
    private CommandBuffer _commandBuffer;
    private RenderTexture _renderTexture;

    private void OnEnable()
    {
        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "ReadbackExample";
        

        _renderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();

        _commandBuffer.SetRenderTarget(_renderTexture);
        _commandBuffer.ClearRenderTarget(true, true, Color.green);

        Graphics.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();

        AsyncGPUReadback.Request(_renderTexture, 0, OnCompleteReadback);
    }

    private void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("Failed to read GPU texture");
            return;
        }

        Debug.Log("Successfully read GPU texture");

        // Access your texture data here using request.GetData<Color32>()
    }

    private void OnDisable()
    {
        if (_commandBuffer != null)
        {
            _commandBuffer.Dispose();
        }

        if (_renderTexture != null)
        {
            _renderTexture.Release();
        }
    }
}
