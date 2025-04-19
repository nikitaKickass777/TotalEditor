using System.Collections.Generic;
[System.Serializable]
public class DialogueList
{
    public List<Dialogue> dialogues;
}

[System.Serializable]
public class Dialogue
{
    public int dialogueId;
    public string dialogueName;
    public int characterId;
    public List<DialogueLine> lines;
}

[System.Serializable]
public class DialogueLine
{
    public int lineId;
    public string lineText;
    public int nextLineId = -1;
    public bool hasOptions;
    public List<DialogueOption> options;
}

[System.Serializable]
public class DialogueOption
{
    public int optionId;
    public int nextLineId = -1;
    public string optionText;
    public string conditionChange;
    
}
