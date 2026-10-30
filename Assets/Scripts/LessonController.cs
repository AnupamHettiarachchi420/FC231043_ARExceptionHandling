using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class LessonController : MonoBehaviour
{
    [Header("Lesson")]
    [SerializeField] private LessonId lessonId = LessonId.Scene1_NormalFlow;

    [Header("References")]
    [SerializeField] private ExceptionStage stage;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text codeTitle;
    [SerializeField] private TMP_Text modeButtonLabel;

    [Header("Button groups")]
    [SerializeField] private GameObject guidedButtons;
    [SerializeField] private GameObject predictButtons;

    [Header("Predict answer labels (A, B, C)")]
    [SerializeField] private TMP_Text[] optionLabels;

    private Lesson lesson;
    private bool predictMode;

    // Guided state
    private int step;

    // Predict state
    private int pStep;
    private int qIndex;
    private int score;
    private int correctIndex;
    private bool awaiting;
    private bool finished;

    private void Start()
    {
        lesson = Lessons.Get(lessonId);
        predictMode = false;
        ApplyModeUi();
        ResetGuided();
    }

    // ---------- Mode switching ----------

    public void ToggleMode()
    {
        if (stage.IsBusy) return;
        StopAllCoroutines();
        predictMode = !predictMode;
        ApplyModeUi();
        if (predictMode) StartPredict(); else ResetGuided();
    }

    private void ApplyModeUi()
    {
        guidedButtons.SetActive(!predictMode);
        predictButtons.SetActive(predictMode);
        modeButtonLabel.text = predictMode ? "Mode: Predict" : "Mode: Guided";
    }

    // ---------- Guided mode ----------

    public void Next()
    {
        if (predictMode || stage.IsBusy) return;
        if (step >= lesson.steps.Length)
        {
            stage.Say("Lesson finished. Press Reset to run it again.");
            return;
        }
        RenderCode(step, false);
        stage.Play(lesson.steps[step]);
        step++;
        codeTitle.text = "CODE   " + step + " / " + lesson.steps.Length;
    }

    public void Back()
    {
        if (predictMode || stage.IsBusy || step == 0) return;
        step--;
        stage.ResetStage(lesson.rootFrame, lesson.rootHasTry);
        for (int i = 0; i < step; i++) stage.ApplyInstant(lesson.steps[i]);
        stage.Say(step == 0
            ? "Back at the start."
            : "Went back. " + lesson.steps[step - 1].message);
        RenderCode(step - 1, false);
        codeTitle.text = "CODE   " + step + " / " + lesson.steps.Length;
    }

    public void RestartLesson()
    {
        if (predictMode || stage.IsBusy) return;
        ResetGuided();
    }

    private void ResetGuided()
    {
        step = 0;
        stage.ResetStage(lesson.rootFrame, lesson.rootHasTry);
        stage.Say("Press Next to run the first line.");
        RenderCode(-1, false);
        codeTitle.text = "CODE   0 / " + lesson.steps.Length;
    }

    // ---------- Predict mode ----------

    private void StartPredict()
    {
        pStep = 0;
        qIndex = 0;
        score = 0;
        finished = false;
        awaiting = false;
        stage.ResetStage(lesson.rootFrame, lesson.rootHasTry);
        foreach (TMP_Text l in optionLabels) l.transform.parent.gameObject.SetActive(true);
        stage.Say("Get ready. Predict what happens before each step runs.");
        RenderCode(-1, false);
        codeTitle.text = "PREDICT  0/" + lesson.questions.Length + "  Score 0";
        StartCoroutine(PredictLoop());
    }

    private IEnumerator PredictLoop()
    {
        while (true)
        {
            while (stage.IsBusy) yield return null;
            yield return new WaitForSeconds(1.0f);

            if (qIndex < lesson.questions.Length && lesson.questions[qIndex].beforeStep == pStep)
            {
                AskQuestion(lesson.questions[qIndex]);
                yield break;
            }
            if (pStep >= lesson.steps.Length)
            {
                Finish();
                yield break;
            }
            RenderCode(pStep, false);
            stage.Play(lesson.steps[pStep]);
            pStep++;
        }
    }

    private void AskQuestion(LessonQuestion q)
    {
        var order = new List<int> { 0, 1, 2 };
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        for (int i = 0; i < 3; i++)
        {
            optionLabels[i].text = q.options[order[i]];
            optionLabels[i].transform.parent.gameObject.SetActive(true);
            if (order[i] == q.correct) correctIndex = i;
        }

        awaiting = true;
        stage.Say("Q" + (qIndex + 1) + "/" + lesson.questions.Length + ": " + q.prompt);
        RenderCode(pStep, true);
        codeTitle.text = "PREDICT  " + (qIndex + 1) + "/" + lesson.questions.Length + "  Score " + score;
    }

    // Called by the three answer buttons (0, 1, 2)
    public void Answer(int index)
    {
        if (!predictMode) return;
        if (finished) { StartPredict(); return; }
        if (!awaiting) return;
        awaiting = false;

        bool right = index == correctIndex;
        if (right) score++;
        string prefix = right
            ? "<color=#6FE3A0>Correct!</color> "
            : "<color=#FF7B7B>Not quite.</color> ";

        RenderCode(pStep, false);
        stage.Play(lesson.steps[pStep], prefix);
        pStep++;
        qIndex++;
        codeTitle.text = "PREDICT  " + qIndex + "/" + lesson.questions.Length + "  Score " + score;
        StartCoroutine(PredictLoop());
    }

    private void Finish()
    {
        finished = true;
        awaiting = false;

        string key = "Best_" + lessonId;
        int best = Mathf.Max(PlayerPrefs.GetInt(key, 0), score);
        PlayerPrefs.SetInt(key, best);
        PlayerPrefs.Save();

        optionLabels[0].transform.parent.gameObject.SetActive(false);
        optionLabels[2].transform.parent.gameObject.SetActive(false);
        optionLabels[1].text = "Retry";

        stage.Say("Finished! Score " + score + " / " + lesson.questions.Length
                  + "  (best " + best + "). Tap Retry to try again.");
        RenderCode(lesson.steps.Length, false);
        codeTitle.text = "PREDICT   DONE";
    }

    // ---------- Code panel ----------

    // current = index of the step being run; earlier steps' lines turn green.
    // upcoming = the step has not run yet (shown with a blue marker).
    private void RenderCode(int current, bool upcoming)
    {
        var done = new HashSet<int>();
        for (int i = 0; i < current && i < lesson.steps.Length; i++) done.Add(lesson.steps[i].line);
        int active = current >= 0 && current < lesson.steps.Length ? lesson.steps[current].line : -1;

        var sb = new StringBuilder();
        for (int i = 0; i < lesson.code.Length; i++)
        {
            string line = lesson.code[i];
            if (i == active)
            {
                if (upcoming)
                    sb.AppendLine("<mark=#4DABF755><color=#FFFFFF>" + line + "</color></mark>");
                else
                    sb.AppendLine("<mark=#FFD54A55><color=#FFFFFF>" + line + "</color></mark>");
            }
            else if (done.Contains(i))
                sb.AppendLine("<color=#6FD6A0>" + line + "</color>");
            else
                sb.AppendLine("<color=#8A93A6>" + line + "</color>");
        }
        codeText.text = sb.ToString();
    }
}