using System;

[Serializable]
public class MarkedSelection
{
    
    public string text;
    public int lawId;
    public int startIndex;
    public int endIndex;

    public MarkedSelection(string text, int lawId = -1, int startIndex = -1, int endIndex = -1)
    {
        this.text = text;
        this.lawId = lawId;
        this.startIndex = startIndex;
        this.endIndex = endIndex;
    }
    public bool Overlaps(MarkedSelection other)
    {
        return startIndex < other.endIndex && endIndex > other.startIndex;
    }
}