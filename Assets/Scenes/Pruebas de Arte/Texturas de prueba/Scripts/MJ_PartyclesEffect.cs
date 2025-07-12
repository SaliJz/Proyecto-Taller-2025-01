using System.Collections;
using UnityEngine;

public class MJ_PartyclesEffect : MonoBehaviour
{
    [SerializeField] float Delay;
    [SerializeField] GameObject particulas;

    private void Start()
    {
        StartCoroutine(delayPartycles());
    }

    IEnumerator delayPartycles()
    {
        yield return new WaitForSeconds(Delay);

        particulas.SetActive(true);
    }
}
