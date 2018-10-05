using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class MyPipeline : RenderPipeline
{
    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        base.Render(renderContext, cameras);

        foreach (Camera cam in cameras)
        {
            Render(renderContext, cam);
        }
    }

    void Render(ScriptableRenderContext context,Camera camera)
    {
        context.SetupCameraProperties(camera);

        var buffer = new CommandBuffer { name = camera.name }; // buffer holds list of graphics commands to be excuted
        CameraClearFlags clearFlags = camera.clearFlags;
        buffer.ClearRenderTarget(
                                 (clearFlags & CameraClearFlags.Depth) !=0,
                                 (clearFlags & CameraClearFlags.Color) != 0,
                                 camera.backgroundColor );
        context.ExecuteCommandBuffer(buffer);
        buffer.Release(); // release resorces used by buffer

        context.DrawSkybox(camera);
        context.Submit();  // must call submit to make draw functions work
    }
}