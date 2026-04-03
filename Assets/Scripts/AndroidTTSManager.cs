using UnityEngine;

public class AndroidTTSManager : MonoBehaviour
{
    private AndroidJavaObject tts;
    private bool isInitialized = false;

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        InitializeTTS();
#endif
    }

    private void InitializeTTS()
    {
        // Android 메인 스레드에서 실행
        AndroidJNI.AttachCurrentThread();

        var unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity");

        // TextToSpeech 객체 생성, OnInitListener를 람다로 연결
        tts = new AndroidJavaObject(
            "android.speech.tts.TextToSpeech",
            unityActivity,
            new TTSInitListener(OnTTSInit)
        );
    }

    private void OnTTSInit(bool success)
    {
        if (success)
        {
            // 언어를 한국어로 설정
            var locale = new AndroidJavaClass("java.util.Locale");
            var korean = locale.GetStatic<AndroidJavaObject>("KOREAN");
            int result = tts.Call<int>("setLanguage", korean);

            isInitialized = (result != -2 && result != -1); // LANG_NOT_SUPPORTED, LANG_MISSING_DATA 체크
            if (!isInitialized)
                Debug.LogWarning("기기에 한국어 TTS 데이터가 없습니다. 설정 > 언어 > TTS에서 한국어를 설치하세요.");
            else
                Debug.Log("Android TTS 초기화 완료");
        }
        else
        {
            Debug.LogError("Android TTS 초기화 실패");
        }
    }

    public void Speak(string text)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("TTS가 아직 초기화되지 않았습니다.");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // QUEUE_FLUSH(0): 기존 음성 중단하고 즉시 재생
        // QUEUE_ADD(1):   기존 음성 이후에 추가
        tts.Call<int>("speak", text, 0, null, "utterance_" + Time.time);
#else
        Debug.Log($"[TTS 시뮬레이션] {text}");
#endif
    }

    public void Stop()
    {
        tts?.Call<int>("stop");
    }

    public void SetSpeechRate(float rate) // 1.0 = 기본속도, 0.5 = 느리게
    {
        tts?.Call<int>("setSpeechRate", rate);
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        tts?.Call<int>("shutdown");
#endif
    }
}

// Android OnInitListener 인터페이스 구현
public class TTSInitListener : AndroidJavaProxy
{
    private readonly System.Action<bool> callback;

    public TTSInitListener(System.Action<bool> callback)
        : base("android.speech.tts.TextToSpeech$OnInitListener")
    {
        this.callback = callback;
    }

    public void onInit(int status)
    {
        // SUCCESS = 0
        callback?.Invoke(status == 0);
    }
}