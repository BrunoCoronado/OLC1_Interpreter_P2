using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.analisis
{
    class Gramatica : Grammar 
    {
        public Gramatica() : base (caseSensitive: false)
        {
            //COMENTARIOS
            CommentTerminal comentarioLinea = new CommentTerminal("comentario", ">>", "\n");
            CommentTerminal comentarioMultiLinea = new CommentTerminal("comentario", "<-", "->");

            //TIPOS DE DATOS
            var _int = ToTerm("int");
            RegexBasedTerminal numeroEntero = new RegexBasedTerminal("entero", "[0-9]+");

            var _double = ToTerm("double");
            RegexBasedTerminal numeroDecimal = new RegexBasedTerminal("decimal", "[0-9]+\\.[0-9]+");

            var _bool = ToTerm("bool");
            RegexBasedTerminal valorBooleano = new RegexBasedTerminal("booleano", "verdadero|falso|true|false");

            var _char = ToTerm("char");
            RegexBasedTerminal caracter = new RegexBasedTerminal("caracter", "'(.?)'");

            var _string = ToTerm("string");
            StringLiteral textoEntreComillas = new StringLiteral("textoEntreComillas", "\"");

            //ACEPTACION
            var aceptacion = ToTerm("$");

            //OPERADORES RELACIONALES
            var igualacion = ToTerm("==");
            var diferenciacion = ToTerm("!=");
            var menorQue = ToTerm("<");
            var menorIgual = ToTerm("<=");
            var mayorQue = ToTerm(">");
            var mayorIgual = ToTerm(">=");

            //OPERADORES LOGICOS
            var or = ToTerm("||");
            var and = ToTerm("&&");
            var not = ToTerm("!");

            //OPERADORES ARITMETICOS
            var suma = ToTerm("+");
            var resta = ToTerm("-");
            var multiplicacion = ToTerm("*");
            var division = ToTerm("/");
            var potencia = ToTerm("^");

            //AUMENTO Y DECREMENTO
            var aumento = ToTerm("++");
            var decremento = ToTerm("--");

            //SIMBOLOS DECLARACION Y ANALISIS
            IdentifierTerminal identificador = new IdentifierTerminal("identificador");
            var coma = ToTerm(",");
            var igual = ToTerm("=");
            var puntoYComa = ToTerm(";");

            //ARREGLOS
            var array = ToTerm("array");
            var corcheteAbre = ToTerm("[");
            var corcheteCierra = ToTerm("]");
            var llaveAbre = ToTerm("{");
            var llaveCierra = ToTerm("}");

            //CLASES
            var clase = ToTerm("clase");
            var importar = ToTerm("importar");

            var _new = ToTerm("new");
            var punto = ToTerm(".");
            var parentesisAbre = ToTerm("(");
            var parentesisCierra = ToTerm(")");

            var _override = ToTerm("override");

            //VISIBILIDAD 
            var publico = ToTerm("publico");
            var privado = ToTerm("privado");

            //FUNCIONES
            var _void = ToTerm("void");
            var _return = ToTerm("return");

            //FUNCIONES NATIVAS
            var show = ToTerm("show");

            var _if = ToTerm("if");
            var _else = ToTerm("else");

            var _for = ToTerm("for");

            var repeat = ToTerm("repeat");

            var _while = ToTerm("while");

            var comprobar = ToTerm("comprobar");
            var caso = ToTerm("caso");
            var salir = ToTerm("salir");
            var defecto = ToTerm("defecto");

            var hacer = ToTerm("hacer");
            var mientras = ToTerm("mientras");

            var continuar = ToTerm("continuar");

            var addFigure = ToTerm("addFigure");
            var circle = ToTerm("circle");
            var triangle = ToTerm("triangle");
            var square = ToTerm("square");
            var line = ToTerm("line");

            var figure = ToTerm("figure");

            var main = ToTerm("main");

            //NO TERMINALES
            NonTerminal A = new NonTerminal("A");
            NonTerminal PROGRAMA = new NonTerminal("PROGRAMA");
            NonTerminal SENTENCIA = new NonTerminal("SENTENCIA");
            NonTerminal DECLARACION_CLASE = new NonTerminal("DECLARACION_CLASE");
            NonTerminal IMPORTAR = new NonTerminal("IMPORTAR");

            //PREFERENCIAS
            this.Root = A;
            this.MarkPunctuation("$","{", "}", "clase", "importar");
            this.MarkTransient(A, SENTENCIA);

            //GRAMATICA
            A.Rule = PROGRAMA + aceptacion
                    | aceptacion
            ;

            PROGRAMA.Rule = MakePlusRule(PROGRAMA, SENTENCIA);

            SENTENCIA.Rule = DECLARACION_CLASE
            ;

            DECLARACION_CLASE.Rule = clase + identificador + importar + IMPORTAR + llaveAbre + llaveCierra
                    | clase + identificador + llaveAbre + llaveCierra
            ;

            IMPORTAR.Rule = MakePlusRule(IMPORTAR, coma, identificador);
        }
    }
}
