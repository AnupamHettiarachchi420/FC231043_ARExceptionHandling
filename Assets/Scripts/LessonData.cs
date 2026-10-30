public enum StepKind { Print, Call, Return, Throw, Unwind, Catch, Finally, Crash, Note }

public enum LessonId
{
    Scene1_NormalFlow,
    Scene2_TryCatch,
    Scene3_Propagation,
    Scene4_CatchOrder,
    Scene5_Finally
}

public class LessonStep
{
    public readonly int line;          // code line to highlight (-1 = none)
    public readonly StepKind kind;
    public readonly string arg;        // frame label, exception type or printed text
    public readonly string message;    // explanation shown to the learner
    public readonly bool hasTry;       // for Call: the new frame contains a try block

    public LessonStep(int line, StepKind kind, string arg, string message, bool hasTry = false)
    {
        this.line = line;
        this.kind = kind;
        this.arg = arg;
        this.message = message;
        this.hasTry = hasTry;
    }
}

public class LessonQuestion
{
    public readonly int beforeStep;    // asked just before this step runs
    public readonly string prompt;
    public readonly string[] options;  // exactly 3
    public readonly int correct;       // index of the correct option

    public LessonQuestion(int beforeStep, string prompt, string[] options, int correct)
    {
        this.beforeStep = beforeStep;
        this.prompt = prompt;
        this.options = options;
        this.correct = correct;
    }
}

public class Lesson
{
    public string title;
    public string rootFrame;
    public bool rootHasTry;
    public string[] code;
    public LessonStep[] steps;
    public LessonQuestion[] questions;
}

public static class Lessons
{
    public static Lesson Get(LessonId id)
    {
        switch (id)
        {
            case LessonId.Scene1_NormalFlow: return NormalFlow();
            case LessonId.Scene2_TryCatch: return TryCatch();
            default: return NormalFlow();   // scenes 2 to 5 are added later
        }
    }

    private static Lesson NormalFlow()
    {
        return new Lesson
        {
            title = "Normal flow versus an error",
            rootFrame = "main()",
            rootHasTry = false,
            code = new[]
            {
                "print(\"Start\");",
                "r = divide(10, 2);",
                "    return a / b;",
                "print(r);",
                "s = divide(10, 0);",
                "    return a / b;",
                "print(\"End\");"
            },
            steps = new[]
            {
                new LessonStep(0, StepKind.Print, "Start",
                    "Normal flow: main runs one statement after another."),
                new LessonStep(1, StepKind.Call, "divide(10, 2)",
                    "main calls divide. A new frame is pushed on top of the call stack."),
                new LessonStep(2, StepKind.Return, "",
                    "10 / 2 = 5. divide returns 5 and its frame is removed."),
                new LessonStep(3, StepKind.Print, "5",
                    "Back in main, the result 5 is printed."),
                new LessonStep(4, StepKind.Call, "divide(10, 0)",
                    "main calls divide again, this time with b = 0."),
                new LessonStep(5, StepKind.Throw, "ArithmeticException",
                    "10 / 0 is illegal, so divide throws an ArithmeticException."),
                new LessonStep(4, StepKind.Unwind, "",
                    "divide has no catch, so its frame is discarded and the exception moves to the caller, main."),
                new LessonStep(-1, StepKind.Crash, "",
                    "main has no catch either. The program crashes, so End is never printed.")
            },
            questions = new[]
            {
                new LessonQuestion(5, "divide(10, 0) is about to run. What happens?",
                    new[] { "It returns 0", "It throws an ArithmeticException", "It returns infinity" }, 1),
                new LessonQuestion(6, "divide has no catch. Where does the exception go next?",
                    new[] { "Back to main, the caller", "It disappears", "Into divide(10, 2)" }, 0),
                new LessonQuestion(7, "main has no catch either. What happens to print(\"End\")?",
                    new[] { "End is printed", "End is never printed", "End is printed twice" }, 1)
            }
        };
    }
    private static Lesson TryCatch()
    {
        return new Lesson
        {
            title = "try and catch",
            rootFrame = "main()",
            rootHasTry = true,
            code = new[]
            {
                "try {",
                "    r = divide(10, 0);",
                "    print(\"OK\");",
                "} catch (ArithmeticException e) {",
                "    print(\"Caught\");",
                "}",
                "print(\"End\");"
            },
            steps = new[]
            {
                new LessonStep(0, StepKind.Note, "",
                    "main enters a try block. If something inside it throws an exception, main can catch it."),
                new LessonStep(1, StepKind.Call, "divide(10, 0)",
                    "Inside the try block, main calls divide with b = 0."),
                new LessonStep(1, StepKind.Throw, "ArithmeticException",
                    "10 / 0 is illegal, so divide throws an ArithmeticException."),
                new LessonStep(1, StepKind.Unwind, "",
                    "divide has no catch, so its frame is discarded and the exception moves to main."),
                new LessonStep(3, StepKind.Catch, "",
                    "main has a catch for ArithmeticException, so the exception is caught and handled."),
                new LessonStep(4, StepKind.Print, "Caught",
                    "The catch block runs and prints Caught. The rest of the try block, print(\"OK\"), was skipped."),
                new LessonStep(6, StepKind.Print, "End",
                    "The program carries on after the try-catch and prints End.")
            },
            questions = new[]
            {
                new LessonQuestion(3, "divide has no catch. Where does the exception go?",
                    new[] { "Back to main, the caller", "It is ignored", "The program stops at once" }, 0),
                new LessonQuestion(5, "The exception was caught. Which line runs next?",
                    new[] { "print(\"OK\")", "print(\"Caught\")", "print(\"End\")" }, 1),
                new LessonQuestion(6, "After the catch block finishes, what happens?",
                    new[] { "The program crashes", "The program continues with print(\"End\")", "The try block runs again" }, 1)
            }
        };
    }
}