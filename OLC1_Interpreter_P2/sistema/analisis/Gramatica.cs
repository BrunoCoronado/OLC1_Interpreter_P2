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
            CommentTerminal comentarioLinea = new CommentTerminal("comentarioLinea", ">>", "\n", "\r\n", "\r", "\u2085", "\u2028", "\u2029");
            CommentTerminal comentarioMultiLinea = new CommentTerminal("comentarioMultiLinea", "<-", "->");

            //TIPOS DE DATOS
            var _int = ToTerm("int");
            RegexBasedTerminal numeroEntero = new RegexBasedTerminal("int", "[0-9]+");

            var _double = ToTerm("double");
            RegexBasedTerminal numeroDecimal = new RegexBasedTerminal("double", "[0-9]+\\.[0-9]+");

            var _bool = ToTerm("bool");
            RegexBasedTerminal valorBooleano = new RegexBasedTerminal("bool", "verdadero|falso|true|false");

            var _char = ToTerm("char");
            //RegexBasedTerminal caracter = new RegexBasedTerminal("caracter", "'(.?)'");
            StringLiteral caracter = new StringLiteral("char", "'", StringOptions.IsChar);

            var _string = ToTerm("string");
            StringLiteral cadena = new StringLiteral("string", "\"");

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
            var print = ToTerm("print");

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
            NonTerminal E = new NonTerminal("E");
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
            NonTerminal DECLARACION_ARREGLO = new NonTerminal("DECLARACION_ARREGLO");
            NonTerminal DECLARACION_ASIGNACION_ARREGLO = new NonTerminal("DECLARACION_ASIGNACION_ARREGLO");
            NonTerminal DIMENSIONES_ARREGLO = new NonTerminal("DIMENSIONES_ARREGLO");
            NonTerminal CONTENIDO_ARREGLO = new NonTerminal("CONTENIDO_ARREGLO");
            NonTerminal VALORES_ARREGLO = new NonTerminal("VALORES_ARREGLO");
            NonTerminal DECLARACION_FUNCION_VACIA = new NonTerminal("DECLARACION_FUNCION_VACIA");
            NonTerminal LISTA_PARAMETROS = new NonTerminal("LISTA_PARAMETROS");
            NonTerminal LISTA_PARAMETROS_LLAMADA = new NonTerminal("LISTA_PARAMETROS_LLAMADA");
            NonTerminal PARAMETRO_FUNCION = new NonTerminal("PARAMETRO_FUNCION");
            NonTerminal SENTENCIAS_FUNCION_SIN_RETORNO = new NonTerminal("SENTENCIAS_FUNCION_SIN_RETORNO");
            NonTerminal SENTENCIA_FUNCION_SIN_RETORNO = new NonTerminal("SENTENCIA_FUNCION_SIN_RETORNO");
            NonTerminal METODO_MAIN = new NonTerminal("METODO_MAIN");
            NonTerminal SENTENCIAS_MAIN = new NonTerminal("SENTENCIAS_MAIN");
            NonTerminal SENTENCIA_MAIN = new NonTerminal("SENTENCIA_MAIN");
            NonTerminal FUNCIONES_NATIVAS = new NonTerminal("FUNCIONES_NATIVAS");
            NonTerminal FUNCION_NATIVA_PRINT = new NonTerminal("FUNCION_NATIVA_PRINT");
            NonTerminal FUNCION_NATIVA_SHOW = new NonTerminal("FUNCION_NATIVA_SHOW");
            NonTerminal FUNCION_LOCAL = new NonTerminal("FUNCION_LOCAL"); 

            //PREFERENCIAS
            this.Root = A;
            this.NonGrammarTerminals.Add(comentarioLinea);
            this.NonGrammarTerminals.Add(comentarioMultiLinea);
            this.MarkPunctuation("$","{", "}", ";", "," , "=", "]", "[", "(", ")", "clase", "importar", "array", "void", "main", "print", "show");
            this.MarkTransient(A, SENTENCIA, SENTENCIA_CLASE, TIPO_DATO, CONTENIDO_ARREGLO, SENTENCIA_MAIN, FUNCIONES_NATIVAS, SENTENCIA_FUNCION_SIN_RETORNO);
            this.RegisterOperators(1, Associativity.Left, or);
            this.RegisterOperators(2, Associativity.Left, and);
            this.RegisterOperators(3, Associativity.Left, not);
            this.RegisterOperators(4, Associativity.Left, igualacion, diferenciacion, menorQue, menorIgual, mayorQue, mayorIgual);
            this.RegisterOperators(5, Associativity.Left, suma, resta);
            this.RegisterOperators(6, Associativity.Left, multiplicacion, division);
            this.RegisterOperators(7, Associativity.Left, potencia);

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

            SENTENCIA_CLASE.Rule = METODO_MAIN
                    | DECLARACION_VARIABLE
                    | ASIGNACION_VARIABLE
                    | DECLARACION_ASIGNACION_VARIABLE
                    | DECLARACION_ARREGLO
                    | DECLARACION_ASIGNACION_ARREGLO
                    | DECLARACION_FUNCION_VACIA
            ;

            DECLARACION_VARIABLE.Rule = TIPO_DATO + L_IDENTIFICADOR + puntoYComa
                    | visibilidad + TIPO_DATO + L_IDENTIFICADOR + puntoYComa
            ;

            TIPO_DATO.Rule = _int | _double | _bool | _char | _string;

            L_IDENTIFICADOR.Rule = MakePlusRule(L_IDENTIFICADOR,  coma, identificador);

            ASIGNACION_VARIABLE.Rule = identificador + igual + E + puntoYComa;

            DECLARACION_ASIGNACION_VARIABLE.Rule = TIPO_DATO + L_IDENTIFICADOR + igual + E + puntoYComa
                    | visibilidad + TIPO_DATO + L_IDENTIFICADOR + igual + E + puntoYComa

            ;
            
            DECLARACION_ARREGLO.Rule = TIPO_DATO + array + L_IDENTIFICADOR + DIMENSIONES_ARREGLO + puntoYComa
                    | visibilidad + TIPO_DATO + array + L_IDENTIFICADOR + DIMENSIONES_ARREGLO + puntoYComa
            ;

            DECLARACION_ASIGNACION_ARREGLO.Rule = TIPO_DATO + array + L_IDENTIFICADOR + DIMENSIONES_ARREGLO + igual + CONTENIDO_ARREGLO + puntoYComa
                    | visibilidad + TIPO_DATO + array + L_IDENTIFICADOR + DIMENSIONES_ARREGLO + igual + CONTENIDO_ARREGLO + puntoYComa
            ;
            
            DIMENSIONES_ARREGLO.Rule = corcheteAbre + E + corcheteCierra
                    | corcheteAbre + E + corcheteCierra + corcheteAbre + E + corcheteCierra
                    | corcheteAbre + E + corcheteCierra + corcheteAbre + E + corcheteCierra + corcheteAbre + E + corcheteCierra
            ;

            CONTENIDO_ARREGLO.Rule = llaveAbre + VALORES_ARREGLO + llaveCierra
                    |llaveAbre + CONTENIDO_ARREGLO + llaveCierra    
            ;
            
            VALORES_ARREGLO.Rule = MakePlusRule(VALORES_ARREGLO, coma, E)
                    | MakePlusRule(VALORES_ARREGLO, coma, CONTENIDO_ARREGLO)    
            ;

            DECLARACION_FUNCION_VACIA.Rule = identificador + _void + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_SIN_RETORNO + llaveCierra
                    | identificador + _void + _override + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_SIN_RETORNO + llaveCierra
                    | visibilidad + identificador + _void + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_SIN_RETORNO + llaveCierra
                    | visibilidad + identificador + _void + _override + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_SIN_RETORNO + llaveCierra
            ;

            LISTA_PARAMETROS.Rule = MakeStarRule(LISTA_PARAMETROS, coma, PARAMETRO_FUNCION);

            LISTA_PARAMETROS_LLAMADA.Rule = MakeStarRule(LISTA_PARAMETROS_LLAMADA, coma, E);

            PARAMETRO_FUNCION.Rule = TIPO_DATO + identificador;

            SENTENCIAS_FUNCION_SIN_RETORNO.Rule = MakeStarRule(SENTENCIAS_FUNCION_SIN_RETORNO, SENTENCIA_FUNCION_SIN_RETORNO);

            SENTENCIA_FUNCION_SIN_RETORNO.Rule = FUNCIONES_NATIVAS
                    | FUNCION_LOCAL
                    | DECLARACION_VARIABLE
                    | ASIGNACION_VARIABLE
                    | DECLARACION_ASIGNACION_VARIABLE
                    | DECLARACION_ARREGLO
                    | DECLARACION_ASIGNACION_ARREGLO
            ;

            METODO_MAIN.Rule = main + parentesisAbre + parentesisCierra + llaveAbre + SENTENCIAS_MAIN + llaveCierra;

            SENTENCIAS_MAIN.Rule = MakeStarRule(SENTENCIAS_MAIN, SENTENCIA_MAIN);

            SENTENCIA_MAIN.Rule = FUNCIONES_NATIVAS
                    | FUNCION_LOCAL
                    | DECLARACION_VARIABLE
                    | ASIGNACION_VARIABLE
                    | DECLARACION_ASIGNACION_VARIABLE
                    | DECLARACION_ARREGLO
                    | DECLARACION_ASIGNACION_ARREGLO
            ;

            FUNCIONES_NATIVAS.Rule = FUNCION_NATIVA_PRINT
                    | FUNCION_NATIVA_SHOW            
            ;

            FUNCION_NATIVA_PRINT.Rule = print + parentesisAbre + E + parentesisCierra + puntoYComa;

            FUNCION_NATIVA_SHOW.Rule = show + parentesisAbre + E + coma + E + parentesisCierra + puntoYComa;

            FUNCION_LOCAL.Rule = identificador + parentesisAbre + LISTA_PARAMETROS_LLAMADA + parentesisCierra + puntoYComa;

            E.Rule = E + suma + E
                    | E + resta + E
                    | E + division + E
                    | E + multiplicacion + E
                    | E + potencia + E
                    | resta + E
                    | E + or + E
                    | E + and + E
                    | not + E
                    | E + igualacion + E
                    | E + diferenciacion + E
                    | E + menorQue + E
                    | E + menorIgual + E
                    | E + mayorQue + E
                    | E + mayorIgual + E
                    | parentesisAbre + E + parentesisCierra
                    | numeroEntero
                    | numeroDecimal
                    | valorBooleano
                    | caracter
                    | cadena
                    | identificador
                    //| identificador + corcheteAbre + E + corcheteCierra
            ;
        }
    }
}
