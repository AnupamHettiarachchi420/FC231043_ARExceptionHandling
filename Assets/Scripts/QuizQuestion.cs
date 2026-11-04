using UnityEngine;

[System.Serializable]
public class QuizQuestion
{
    public string id;           // for example A1 or B1
    public string set;          // "A" = pre-test, "B" = post-test
    public int objective;       // 1 to 5, matching the Project Charter
    [TextArea(2, 10)] public string text;
    public string[] options = new string[4];
    public int correct;         // index of the correct option; the starter data keeps it first
    [TextArea(1, 5)] public string explanation;
}