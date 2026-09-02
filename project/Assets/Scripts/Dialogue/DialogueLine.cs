using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea] public string text;
    public Sprite speakerPortrait;
}