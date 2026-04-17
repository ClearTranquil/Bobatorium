using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TMPro;

public class CustomerCutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    private GameManager gameManager;

    [Header("Bindings")]
    [SerializeField] private AnimationTrack animationTrack;
    private Customer pendingCustomer;

    [Header("UI")]
    [SerializeField] private TMP_Text conversionText;
    [SerializeField] private TMP_Text customerBarkText;

    private System.Action currentOnComplete;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void PlayCutscene(Customer cus, System.Action onComplete)
    {
        if (director.state == PlayState.Playing)
            return;

        UpdateUI(cus);

        pendingCustomer = cus;
        currentOnComplete = onComplete;

        gameManager.PauseForCutscene();

        director.Stop();

        Debug.Log(animationTrack == null ? "TRACK IS NULL" : "TRACK OK");
        Debug.Log(cus.Animator == null ? "ANIMATOR NULL" : "ANIMATOR OK");

        foreach (var output in director.playableAsset.outputs)
        {
            if (output.sourceObject is AnimationTrack track)
            {
                Debug.Log($"Found animation track: {output.streamName}");

                director.SetGenericBinding(track, cus.Animator);
                break;
            }
        }

        director.RebuildGraph();

        director.time = 0;
        director.Evaluate();

        director.stopped -= OnStopped;
        director.stopped += OnStopped;

        director.Play();
    }

    public void ShowRegularStar()
    {
        if (pendingCustomer == null)
            return;

        Debug.Log($"Star for {pendingCustomer.name}");

        pendingCustomer.ActivateStar();
    }

    public void UpdateUI(Customer cus)
    {
        CustomerProfile profile = cus.Profile;

        conversionText.text = profile.customerName + " is now a Regular!";
        customerBarkText.text = profile.bark;
    }

    private void OnStopped(PlayableDirector d)
    {
        director.stopped -= OnStopped;

        gameManager.UnpauseCutscene();

        currentOnComplete?.Invoke();
        currentOnComplete = null;
    }
}
