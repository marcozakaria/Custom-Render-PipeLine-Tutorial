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
        ScriptableCullingParameters cullingParameters;
        if(!CullResults.GetCullingParameters(camera, out cullingParameters))  // check if culling parameters are valid else return
        {   
            return;
        }

        CullResults cull = CullResults.Cull(ref cullingParameters, context);  //CullResults contains information about what is visible in the context

                context.SetupCameraProperties(camera);

        var buffer = new CommandBuffer {
                 name = camera.name
            }; // Command buffers hold list of rendering commands ("set render target, draw mesh, ...") to be excuted

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