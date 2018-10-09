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

        DrawRendererSettings drawSettings = new DrawRendererSettings(
                camera, new ShaderPassName("SRPDefaultUnlit")    // The camera is used to setup sorting and culling layers, while the pass controls which shader pass is used for rendering.
            ) ;
        drawSettings.sorting.flags = SortFlags.CommonOpaque;    //This instructs Unity to sort the renderers by distance from front to back

        // delay drawing transparent renderers until after the skybox to avoid over draw of objects with skybox
        FilterRenderersSettings filterSettings = new FilterRenderersSettings(true) {    // limit the draw before the skybox to only the opaque renderers.
                renderQueueRange = RenderQueueRange.opaque   
            };

        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

        context.DrawSkybox(camera);

        // render transparent after skybox
        // Trnsparent combines the color of what's being drawn with what has been drawn before,
        drawSettings.sorting.flags = SortFlags.CommonTransparent;
        filterSettings.renderQueueRange = RenderQueueRange.transparent;  
        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

        context.Submit();  // must call submit to make draw functions work
    }
}