using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitTest : MonoBehaviour
{
    public Image alksdjflkaj, eeeeeeeeeeee;
    public Data_Manager.FishStruct fishStruct;
    public bool hit;
    Coroutine hitCoroutine;

    void Start()
    {
        HitStart();
    }

    public void InputHit()
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        if (hit == true)
        {
            Debug.LogWarning("��Ʈ ����");
        }
        else
        {
            Debug.LogWarning("��Ʈ ����");
        }
    }

    void HitStart()
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        hitCoroutine = StartCoroutine(HitState());
    }

    IEnumerator HitState()
    {
        float randomDelay = Random.Range(fishStruct.hitValue.x, fishStruct.hitValue.y);
        yield return new WaitForSeconds(randomDelay);
        hit = true;
        float hitDelay = 0.5f;
        yield return new WaitForSeconds(hitDelay);
        hit = false;
    }
}
