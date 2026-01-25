using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Snipped : MonoBehaviour
{
    [Header("FMOD Events")]
    [SerializeField] private EventReference whisperLoop;     // SchnipselFluestern01
    [SerializeField] private EventReference pickupOneShot;   // SchnipselPickUp01
    [SerializeField] private EventReference vanishOneShot;   // SchnipselVerschwinden

    [Header("Fade")]
    [SerializeField] private float minFadeTime = 0.3f;

    private XRGrabInteractable grab;
    private StudioEventEmitter loopEmitter;

    private float pickupEndTime = 0f;
    private bool isDespawning = false;

    private Renderer rend;
    private Color initialColor;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);

        // Renderer finden (MeshRenderer oder SpriteRenderer)
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            initialColor = rend.material.color;

        // Flüstern-Loop
        loopEmitter = gameObject.AddComponent<StudioEventEmitter>();
        loopEmitter.EventReference = whisperLoop;
        loopEmitter.StopEvent = EmitterGameEvent.None;
    }

    private void Start()
    {
        if (!whisperLoop.IsNull)
            loopEmitter.Play();
    }

    private void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (isDespawning) return;

        if (loopEmitter.IsPlaying())
            loopEmitter.Stop();

        float pickupLen = GetEventLengthSeconds(pickupOneShot);
        pickupEndTime = Time.time + pickupLen;

        PlayEventInstance(pickupOneShot);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (isDespawning) return;
        StartCoroutine(DespawnSequence());
    }

    private IEnumerator DespawnSequence()
    {
        isDespawning = true;
        grab.enabled = false;

        // Warten bis Pickup fertig ist
        float waitPickup = Mathf.Max(0f, pickupEndTime - Time.time);
        if (waitPickup > 0f)
            yield return new WaitForSeconds(waitPickup);

        // Vanish starten
        float vanishLen = GetEventLengthSeconds(vanishOneShot);
        float fadeTime = Mathf.Max(minFadeTime, vanishLen);

        PlayEventInstance(vanishOneShot);

        // Fade-Out
        if (rend != null)
        {
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeTime);

                Color c = initialColor;
                c.a = Mathf.Lerp(1f, 0f, k);
                rend.material.color = c;

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeTime);
        }

        Destroy(gameObject);
    }

    // ---------- FMOD Helpers ----------

    private void PlayEventInstance(EventReference ev)
    {
        if (ev.IsNull) return;

        EventInstance inst = RuntimeManager.CreateInstance(ev);
        inst.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        inst.start();
        inst.release();
    }

    private float GetEventLengthSeconds(EventReference ev)
    {
        if (ev.IsNull) return 0f;

        try
        {
            EventDescription desc = RuntimeManager.GetEventDescription(ev);
            desc.getLength(out int lengthMs);
            return lengthMs / 1000f;
        }
        catch
        {
            return 0f;
        }
    }
}
