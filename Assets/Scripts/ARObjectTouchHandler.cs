using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARCore;

public class ARObjectTouchHandler : MonoBehaviour
{
    void Update()
    {
        Vector2 screenPos;

        // 에디터: 마우스 / 기기: 터치
#if UNITY_EDITOR
        if (!Input.GetMouseButtonDown(0)) return;
        screenPos = Input.mousePosition;
#else
    if (Input.touchCount == 0) return;
    if (Input.GetTouch(0).phase != TouchPhase.Began) return;
    screenPos = Input.GetTouch(0).position;
#endif

        Debug.Log($"Screen Position: {screenPos}");
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Hit Object: {hit.transform.name}");  
            hit.transform.GetComponent<Animator>().enabled = true;
        }
    }
}