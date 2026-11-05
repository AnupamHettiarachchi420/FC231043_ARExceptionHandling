using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;

public class QuizController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private QuestionBank bank;
    [SerializeField] private int questionsPerTest = 10;

    [Header("Panels")]
    [SerializeField] private GameObject setupPanel;
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject resultsPanel;

    [Header("Setup")]
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text setupMessage;

    [Header("Question")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button[] optionButtons;   // 4 buttons
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextLabel;

    [Header("Results")]
    [SerializeField] private TMP_Text resultsTitle;
    [SerializeField] private TMP_Text resultsText;
    [SerializeField] private TMP_Text resultsNote;

    private List<QuizQuestion> test;
    private int[] order;
    private Color[] baseColors;
    private string code;
    private string set;
    private int index;
    private int score;
    private bool answered;
    private bool confirmClear;

    private void Awake()
    {
        baseColors = new Color[optionButtons.Length];
        for (int i = 0; i < optionButtons.Length; i++)
            baseColors[i] = optionButtons[i].GetComponent<Image>().color;
    }

    private void Start() { ShowSetup(); }

    // ---------- Screens ----------

    public void ShowSetup()
    {
        setupPanel.SetActive(true);
        questionPanel.SetActive(false);
        resultsPanel.SetActive(false);
        setupMessage.text = "";
        confirmClear = false;
    }

    public void ViewResults()
    {
        setupPanel.SetActive(false);
        questionPanel.SetActive(false);
        resultsPanel.SetActive(true);
        resultsTitle.text = "Results";
        resultsText.text = QuizStore.BuildSummary();
        resultsNote.text = "";
        confirmClear = false;
    }

    // ---------- Starting a test ----------

    public void StartPre() { StartTest("A"); }
    public void StartPost() { StartTest("B"); }

    private void StartTest(string testSet)
    {
        string entered = codeInput.text.Trim().ToUpperInvariant();
        if (!IsValidCode(entered))
        {
            setupMessage.text = "Enter an anonymous code of 1 to 6 letters or numbers, for example T01.";
            return;
        }

        List<QuizQuestion> pool = bank.GetSet(testSet);
        if (pool.Count == 0)
        {
            setupMessage.text = "No questions were found in the question bank.";
            return;
        }

        code = entered;
        set = testSet;
        test = new List<QuizQuestion>(pool.GetRange(0, Mathf.Min(questionsPerTest, pool.Count)));
        Shuffle(test);
        index = 0;
        score = 0;

        setupPanel.SetActive(false);
        resultsPanel.SetActive(false);
        questionPanel.SetActive(true);
        ShowQuestion();
    }

    private static bool IsValidCode(string value)
    {
        if (value.Length < 1 || value.Length > 6) return false;
        foreach (char c in value)
            if (!char.IsLetterOrDigit(c)) return false;
        return true;
    }

    // ---------- Questions ----------

    private void ShowQuestion()
    {
        QuizQuestion q = test[index];
        answered = false;

        var indices = new List<int>();
        for (int i = 0; i < q.options.Length; i++)
            if (!string.IsNullOrEmpty(q.options[i])) indices.Add(i);
        Shuffle(indices);
        order = indices.ToArray();

        headerText.text = (set == "A" ? "Pre-test" : "Post-test") + "   " + (index + 1) + " / " + test.Count;
        questionText.text = q.text;
        feedbackText.text = "";
        nextButton.gameObject.SetActive(false);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool used = i < order.Length;
            optionButtons[i].gameObject.SetActive(used);
            if (!used) continue;
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = q.options[order[i]];
            optionButtons[i].GetComponent<Image>().color = baseColors[i];
        }
    }

    // Called by the four answer buttons (0 to 3)
    public void Answer(int button)
    {
        if (answered || button >= order.Length) return;
        answered = true;

        QuizQuestion q = test[index];
        bool right = order[button] == q.correct;
        if (right) score++;

        for (int i = 0; i < order.Length; i++)
        {
            Image img = optionButtons[i].GetComponent<Image>();
            if (order[i] == q.correct) img.color = new Color(0.30f, 0.67f, 0.97f);   // blue = correct answer
            else if (i == button) img.color = new Color(1f, 0.62f, 0.26f);            // orange = your wrong choice
            else img.color = new Color(0.55f, 0.60f, 0.70f);                          // grey = others
        }

        feedbackText.text = (right
            ? "<color=#7CC7FF>Correct!</color> "
            : "<color=#FFB066>Not quite.</color> ") + q.explanation;
        nextLabel.text = index + 1 >= test.Count ? "Finish" : "Next";
        nextButton.gameObject.SetActive(true);
    }

    public void Next()
    {
        if (!answered) return;
        index++;
        if (index >= test.Count) Finish(); else ShowQuestion();
    }

    // ---------- Finishing ----------

    private void Finish()
    {
        string type = set == "A" ? "pre" : "post";
        QuizStore.Add(code, type, score, test.Count);

        questionPanel.SetActive(false);
        resultsPanel.SetActive(true);
        resultsTitle.text = (set == "A" ? "Pre-test" : "Post-test") + " complete";

        string text = "Code: " + code + "\nScore: " + score + " / " + test.Count + "\n\n";
        if (set == "A")
        {
            text += "Now work through the lessons, then take the post-test with the same code.";
        }
        else
        {
            QuizAttempt pre = QuizStore.Latest(QuizStore.Load(), code, "pre");
            if (pre != null)
            {
                int gain = score - pre.score;
                text += "Pre-test: " + pre.score + " / " + pre.total + "\nGain: " + (gain >= 0 ? "+" : "") + gain;
            }
            else
            {
                text += "No pre-test was found for this code.";
            }
        }
        resultsText.text = text;
        resultsNote.text = "";
        confirmClear = false;
    }

    // ---------- Results tools ----------

    public void CopyCsv()
    {
        GUIUtility.systemCopyBuffer = QuizStore.BuildCsv();
        resultsNote.text = "Copied. Paste it into a note or a message to yourself.";
        confirmClear = false;
    }

    public void ClearResults()
    {
        if (!confirmClear)
        {
            confirmClear = true;
            resultsNote.text = "Tap Clear again to delete ALL stored results.";
            return;
        }
        QuizStore.ClearAll();
        confirmClear = false;
        resultsText.text = QuizStore.BuildSummary();
        resultsNote.text = "All results were deleted.";
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}