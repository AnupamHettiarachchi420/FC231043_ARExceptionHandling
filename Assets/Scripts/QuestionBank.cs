using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionBank", menuName = "Exception Explorer/Question Bank")]
public class QuestionBank : ScriptableObject
{
    public List<QuizQuestion> questions = new();

    public List<QuizQuestion> GetSet(string set)
    {
        var list = new List<QuizQuestion>();
        foreach (QuizQuestion q in questions)
            if (q.set == set) list.Add(q);
        return list;
    }

#if UNITY_EDITOR
    [ContextMenu("Fill with starter questions")]
    private void FillStarter()
    {
        questions = StarterQuestions.Create();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}