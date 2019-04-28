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
            RegexBasedTerminal visibilidad = new RegexBasedTerminal("visibilidad", "publico|privado");

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
            NonTerminal CUERPO_CLASE = new NonTerminal("CUERPO_CLASE");
            NonTerminal SENTENCIA_CLASE = new NonTerminal("SENTENCIA_CLASE");
            NonTerminal DECLARACION_VARIABLE = new NonTerminal("DECLARACION_VARIABLE");
            NonTerminal ASIGNACION_VARIABLE = new NonTerminal("ASIGNACION_VARIABLE");
            NonTerminal DECLARACION_ASIGNACION_VARIABLE = new NonTerminal("DECLARACION_ASIGNACION_VARIABLE");
            NonTerminal TIPO_DATO = new NonTerminal("TIPO_DATO");
            NonTerminal L_IDENTIFICADOR = new NonTerminal("L_IDENTIFICADOR");

            //PREFERENCIAS
            this.Root = A;
            this.MarkPunctuation("$","{", "}", ";" , "clase", "importar");
            this.MarkTransient(A, SENTENCIA, SENTENCIA_CLASE, TIPO_DATO);

            //GRAMATICA
            A.Rule = PROGRAMA + aceptacion
                    | aceptacion
            ;

            PROGRAMA.Rule = MakePlusRule(PROGRAMA, SENTENCIA);

            SENTENCIA.Rule = DECLARACION_CLASE
            ;

            DECLARACION_CLASE.Rule = clase + identificador + importar + IMPORTAR + llaveAbre + llaveCierra
                    | clase + identificador + importar + IMPORTAR + llaveAbre + CUERPO_CLASE + llaveCierra
                    | clase + identificador + llaveAbre + llaveCierra
                    | clase + identificador + llaveAbre + CUERPO_CLASE + llaveCierra

            ;

            IMPORTAR.Rule = MakePlusRule(IMPORTAR, coma, identificador);

            CUERPO_CLASE.Rule = MakePlusRule(CUERPO_CLASE, SENTENCIA_CLASE);

            SENTENCIA_CLASE.Rule = DECLARACION_VARIABLE
                    //|ASIGNACION_VARIABLE
                    //|DECLARACION_ASIGNACION_VARIABLE
            ;

            DECLARACION_VARIABLE.Rule = TIPO_DATO + L_IDENTIFICADOR + puntoYComa
                    | visibilidad + TIPO_DATO + L_IDENTIFICADOR + puntoYComa
            ;

            TIPO_DATO.Rule = _int | _double | _bool | _char | _string;

            L_IDENTIFICADOR.Rule = MakePlusRule(L_IDENTIFICADOR,  coma, identificador);
        }
    }
}
