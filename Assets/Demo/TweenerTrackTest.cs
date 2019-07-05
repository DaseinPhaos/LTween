using UnityEngine;

public class TweenerTrackTest : MonoBehaviour
{
    public Luxko.Tween.TweenerTrack testTrack;

    public enum EnumToEdit { Zero, One, Two, }
    public EnumToEdit enumToEdit;

    [Luxko.Tween.TweenerTrack.Enum(typeof(EnumToEdit))]
    public int EnumToEditWrap
    {
        get { return (int)enumToEdit; }
        set { enumToEdit = (EnumToEdit)value; }
    }

    public void InvokeTestAsCoroutine()
    {
        testTrack.InvokeBy(this);
    }

    public void EvaluationFromScript()
    {
        // NOTE: track.Reset() needs to be invoked ONCE prior to manual update.
        // The invocation can be put inside any initialization function as you see fit, for example, the MonoBehaviour's Start()/Awake() functions.
        // testTrack.Reset();
        testTrack.SetTimeThenEvaluate(0.5f);
    }
}
