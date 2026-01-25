using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    public static FMODEvents instance { get; private set; }

    [field: Header("Ambience")]
    [field: SerializeField]
    public EventReference ambienceLoop { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Found more than one FMODEvents instance in the scene.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
}