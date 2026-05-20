using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImageLibrarySO", menuName = "ScriptableObjects/ImageLibrarySO", order = 1)]
public class ImageLibrarySO : ScriptableObject
{
    public List<ImageEntry> imageEntries;

    public Texture2D GetTextureByName(string name)
    {
        return imageEntries.Find(e => e.imageName == name)?.texture;
    }
}

[System.Serializable]
public class ImageEntry
{
    public string imageName;
    public Texture2D texture;
}