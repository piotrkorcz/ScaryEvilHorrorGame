using UnityEngine;

public enum wrapModes { Default, Once, Loop, PingPong, ClampForever }

public class AnimationPlayEvent : MonoBehaviour {

    public Animation m_animation;
    public string animationName;
    public wrapModes wrapMode = wrapModes.Default;
    private bool isPlayed = false;

    public void PlayAnimation()
    {
        if (!isPlayed)
        {
            m_animation[animationName].wrapMode = (WrapMode)System.Enum.Parse(typeof(WrapMode), wrapMode.ToString());
            m_animation.Play(animationName);
            isPlayed = true;
        }
    }
}
