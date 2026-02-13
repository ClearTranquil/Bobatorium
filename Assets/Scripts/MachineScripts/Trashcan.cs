using UnityEngine;
using System.Collections;

public class Trashcan : Machine
{
    [Header("Trash Settings")]
    [SerializeField] private float disposeDuration = 1f;
    [SerializeField] private float disposeSpeed = 1f;

    public override void OnCupInserted(Cup cup)
    {
        base.OnCupInserted(cup);

        StartCoroutine(Dispose(cup));
    }

    private IEnumerator Dispose(Cup cup)
    {
        float elapsed = 0f;

        while (elapsed < disposeDuration)
        {
            elapsed += Time.deltaTime;
            cup.transform.Translate(Vector3.down * disposeSpeed * Time.deltaTime);
            yield return null;
        }

        foreach (CupSnapPoint snap in cupSnapPoints)
        {
            snap.Clear();
        }

        Destroy(cup.gameObject);
    }
}
