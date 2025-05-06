using UnityEngine;
using UnityEngine.VFX;

public class ExplosionController : MonoBehaviour
{
    [Header("VFX 设置")]
    public VisualEffect vfx;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            vfx.Play();
            Debug.Log("vfx.Play()");
        }
    }
}
