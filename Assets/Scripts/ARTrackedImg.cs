using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTrackedImg : MonoBehaviour
{
    public float _timer;
    public ARTrackedImageManager trackedImageManager;
    public List<GameObject> _objectList = new List<GameObject>();
    private Dictionary<string, GameObject> _prefabDic = new Dictionary<string, GameObject>();
    private List<ARTrackedImage> _trackedImg = new List<ARTrackedImage>();
    private List<float> _trackedTimer = new List<float>();
    public Text trackedText;
    public Text debugText;
    public XRReferenceImageLibrary library;

    void Awake()
    {
        foreach (GameObject obj in _objectList)
        {
            string tName = obj.name;
            _prefabDic.Add(tName, obj);
        }
    }

    void Update()
    {
        if (_trackedImg.Count > 0)
        {
            List<ARTrackedImage> tNumList = new List<ARTrackedImage>();
            for (var i = 0; i < _trackedImg.Count; i++)
            {
                if (_trackedImg[i].trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited)
                {
                    if (_trackedTimer[i] > _timer)
                    {
                        string name = _trackedImg[i].referenceImage.name;
                        GameObject tObj = _prefabDic[name];
                        tObj.SetActive(false);
                        tNumList.Add(_trackedImg[i]);
                    }
                    else
                    {
                        _trackedTimer[i] += Time.deltaTime;
                    }
                }
            }

            if (tNumList.Count > 0)
            {
                for (var i = 0; i < tNumList.Count; i++)
                {
                    int num = _trackedImg.IndexOf(tNumList[i]);
                    _trackedImg.Remove(_trackedImg[num]);
                    _trackedTimer.Remove(_trackedTimer[num]);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            trackedText.text = "Space Key Pressed!";
        }
    }

    private void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(ImageChanged);
    }
    private void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(ImageChanged);
    }

    private void ImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        try
        {
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                if (!_trackedImg.Contains(trackedImage))
                {
                    _trackedImg.Add(trackedImage);
                    _trackedTimer.Add(0);
                }
            }

            foreach (ARTrackedImage trackedImage in eventArgs.updated)
            {
                if (!_trackedImg.Contains(trackedImage))
                {
                    _trackedImg.Add(trackedImage);
                    _trackedTimer.Add(0);
                }
                else
                {
                    int num = _trackedImg.IndexOf(trackedImage);
                    _trackedTimer[num] = 0;
                }

                UpdateImage(trackedImage);
            }
        }
        catch (System.Exception e)
        {
            debugText.text = $"인식오류 {e.Message}\n{e.StackTrace}";
        }
        
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;
        //name = "Kimbab";
        trackedText.text = name;

        GameObject tObj = _prefabDic[name];
        tObj.transform.position = trackedImage.transform.position;
        tObj.transform.rotation = trackedImage.transform.rotation;
        tObj.SetActive(true);

       
        var refImage = GetReferenceImageByName(name);
       // ARGuideController.Instance.SetImageAndCallAI(name);
    }

    XRReferenceImage? GetReferenceImageByName(string name)
    {
        for (int i = 0; i < library.count; i++)
        {
            var refImage = library[i];
            if (refImage.name == name)
                return refImage;
        }
        return null;
    }
}