using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




using System.Text.RegularExpressions;


public class Lexeme {
    public enum Type { identifier, keyword, separator, op, literal, comment,
                        dialogName, dialogText};
    public Type type;
    public string data;
    public Lexeme(Type type, string data) {
        this.type = type;
        this.data = data;
    }

    override public string ToString() {
        return this.type + " " + this.data;
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


    
    static string whit = "[\r\n\t\f\v ]";
                                   //  start of line, an amount of white space, ", some stuff, "
                                   //  ^[\r\n\t\f\v ]*^".*"
    static Regex strRegex = new Regex("^" + whit + "*\".*\"", RegexOptions.Compiled);
    static Regex opsRegex = new Regex("^" + whit + "(\\+(\\?!(=|\\+))|\\+\\+|-|\\*|/|==|\\+=|>=|<=|>(?!=)|<(?!=)|=(?!=))", RegexOptions.Compiled);

    static Regex controlLine = new Regex("^" + whit + "\\*", RegexOptions.Compiled);

    static Regex dialogName = new Regex("^" + whit + ".*(?=:)", RegexOptions.Compiled);
    static Regex dialogText = new Regex("^" + whit + ":" + ".*$", RegexOptions.Compiled);

    static Dictionary<Regex, Lexeme.Type> controlDefs = new Dictionary<Regex, Lexeme.Type>()
                { { strRegex, Lexeme.Type.literal},
                  { opsRegex, Lexeme.Type.op} };


   public Lexeme nextControlLexeme(string line) {
        foreach (Regex langPattern in controlDefs.Keys) {
            if (langPattern.IsMatch(line)) {
                return new Lexeme(controlDefs[langPattern], langPattern.Match(line).Value);
            }
        }
        return null;
    }

    public List<Lexeme> tokenizeDialogLine(string line) {
        List<Lexeme> lexemes = new List<Lexeme>();
        lexemes.Add(new Lexeme(Lexeme.Type.dialogName, dialogName.Match(line).Value));
        lexemes.Add(new Lexeme(Lexeme.Type.dialogText, dialogText.Match(line).Value)); // still need to remove colon
        return lexemes;
    }



    public List<Lexeme> tokenizeLine(string line) {
        List<Lexeme> lexemes = new List<Lexeme>();
        if (controlLine.IsMatch(line)) { // if is a control line
            while (line != "") {
                Lexeme next = nextControlLexeme(line);
                lexemes.Add(next);
                line = line.Substring(lexemes[lexemes.Count() - 1].data.Length);
            }
        } else { // is a dialog line
            lexemes.AddRange(tokenizeDialogLine(line));
            line = line.Substring(lexemes[lexemes.Count() - 1].data.Length +
                                    lexemes[lexemes.Count() - 2].data.Length);
        }
        

        return lexemes;
    }
}


namespace DiaLn {
    class Program {
        static void Main(string[] args) {
            Interpreter interp = new Interpreter();
            Console.WriteLine(interp.tokenizeLine(" \"hello\""));
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
