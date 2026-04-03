using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiManager : MonoBehaviour
{
    [Header("API 설정")]
    [SerializeField] private string apiKey = "AIzaSyC3F2A8S9Gk4vEwuuRwQ71YuGbyWtcYPuM";
    private const string API_URL =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    // 이미지(Texture2D)를 받아 Gemini Vision으로 설명 요청
    public void AskGemini(Texture2D image, string prompt, Action<string> onResult)
    {
        StartCoroutine(SendRequest(image, prompt, onResult));
    }

    private IEnumerator SendRequest(Texture2D image, string prompt, Action<string> onResult)
    {
        // 1. 이미지를 base64로 인코딩
        byte[] imageBytes = image.EncodeToJPG(75);
        string base64Image = Convert.ToBase64String(imageBytes);

        // 2. Gemini API JSON 바디 구성
        string json = $@"{{
            ""contents"": [{{
                ""parts"": [
                    {{
                        ""inline_data"": {{
                            ""mime_type"": ""image/jpeg"",
                            ""data"": ""{base64Image}""
                        }}
                    }},
                    {{
                        ""text"": ""{prompt}""
                    }}
                ]
            }}]
        }}";

        // 3. UnityWebRequest로 POST 전송
        string url = $"{API_URL}?key={apiKey}";
        using var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 4. 응답 파싱: candidates[0].content.parts[0].text
            var response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
            string resultText = response.candidates[0].content.parts[0].text;
            onResult?.Invoke(resultText);
        }
        else
        {
            Debug.LogError($"Gemini 오류: {request.error}\n{request.downloadHandler.text}");
            onResult?.Invoke(null);
        }
    }
}

// JSON 역직렬화용 클래스
[Serializable]
public class GeminiResponse
{
    public GeminiCandidate[] candidates;
}
[Serializable]
public class GeminiCandidate
{
    public GeminiContent content;
}
[Serializable]
public class GeminiContent
{
    public GeminiPart[] parts;
}
[Serializable]
public class GeminiPart
{
    public string text;
}