using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    [SerializeField] float shrinkDuration = 0.2f;

    public float GetShrinkDuration => shrinkDuration;

    public void MoveTo(GameObject piece, Vector2 position, float duration)
    {
        StartCoroutine(MoveToCoroutine(piece, position, duration));
    }

    IEnumerator MoveToCoroutine(GameObject obj, Vector2 position, float duration)
    {
        RectTransform rectTransform = obj.GetComponent<RectTransform>();

        if (duration == 0)
        {
            rectTransform.anchoredPosition = position;
            yield break;
        }

        float t = 0;
        Vector2 startPos = rectTransform.anchoredPosition;

        while (t < 1)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, position, t);
            t += Time.deltaTime / duration;
            yield return null;
        }

        rectTransform.anchoredPosition = position;
    }

    public void Shrink(GameObject obj)
    {
        StartCoroutine(ShrinkCoroutine(obj));
    }

    IEnumerator ShrinkCoroutine(GameObject obj)
    {
        float t = 0;

        while (t < 1)
        {
            obj.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            t += Time.deltaTime / shrinkDuration;
            yield return null;
        }

        obj.transform.localScale = Vector3.zero;
    }
}
