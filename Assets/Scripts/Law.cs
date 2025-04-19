

using System;
using System.Collections.Generic;

[Serializable]
public class Law
{
    public int id = -1; // index of the law
    public bool isProhibition = true; // true if the law is a prohibition, false if it is a recommendation
    public bool isActive = false; // true if the law is active, false if it is not
    public string text = ""; // text of the law
    
}

[Serializable]
public class LawList
{
    public List<Law> laws;
    public LawList()
    {
        laws = new List<Law>();
    }
}