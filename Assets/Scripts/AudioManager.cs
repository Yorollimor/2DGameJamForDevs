using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    FMODUnity.StudioBankLoader bankLoader;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        bankLoader = GetComponent<FMODUnity.StudioBankLoader>();
    }

    float timer = 0f;
    bool hasLoaded = false;
    private void Update()
    {
        if(hasLoaded) return;
        timer += Time.deltaTime;
        if (FMODUnity.RuntimeManager.HasBankLoaded("Master") && !hasLoaded)
        {
            hasLoaded = true;
            print("Master bank loaded after: " + timer +"seconds");
        }
    }
}
