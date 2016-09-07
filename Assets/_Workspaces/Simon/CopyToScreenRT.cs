using System.Collections;
using UnityEngine;

// fixes the deferred lighting missing final copy&resolve, so the next camera gets the correctly final processed image in the temp screen RT as input
// NOTE: The script must be the last in the image effect chain, so order it in the inspector!
public class CopyToScreenRT : MonoBehaviour
{
    public RenderTexture target; // hold the org. screen RT
    public bool render = false;
   /* private void OnPreRender()
    {
        if (camera.actualRenderingPath == RenderingPath.DeferredLighting)
        {
            activeRT = RenderTexture.active;
        }
        else
        {
            activeRT = null;
        }
    }
    */

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (target != null && render)
        {
            Graphics.Blit(src, target);
            render = false;
        }
       /* if (camera.actualRenderingPath == RenderingPath.DeferredLighting && activeRT)
        {
            if (src.format == activeRT.format)
            {
                Graphics.Blit(src, activeRT);
            }
            else
            {
                Debug.LogWarning("Cant resolve texture, because of different formats!");
            }
        }

        * */
        // script must be last anyway, so we don't need a final copy?
        Graphics.Blit(src, dest); // just in case we are not last!
    }
}