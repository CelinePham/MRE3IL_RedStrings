using System.Collections;
using UnityEngine;
using FMODUnity;

public class AmbiencePlayer : MonoBehaviour
{
    private StudioEventEmitter emitter;

    private IEnumerator Start()
    {
        // Warten bis FMODEvents.instance sicher gesetzt ist
        while (FMODEvents.instance == null)
            yield return null;

        emitter = gameObject.AddComponent<StudioEventEmitter>();
        emitter.EventReference = FMODEvents.instance.ambienceLoop;
        emitter.StopEvent = EmitterGameEvent.None;

        emitter.Play();
    }

    private void OnDestroy()
    {
        if (emitter != null)
            emitter.Stop();
    }
}