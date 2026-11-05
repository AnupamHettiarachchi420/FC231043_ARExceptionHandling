using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class QuizAttempt
{
    public string code;
    public string type;    // "pre" or "post"
    public int score;
    public int total;
    public string date;
}

[System.Serializable]
public class QuizAttemptList
{
    public List<QuizAttempt> attempts = new();
}

public static class QuizStore
{
    private const string Key = "ExceptionExplorerQuizResults";

    public static QuizAttemptList Load()
    {
        string json = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrEmpty(json)) return new QuizAttemptList();
        QuizAttemptList data = JsonUtility.FromJson<QuizAttemptList>(json);
        return data ?? new QuizAttemptList();
    }

    public static void Add(string code, string type, int score, int total)
    {
        QuizAttemptList data = Load();
        data.attempts.Add(new QuizAttempt
        {
            code = code,
            type = type,
            score = score,
            total = total,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        });
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }

    // The latest attempt for a code and test type, or null
    public static QuizAttempt Latest(QuizAttemptList data, string code, string type)
    {
        QuizAttempt found = null;
        foreach (QuizAttempt a in data.attempts)
            if (a.code == code && a.type == type) found = a;
        return found;
    }

    public static string BuildSummary()
    {
        QuizAttemptList data = Load();
        if (data.attempts.Count == 0) return "No results yet.";

        var codes = new List<string>();
        foreach (QuizAttempt a in data.attempts)
            if (!codes.Contains(a.code)) codes.Add(a.code);

        var sb = new StringBuilder();
        sb.AppendLine("<mspace=0.6em>CODE    PRE  POST  GAIN");

        float sumPre = 0f, sumPost = 0f;
        int both = 0, total = 10, shown = 0;

        foreach (string code in codes)
        {
            QuizAttempt pre = Latest(data, code, "pre");
            QuizAttempt post = Latest(data, code, "post");
            string preText = pre != null ? pre.score.ToString() : "-";
            string postText = post != null ? post.score.ToString() : "-";
            string gainText = "-";

            if (pre != null && post != null)
            {
                int gain = post.score - pre.score;
                gainText = (gain >= 0 ? "+" : "") + gain;
                sumPre += pre.score;
                sumPost += post.score;
                total = pre.total;
                both++;
            }

            if (shown < 14)
                sb.AppendLine(code.PadRight(7) + preText.PadLeft(3) + "  " + postText.PadLeft(4) + "  " + gainText.PadLeft(4));
            shown++;
        }
        sb.AppendLine("</mspace>");
        if (shown > 14) sb.AppendLine("(" + (shown - 14) + " more in the CSV)");

        sb.AppendLine("Testers with both tests: " + both);
        if (both > 0)
        {
            float avgPre = sumPre / both;
            float avgPost = sumPost / both;
            sb.AppendLine("Average pre: " + avgPre.ToString("0.0") + " / " + total);
            sb.AppendLine("Average post: " + avgPost.ToString("0.0") + " / " + total);
            sb.AppendLine("Average gain: " + (avgPost - avgPre).ToString("+0.0;-0.0;0.0"));
            if (avgPre > 0f)
                sb.AppendLine("Improvement: " + ((avgPost - avgPre) / avgPre * 100f).ToString("+0;-0;0") + "%");
        }
        return sb.ToString();
    }

    public static string BuildCsv()
    {
        var sb = new StringBuilder("code,type,score,total,date\n");
        foreach (QuizAttempt a in Load().attempts)
            sb.AppendLine(a.code + "," + a.type + "," + a.score + "," + a.total + "," + a.date);
        return sb.ToString();
    }
}