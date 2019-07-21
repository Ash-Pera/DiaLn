using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




using System.Text.RegularExpressions;


public class Lexeme {
    public enum Type { identifier, keyword, separator, op, literal, comment,
                        dialogName, dialogText,
                        UNKNOWN};
    public Type type;
    public string data;
    public Lexeme(Type type, string data) {
        this.type = type;
        this.data = data;
    }

    override public string ToString() {
        return this.type + ": " + this.data;
    }
}



public class Interpreter {
    Dictionary<string, int> ints;

    public Interpreter() {
        ints = new Dictionary<string, int>();
    }

    bool intExists(string intName) {
        return ints.ContainsKey(intName);
    }

    //not safe
    int getInt(string intName) {
        return ints[intName];
    }

    //not safe
    void setInt(string intName, int intValue) {
        ints[intName] = intValue;
    }




    bool declareInt(string intName) {
        if (intExists(intName)) {
            return false;
        } else {
            setInt(intName, 0);
            return true;
        }
    }


    bool defineInt(string intName, int intValue) {
        if (intExists(intName)) {
            setInt(intName, intValue);
            return true;
        } else {
            return false;
        }
    }



    uint howManyTabsAtStart(string str) {
        int offset = str.TakeWhile(c => char.IsWhiteSpace(c)).Count();
        return (uint)str.Substring(0, offset).Count(ch => ch == '\t');
    }

    // const imples static
    const string plus = "\\+(?![+|=])";
    const string minus = "-(?![-])";
    const string div = "/";
    const string times = "\\*";
    const string plusplus = "\\+\\+";
    const string minusminus = "--";

    const string eq = "=(?!=)";
    const string pluseq = "\\+=";
    const string minuseq = "-=";
    const string timeseq = "\\*=";
    const string diveq = "/=";

    const string eqeq = "==";
    const string gtreq = ">=";
    const string lesseq = "<=";
    const string gtr = ">(?!=)";
    const string less = "<(?!=)";

    const string inv = "\\!";

    static readonly string[] opstrings = { plus, minus , div, times, plusplus, minusminus,
                                           eq, pluseq, minuseq, timeseq, diveq,
                                           eqeq, gtreq, lesseq, less, gtr, inv};




    const string whit = "[\r\n\t\f\v ]*";
                                   //  start of line, an amount of white space, ", some stuff, "
                                   //  ^[\r\n\t\f\v ]*^".*"
    static Regex strRegex = new Regex("^" + whit + "\".*\"", RegexOptions.Compiled);
    static Regex opsRegex = new Regex("^" + whit + "(" + String.Join("|", opstrings) + ")", RegexOptions.Compiled);

    static Regex paranRegex = new Regex("^" + whit + "(\\(|\\)|\\[|\\]|\\{|\\})", RegexOptions.Compiled);


    static Regex controlLine = new Regex("^" + whit + "\\*", RegexOptions.Compiled);

    static Regex dialogName = new Regex("^" + whit + ".*?(?=:)", RegexOptions.Compiled);
    static Regex dialogText = new Regex("^" + whit + ":" + ".*$", RegexOptions.Compiled);

    static Dictionary<Regex, Lexeme.Type> controlDefs = new Dictionary<Regex, Lexeme.Type>()
                { { strRegex, Lexeme.Type.literal},
                  { opsRegex, Lexeme.Type.op},
                  {paranRegex, Lexeme.Type.separator } };


   public Lexeme nextControlLexeme(string line) {
        foreach (Regex langPattern in controlDefs.Keys) {
            if (langPattern.IsMatch(line)) {
                return new Lexeme(controlDefs[langPattern], langPattern.Match(line).Value);
            }
        }
        return new Lexeme(Lexeme.Type.UNKNOWN, line);
    }

    public List<Lexeme> tokenizeDialogLine(string line) {
        List<Lexeme> lexemes = new List<Lexeme>();

        string speaker = dialogName.Match(line).Value;
        lexemes.Add(new Lexeme(Lexeme.Type.dialogName, speaker));

        //rest of line should be dialog text
        lexemes.Add(new Lexeme(Lexeme.Type.dialogText, line.Substring(speaker.Length + 1).TrimStart()));
        return lexemes;
    }

    public List<Lexeme> tokenizeControlLine(string line) {
        List<Lexeme> lexemes = new List<Lexeme>();
        string controlIndicator = controlLine.Match(line).Value;

        line = line.Substring(controlIndicator.Length);

         while (line != "") {
             Lexeme next = nextControlLexeme(line);
             lexemes.Add(next);
             line = line.Substring(lexemes.Last().data.Length);
         }

        return lexemes;

    }

    
    public List<Lexeme> tokenizeLine(string line) {
        List<Lexeme> lexemes = new List<Lexeme>();
        if (controlLine.IsMatch(line)) { // if is a control line
            lexemes.AddRange(tokenizeControlLine(line));
        } else { // is a dialog line
            lexemes.AddRange(tokenizeDialogLine(line));
        }
        

        return lexemes;
    }


    public List<Lexeme> tokenize(List<string> lines) {
        List<Lexeme> lexemes = new List<Lexeme>();
        foreach(string line in lines) {
            lexemes.AddRange(tokenizeLine(line));
        }
        return lexemes;
    }
}


namespace DiaLn {
    class Program {
        static void Main(string[] args) {
            Interpreter interp = new Interpreter();

            var testText = new List<string> { "Ash: Hello, world ",
                                              "Computer: Hello Ash! ily :)",
                                              "*++(+)++"};





            var lexed = interp.tokenize(testText);
            lexed.ForEach(Console.WriteLine);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
