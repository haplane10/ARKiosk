using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARGuideController : MonoBehaviour
{
    public static ARGuideController Instance;

    [SerializeField] private GeminiManager geminiManager;
    [SerializeField] private AndroidTTSManager ttsManager;
    [SerializeField] private Camera arCamera;
    public Text debugText;

    private bool isProcessing = false;

    public void Awake()
    {
            Instance = this;
    }
    private void Start()
    {
       
    }

    public void SetImageAndCallAI(string imageName)
    {
        if (string.IsNullOrEmpty(imageName))
        {
            debugText.text = "이미지가 null입니다.";
            return;
        }

        if (hadImageName == imageName)
        {
            return;
        }

        hadImageName = imageName;
        debugText.text = $"새로운 이미지 인식: {imageName}";
        StartCoroutine(ImageAndAsk());
    }

    // ARTrackedImageManager의 trackedImagesChanged 이벤트에 연결
    public void OnImageTracked(ARTrackedImagesChangedEventArgs args)
    {
        if (isProcessing) return;

        foreach (var trackedImage in args.added)
        {
            isProcessing = true;
            StartCoroutine(co_CaptureAndAsk(trackedImage.referenceImage.name));
        }
    }

    string imageName = string.Empty;
    public void CaptureAndAsk(ARTrackedImage trackedImage)
    {
        if (imageName == trackedImage.referenceImage.name) return; // 같은 이미지면 처리하지 않음
       
        imageName = trackedImage.referenceImage.name;
        StartCoroutine(co_CaptureAndAsk(trackedImage.referenceImage.name));
    }

    public System.Collections.IEnumerator co_CaptureAndAsk(string imageName)
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

    public string hadImageName;
    public string description = $"이 이미지를 한국어로 2~3문장으로 친절하게 설명해줘. ";
    public ImageLibrarySO imageLibrarySO;
    private System.Collections.IEnumerator ImageAndAsk()
    {
        // 카메라 화면을 Texture2D로 캡처
        yield return new WaitForEndOfFrame();

        //// 카메라 촬영 하고 있는 이미지를 가져오는 부분
        //var renderTexture = new RenderTexture(Screen.width / 2, Screen.height / 2, 0);
        //arCamera.targetTexture = renderTexture;
        //arCamera.Render();

        //// 카메라 이미지에 맞게 텍스쳐 메모리를 할당 
        //var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        //RenderTexture.active = renderTexture;
        //texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        //texture.Apply();

        //arCamera.targetTexture = null;
        //RenderTexture.active = null;

        // Gemini에 설명 요청 — 프롬프트를 원하는 스타일로 수정 가능
        string prompt = description + $"\n현재 인식된 오브젝트: {hadImageName}";

        var hadImage = imageLibrarySO.GetTextureByName(hadImageName);
        debugText.text = $"{hadImage} \nGemini에 요청: {prompt}";
        geminiManager.AskGemini(hadImage, prompt, (resultText) =>
        {
            if (!string.IsNullOrEmpty(resultText))
            {
                // TTS로 읽기
                ttsManager.Speak(resultText);
            }
            isProcessing = false;
            //Destroy(texture);
        });
        //Destroy(renderTexture);
    }
}