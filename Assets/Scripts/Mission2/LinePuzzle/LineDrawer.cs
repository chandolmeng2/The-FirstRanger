using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    public GameObject linePrefab;
    public Transform lineParent;

    private Dictionary<string, List<GameObject>> linesByColor = new Dictionary<string, List<GameObject>>();

    public void ClearLine(string color)
    {
        if (!linesByColor.ContainsKey(color)) return;

        foreach (var obj in linesByColor[color])
            Destroy(obj);
        linesByColor[color].Clear();
    }

    public void DrawLine(Vector2 start, Vector2 end, string color)
    {
        GameObject lineObj = Instantiate(linePrefab, lineParent);
        RectTransform rect = lineObj.GetComponent<RectTransform>();

        Vector2 dir = end - start;
        float length = dir.magnitude - 15f;

        if (length < 1f) return;

        Vector2 center = (start + end) / 2f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float thickness = 7f;

        // ? position 기준으로 설정
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.position = center;
        rect.rotation = Quaternion.Euler(0, 0, angle);
        rect.sizeDelta = new Vector2(0, thickness);

        Image img = lineObj.GetComponent<Image>();
        img.color = GetColor(color);

        rect.DOSizeDelta(new Vector2(length, thickness), 0.15f).SetEase(Ease.OutCubic);

        if (!linesByColor.ContainsKey(color))
            linesByColor[color] = new List<GameObject>();
        linesByColor[color].Add(lineObj);
    }


    private Color GetColor(string color)
    {
        return color switch
        {
            "Red" => Color.red,
            "Blue" => Color.blue,
            "Green" => Color.green,
            "Yellow" => Color.yellow,
            _ => Color.white
        };
    }
}
