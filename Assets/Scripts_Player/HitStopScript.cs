using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopScript : MonoBehaviour
{
    public void HitStop(float duration)
    {
        StartCoroutine(PerformStop(duration));
    }
    IEnumerator PerformStop(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
}
