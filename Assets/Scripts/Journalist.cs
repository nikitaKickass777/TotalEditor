
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Journalist
{
        public int id;
        public string name;
        public float relationship; // 0 to 100, with 100 being the best relationship
        public int reprimands; // Number of reprimands received
        [NonSerialized] public Sprite portrait; // Journalist's portrait
        [NonSerialized] public Sprite portraitTalking; // Journalist's portrait when talking
        [NonSerialized] public Sprite portraitDoor; // Journalist's portrait when at the door
}
//Wrapper needed for the JsonUtility to work with lists!
[Serializable]
public class JournalistList
{
        public List<Journalist> journalists;
        public JournalistList()
        {
                journalists = new List<Journalist>();
        }
}