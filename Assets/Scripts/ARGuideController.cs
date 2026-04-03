using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARGuideController : MonoBehaviour
{
    [SerializeField] private GeminiManager geminiManager;
    [SerializeField] private AndroidTTSManager ttsManager;
    [SerializeField] private Camera arCamera;

    private bool isProcessing = false;

    // ARTrackedImageManager의 trackedImagesChanged 이벤트에 연결
    public void OnImageTracked(ARTrackedImagesChangedEventArgs args)
    {
        if (isProcessing) return;

        foreach (var trackedImage in args.added)
        {
            isProcessing = true;
            StartCoroutine(CaptureAndAsk(trackedImage.referenceImage.name));
        }
    }

    private System.Collections.IEnumerator CaptureAndAsk(string imageName)
    {
        // 카메라 화면을 Texture2D로 캡처
        yield return new WaitForEndOfFrame();

        var renderTexture = new RenderTexture(Screen.width / 2, Screen.height / 2, 0);
        arCamera.targetTexture = renderTexture;
        arCamera.Render();

        var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        arCamera.targetTexture = null;
        RenderTexture.active = null;

        // Gemini에 설명 요청 — 프롬프트를 원하는 스타일로 수정 가능
        string prompt = $"이 이미지를 한국어로 2~3문장으로 친절하게 설명해줘. " +
                        $"현재 인식된 오브젝트: {imageName}";

        geminiManager.AskGemini(texture, prompt, (resultText) =>
        {
            if (!string.IsNullOrEmpty(resultText))
            {
                // TTS로 읽기
                ttsManager.Speak(resultText);
            }
            isProcessing = false;
            Destroy(texture);
        });

        Destroy(renderTexture);
    }
}