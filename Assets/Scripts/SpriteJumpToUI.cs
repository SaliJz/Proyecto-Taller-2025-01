using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteJumpToUI : MonoBehaviour
{
    private SpriteRenderer sourceSprite;
    public Canvas uiCanvas;
    public Image imageTemplate;
    public RectTransform targetUIPosition;
    [SerializeField] float duration=0.5f;

    public bool ejecutarAnimacion = false;
    private void Start()
    {
        sourceSprite = GetComponent<SpriteRenderer>();
    }
    public void JumpSpriteToHUD()
    {
        StartCoroutine(AnimateToHUD());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ejecutarAnimacion = true;
        }

        if (ejecutarAnimacion)
        {
            ejecutarAnimacion = false;
            JumpSpriteToHUD();
        }
    }

    IEnumerator AnimateToHUD()
    {
        if (imageTemplate == null || uiCanvas == null || sourceSprite == null)
        {
            Debug.LogError("[SpriteJumpToUI] Faltan referencias: imageTemplate, uiCanvas o sourceSprite no están asignados.");
            yield break;
        }

        Image movingImage = Instantiate(imageTemplate, uiCanvas.transform);
        movingImage.sprite = sourceSprite.sprite;
        movingImage.gameObject.SetActive(true);

        RectTransform imgRect = movingImage.GetComponent<RectTransform>();

        Vector2 startPos = WorldToCanvasPosition(Camera.main, uiCanvas, sourceSprite.transform.position);
        imgRect.anchoredPosition = startPos;

        Vector3[] corners = new Vector3[4];
        sourceSprite.GetComponent<Renderer>().bounds.GetBoundsCorners(Camera.main, corners);
        float screenWidth = Vector3.Distance(Camera.main.WorldToScreenPoint(corners[0]), Camera.main.WorldToScreenPoint(corners[3]));
        float screenHeight = Vector3.Distance(Camera.main.WorldToScreenPoint(corners[0]), Camera.main.WorldToScreenPoint(corners[1]));
        float canvasScale = uiCanvas.scaleFactor;

        Vector2 startSize = new Vector2(screenWidth, screenHeight) / canvasScale;
        imgRect.sizeDelta = startSize;

        Vector2 endPos = targetUIPosition.anchoredPosition;
        Vector2 endSize = imageTemplate.rectTransform.sizeDelta;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            imgRect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            imgRect.sizeDelta = Vector2.Lerp(startSize, endSize, progress);

            yield return null;
        }

        imgRect.anchoredPosition = endPos;
        imgRect.sizeDelta = endSize;

        Destroy(movingImage.gameObject,2);
    }

    Vector2 WorldToCanvasPosition(Camera camera, Canvas canvas, Vector3 worldPosition)
    {
        Vector3 screenPos = camera.WorldToScreenPoint(worldPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera,
            out Vector2 localPoint
        );

        return localPoint;
    }

}


public static class BoundsExtensions
{
    public static void GetBoundsCorners(this Bounds bounds, Camera cam, Vector3[] corners)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z); // bottom-left-front
        corners[1] = center + new Vector3(-extents.x, extents.y, -extents.z);  // top-left-front
        corners[2] = center + new Vector3(extents.x, extents.y, -extents.z);   // top-right-front
        corners[3] = center + new Vector3(extents.x, -extents.y, -extents.z);  // bottom-right-front
    }
}
