using System.Collections.Generic;

public static class StarterQuestions
{
    private static QuizQuestion Q(string id, string set, int objective,
        string text, string[] options, string explanation)
    {
        return new QuizQuestion
        {
            id = id,
            set = set,
            objective = objective,
            text = text,
            options = options,
            correct = 0,
            explanation = explanation
        };
    }

    public static List<QuizQuestion> Create()
    {
        return new List<QuizQuestion>
        {
            // ---------- SET A (pre-test) ----------
            Q("A1", "A", 2,
              "What is printed?\n\ntry { print(\"1\"); int x = 5 / 0; print(\"2\"); }\ncatch (ArithmeticException e) { print(\"3\"); }\nprint(\"4\");",
              new[] { "1 3 4", "1 2 3 4", "1 3", "1 2 4" },
              "5 / 0 throws an exception, so print(\"2\") is skipped. The catch prints 3 and the program then continues with 4."),

            Q("A2", "A", 2,
              "What is printed?\n\ntry { print(\"A\"); print(\"B\"); }\ncatch (Exception e) { print(\"C\"); }\nfinally { print(\"D\"); }",
              new[] { "A B D", "A B", "A B C D", "A D" },
              "No exception is thrown, so the catch is skipped. The finally block always runs."),

            Q("A3", "A", 1,
              "An exception is thrown part-way through a try block that has a matching catch and a finally block. Which parts run, and in what order?",
              new[] { "The try block up to the error, then the catch, then the finally", "The whole try block, then the catch, then the finally", "Only the catch block", "The try block up to the error, then only the finally" },
              "The try block stops at the error. The matching catch handles it, and finally runs last."),

            Q("A4", "A", 1,
              "What is printed?\n\nvoid f() {\n  try { throw new ArithmeticException(); }\n  catch (NullPointerException e) { print(\"N\"); }\n  finally { print(\"F\"); }\n}\n\nmain: try { f(); } catch (Exception e) { print(\"M\"); }",
              new[] { "F M", "N F M", "M", "F" },
              "The catch in f does not match, so the exception leaves f, but f's finally still runs first (F). Then main's catch handles it (M)."),

            Q("A5", "A", 3,
              "main calls a(), a() calls b(), and b() throws an exception. Only main has a matching catch. What happens to the frames of b() and a()?",
              new[] { "They are discarded one by one as the exception moves up to main", "They keep running after the exception", "Only b() is discarded and a() carries on", "They stay on the stack until main finishes" },
              "An unhandled exception unwinds the call stack: each method without a matching catch is discarded until a handler is found."),

            Q("A6", "A", 3,
              "An ArithmeticException is thrown and no method in the call chain has a catch for it. What happens?",
              new[] { "The program ends with an error message", "The exception is ignored and the program carries on", "Execution continues on the line after the throw", "The last method called catches it automatically" },
              "If no method handles the exception, it leaves the top of the call stack and the program terminates."),

            Q("A7", "A", 4,
              "What is printed?\n\ntry { String s = null; s.length(); }\ncatch (ArithmeticException e) { print(\"1\"); }\ncatch (NullPointerException e) { print(\"2\"); }\ncatch (Exception e) { print(\"3\"); }",
              new[] { "2", "1", "3", "2 3" },
              "Using a null reference throws a NullPointerException. The first matching catch (the second one) runs, and the remaining catches are skipped."),

            Q("A8", "A", 4,
              "This code does not compile:\n\ntry { ... }\ncatch (Exception e) { ... }\ncatch (ArithmeticException e) { ... }\n\nWhat is the correct fix?",
              new[] { "Put the ArithmeticException catch before the Exception catch", "Remove the try block", "Put the finally block first", "Change both catches to catch (Exception e)" },
              "A general catch placed first catches everything, so the specific catch below it could never run. Specific catches must come first."),

            Q("A9", "A", 5,
              "A method readConfig() fails when a file is missing. main() can show the user a clear message, but readConfig() cannot. Where should the exception be caught?",
              new[] { "In main(), which can show the message", "Inside readConfig(), by ignoring it silently", "Nowhere, so the program crashes", "In every method in the chain" },
              "Catch an exception where you can do something useful about it, not necessarily where it happens."),

            Q("A10", "A", 5,
              "Why is an empty catch block, such as catch (Exception e) { }, usually a bad idea?",
              new[] { "It hides the error, so the problem goes unnoticed", "It makes the program run slower", "It stops the finally block from running", "The compiler does not allow it" },
              "An empty catch swallows the error silently, which makes bugs very hard to find."),

            Q("A11", "A", 2,
              "What is printed?\n\nvoid f() { print(\"1\"); int x = 1 / 0; print(\"2\"); }\n\nmain: try { f(); print(\"3\"); }\ncatch (ArithmeticException e) { print(\"4\"); }\nprint(\"5\");",
              new[] { "1 4 5", "1 2 3 4 5", "1 3 5", "1 4" },
              "f prints 1, then 1 / 0 throws. The exception leaves f, skips print(\"3\") in the try block, and main's catch prints 4. The program then continues with 5."),

            Q("A12", "A", 1,
              "What is printed?\n\ntry { print(\"A\"); throw new RuntimeException(); }\ncatch (RuntimeException e) { print(\"B\"); }\nfinally { print(\"C\"); }\nprint(\"D\");",
              new[] { "A B C D", "A C B D", "A B D", "A B C" },
              "try runs up to the throw (A), the catch handles it (B), finally always runs next (C), then the program continues (D)."),

            // ---------- SET B (post-test) ----------
            Q("B1", "B", 2,
              "What is printed?\n\ntry { print(\"X\"); int v = 8 / 0; print(\"Y\"); }\ncatch (ArithmeticException e) { print(\"Z\"); }\nprint(\"W\");",
              new[] { "X Z W", "X Y Z W", "X Z", "X Y W" },
              "8 / 0 throws an exception, so print(\"Y\") is skipped. The catch prints Z and the program then continues with W."),

            Q("B2", "B", 2,
              "What is printed?\n\ntry { print(\"P\"); print(\"Q\"); }\ncatch (Exception e) { print(\"R\"); }\nfinally { print(\"S\"); }",
              new[] { "P Q S", "P Q", "P Q R S", "P S" },
              "No exception is thrown, so the catch is skipped. The finally block always runs."),

            Q("B3", "B", 1,
              "A statement inside a try block throws an exception. The try has a matching catch and a finally block. What is the order of execution?",
              new[] { "Try (up to the error), catch, finally", "Try (all of it), catch, finally", "Catch only", "Try (up to the error), finally only" },
              "The try block stops at the error. The matching catch handles it, and finally runs last."),

            Q("B4", "B", 1,
              "What is printed?\n\nvoid g() {\n  try { throw new ArithmeticException(); }\n  catch (IllegalStateException e) { print(\"S\"); }\n  finally { print(\"T\"); }\n}\n\nmain: try { g(); } catch (Exception e) { print(\"U\"); }",
              new[] { "T U", "S T U", "U", "T" },
              "The catch in g does not match, so the exception leaves g, but g's finally still runs first (T). Then main's catch handles it (U)."),

            Q("B5", "B", 3,
              "main calls x(), x() calls y(), and y() throws an exception. Only main has a matching catch. What happens to the frames of y() and x()?",
              new[] { "They are discarded one by one as the exception moves up to main", "They keep running after the exception", "Only y() is discarded and x() carries on", "They stay on the stack until main finishes" },
              "An unhandled exception unwinds the call stack: each method without a matching catch is discarded until a handler is found."),

            Q("B6", "B", 3,
              "An exception is thrown and no method in the call chain has a matching catch for it. What happens to the program?",
              new[] { "The program ends with an error message", "The exception is ignored and the program carries on", "Execution continues on the line after the throw", "The first method called catches it automatically" },
              "If no method handles the exception, it leaves the top of the call stack and the program terminates."),

            Q("B7", "B", 4,
              "What is printed?\n\ntry { Integer.parseInt(\"abc\"); }\ncatch (NullPointerException e) { print(\"1\"); }\ncatch (ArithmeticException e) { print(\"2\"); }\ncatch (Exception e) { print(\"3\"); }",
              new[] { "3", "1", "2", "2 3" },
              "Parsing \"abc\" throws a NumberFormatException. It matches neither of the first two catches, so the general catch (Exception) handles it."),

            Q("B8", "B", 4,
              "This code does not compile:\n\ntry { ... }\ncatch (Throwable t) { ... }\ncatch (NullPointerException e) { ... }\n\nWhat is the correct fix?",
              new[] { "Put the NullPointerException catch before the Throwable catch", "Delete the try block", "Add a finally block after both catches", "Change the NullPointerException catch to catch (Throwable t)" },
              "A general catch placed first catches everything, so the specific catch below it could never run. Specific catches must come first."),

            Q("B9", "B", 5,
              "A method saveOrder() fails when the database is offline. The checkout screen code can tell the user to try again, but saveOrder() cannot. Where should the exception be caught?",
              new[] { "In the checkout code, which can tell the user", "Inside saveOrder(), by ignoring it silently", "Nowhere, so the program crashes", "In every method in the chain" },
              "Catch an exception where you can do something useful about it, not necessarily where it happens."),

            Q("B10", "B", 5,
              "What is the main danger of writing catch (Exception e) { } with nothing inside it?",
              new[] { "The error is hidden and nobody notices the problem", "The program always crashes anyway", "The finally block is skipped", "The code will not compile" },
              "An empty catch swallows the error silently, which makes bugs very hard to find."),

            Q("B11", "B", 2,
              "What is printed?\n\nvoid g() { print(\"a\"); int x = 2 / 0; print(\"b\"); }\n\nmain: try { g(); print(\"c\"); }\ncatch (ArithmeticException e) { print(\"d\"); }\nprint(\"e\");",
              new[] { "a d e", "a b c d e", "a c e", "a d" },
              "g prints a, then 2 / 0 throws. The exception leaves g, skips print(\"c\") in the try block, and main's catch prints d. The program then continues with e."),

            Q("B12", "B", 1,
              "What is printed?\n\ntry { print(\"K\"); throw new RuntimeException(); }\ncatch (RuntimeException e) { print(\"L\"); }\nfinally { print(\"M\"); }\nprint(\"N\");",
              new[] { "K L M N", "K M L N", "K L N", "K L M" },
              "try runs up to the throw (K), the catch handles it (L), finally always runs next (M), then the program continues (N).")
        };
    }
}