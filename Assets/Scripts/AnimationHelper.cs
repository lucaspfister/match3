using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    public void MoveTo(GameObject piece, Vector2 position, float duration)
    {
        StartCoroutine(Move(piece, position, duration));
    }

    IEnumerator Move(GameObject piece, Vector2 position, float duration)
    {
        RectTransform rectTransform = piece.GetComponent<RectTransform>();

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
}
