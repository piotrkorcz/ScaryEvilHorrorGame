using UnityEngine;
using System.Collections;

public class BreathAnim : MonoBehaviour
{
    public Animation anim;
    public string breathAnim = "Breath";
    public string idleAnim = "BreathIdle";

    void Update()
    {
        if (!ControlFreak2.CF2Input.GetButton("Fire2"))
            anim.Play(breathAnim);
        else
            anim.CrossFade(idleAnim);
    }
}