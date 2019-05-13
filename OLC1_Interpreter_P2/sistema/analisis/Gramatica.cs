using Irony.Parsing;
using OLC1_Interpreter_P2.sistema.bean;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.analisis
{
    class Gramatica : Grammar 
    {
        public ArrayList errores;

        public Gramatica() : base (caseSensitive: false)
        {
            //ERRORES
            errores = new ArrayList();

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
            var _aumento = ToTerm("++");
            var _decremento = ToTerm("--");

            //SIMBOLOS DECLARACION Y ANALISIS
            IdentifierTerminal identificador = new IdentifierTerminal("identificador");
            var coma = ToTerm(",");
            var igual = ToTerm("=");
            var puntoYComa = ToTerm(";");
            var dosPuntos = ToTerm(":");

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
            var _caso = ToTerm("caso");
            var salir = ToTerm("salir");
            var _defecto = ToTerm("defecto");

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
            NonTerminal REASIGNACION_VALOR_ARREGLO = new NonTerminal("REASIGNACION_VALOR_ARREGLO");
            NonTerminal DECLARACION_FUNCION_VACIA = new NonTerminal("DECLARACION_FUNCION_VACIA");
            NonTerminal DECLARACION_FUNCION_RETORNO = new NonTerminal("DECLARACION_FUNCION_RETORNO");
            NonTerminal LISTA_PARAMETROS = new NonTerminal("LISTA_PARAMETROS");
            NonTerminal LISTA_PARAMETROS_LLAMADA = new NonTerminal("LISTA_PARAMETROS_LLAMADA");
            NonTerminal PARAMETRO_FUNCION = new NonTerminal("PARAMETRO_FUNCION");
            NonTerminal SENTENCIAS_FUNCION_SIN_RETORNO = new NonTerminal("SENTENCIAS_FUNCION_SIN_RETORNO");
            NonTerminal SENTENCIA_FUNCION_SIN_RETORNO = new NonTerminal("SENTENCIA_FUNCION_SIN_RETORNO");
            NonTerminal SENTENCIAS_FUNCION_RETORNO = new NonTerminal("SENTENCIAS_FUNCION_RETORNO");
            NonTerminal SENTENCIA_FUNCION_RETORNO = new NonTerminal("SENTENCIA_FUNCION_RETORNO");
            NonTerminal METODO_MAIN = new NonTerminal("METODO_MAIN");
            NonTerminal SENTENCIAS_MAIN = new NonTerminal("SENTENCIAS_MAIN");
            NonTerminal SENTENCIA_MAIN = new NonTerminal("SENTENCIA_MAIN");
            NonTerminal FUNCIONES_NATIVAS = new NonTerminal("FUNCIONES_NATIVAS");
            NonTerminal FUNCION_NATIVA_PRINT = new NonTerminal("FUNCION_NATIVA_PRINT");
            NonTerminal FUNCION_NATIVA_SHOW = new NonTerminal("FUNCION_NATIVA_SHOW");
            NonTerminal FUNCION_NATIVA_WHILE = new NonTerminal("FUNCION_NATIVA_WHILE");
            NonTerminal FUNCION_NATIVA_FOR = new NonTerminal("FUNCION_NATIVA_FOR");
            NonTerminal FUNCION_NATIVA_IF = new NonTerminal("FUNCION_NATIVA_IF");
            NonTerminal CONDIDICIONES_ELSE_IF = new NonTerminal("CONDIDICIONES_ELSE_IF");
            NonTerminal CONDICION_ELSE_IF = new NonTerminal("CONDICION_ELSE_IF");
            NonTerminal CONDICION_IF = new NonTerminal("CONDICION_IF");
            NonTerminal CONDICION_ELSE = new NonTerminal("CONDICION_ELSE");
            NonTerminal FUNCION_LOCAL = new NonTerminal("FUNCION_LOCAL");
            NonTerminal AUMENTO = new NonTerminal("AUMENTO");
            NonTerminal DECREMENTO = new NonTerminal("DECREMENTO");
            NonTerminal AUMENTO_DECREMENTO = new NonTerminal("AUMENTO_DECREMENTO");
            NonTerminal DATOS_AUMENTO_DECREMENTO = new NonTerminal("DATOS_AUMENTO_DECREMENTO");
            NonTerminal SENTENCIAS_BUCLE = new NonTerminal("SENTENCIAS_BUCLE");
            NonTerminal SENTENCIA_BUCLE = new NonTerminal("SENTENCIA_BUCLE");
            NonTerminal SENTENCIA_CONTINUAR = new NonTerminal("SENTENCIA_CONTINUAR");
            NonTerminal SENTENCIA_SALIR = new NonTerminal("SENTENCIA_SALIR");
            NonTerminal DECLARACIONES_FOR = new NonTerminal("DECLARACIONES_FOR");
            NonTerminal ASIGNACION_DECLARACION_FOR = new NonTerminal("ASIGNACION_DECLARACION_FOR");
            NonTerminal ACTUALIZACION_FOR = new NonTerminal("ACTUALIZACION_FOR");
            NonTerminal SENTENCIA_RETURN = new NonTerminal("SENTENCIA_RETURN");
            NonTerminal DECLARACION_OBJETO = new NonTerminal("DECLARACION_OBJETO");
            NonTerminal ASIGNACION_OBJETO = new NonTerminal("ASIGNACION_OBJETO");
            NonTerminal DECLARACION_ASIGNACION_OBJETO = new NonTerminal("DECLARACION_ASIGNACION_OBJETO");
            NonTerminal ACCESO_VARIABLE_OBJETO = new NonTerminal("ACCESO_VARIABLE_OBJETO");
            NonTerminal ACCESO_FUNCION_OBJETO = new NonTerminal("ACCESO_FUNCION_OBJETO");
            NonTerminal REASIGNACION_VARIABLE_OBJETO = new NonTerminal("REASIGNACION_VARIABLE_OBJETO");
            NonTerminal FUNCION_NATIVA_ADDFIGURE = new NonTerminal("FUNCION_NATIVA_ADDFIGURE");
            NonTerminal FIGURAS = new NonTerminal("FIGURAS");
            NonTerminal FUNCION_NATIVA_CIRCLE = new NonTerminal("FUNCION_NATIVA_CIRCLE");
            NonTerminal FUNCION_NATIVA_TRIANGLE = new NonTerminal("FUNCION_NATIVA_TRIANGLE");
            NonTerminal FUNCION_NATIVA_SQUARE = new NonTerminal("FUNCION_NATIVA_SQUARE");
            NonTerminal FUNCION_NATIVA_LINE = new NonTerminal("FUNCION_NATIVA_LINE");
            NonTerminal FUNCION_NATIVA_FIGURE = new NonTerminal("FUNCION_NATIVA_FIGURE");
            NonTerminal FUNCION_NATIVA_REPEAT = new NonTerminal("FUNCION_NATIVA_REPEAT");
            NonTerminal FUNCION_NATIVA_DO_WHILE = new NonTerminal("FUNCION_NATIVA_DO_WHILE");
            NonTerminal FUNCION_NATIVA_COMPROBAR = new NonTerminal("FUNCION_NATIVA_COMPROBAR");
            NonTerminal LISTA_CASOS = new NonTerminal("LISTA_CASOS");
            NonTerminal CASO = new NonTerminal("CASO");
            NonTerminal DEFECTO = new NonTerminal("DEFECTO");


            //PREFERENCIAS
            this.Root = A;
            this.NonGrammarTerminals.Add(comentarioLinea);
            this.NonGrammarTerminals.Add(comentarioMultiLinea);
            this.MarkPunctuation("$","{", "}", ":", ";", ",", "." , "=", "]", "[", "(", ")", "clase", "importar", "array", "void", "main", "print", "show", "while", "for", "if", "else", "return", "new", "addFigure", "circle", "triangle", "square", "line", "figure", "repeat", "hacer", "mientras", "comprobar", "caso", "defecto");
            this.MarkTransient(A, SENTENCIA, SENTENCIA_CLASE, TIPO_DATO, CONTENIDO_ARREGLO, SENTENCIA_MAIN, FUNCIONES_NATIVAS, SENTENCIA_FUNCION_SIN_RETORNO, DATOS_AUMENTO_DECREMENTO, SENTENCIA_BUCLE, SENTENCIA_FUNCION_RETORNO, FIGURAS);
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
                    //| ASIGNACION_VARIABLE
                    | DECLARACION_ASIGNACION_VARIABLE
                    | DECLARACION_ARREGLO
                    | DECLARACION_ASIGNACION_ARREGLO
                    //| REASIGNACION_VALOR_ARREGLO
                    | DECLARACION_FUNCION_VACIA
                    | DECLARACION_FUNCION_RETORNO
                    | DECLARACION_OBJETO
                    //| ASIGNACION_OBJETO
                    | DECLARACION_ASIGNACION_OBJETO

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

            REASIGNACION_VALOR_ARREGLO.Rule = identificador + DIMENSIONES_ARREGLO +  igual + E + puntoYComa;

            DECLARACION_FUNCION_VACIA.Rule = identificador + _void + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_SIN_RETORNO + llaveCierra
                    | identificador + _void + _override + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_SIN_RETORNO + llaveCierra
                    | visibilidad + identificador + _void + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_SIN_RETORNO + llaveCierra
                    | visibilidad + identificador + _void + _override + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_SIN_RETORNO + llaveCierra
            ;

            DECLARACION_FUNCION_RETORNO.Rule = identificador + TIPO_DATO + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_RETORNO + llaveCierra
                    | identificador + TIPO_DATO + _override + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_RETORNO + llaveCierra
                    | visibilidad + identificador + TIPO_DATO + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_RETORNO + llaveCierra
                    | visibilidad + identificador + TIPO_DATO + _override + parentesisAbre + LISTA_PARAMETROS + parentesisCierra + llaveAbre + SENTENCIAS_FUNCION_RETORNO + llaveCierra
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
                    | REASIGNACION_VALOR_ARREGLO
                    | AUMENTO_DECREMENTO
                    | DECLARACION_OBJETO
                    | ASIGNACION_OBJETO
                    | DECLARACION_ASIGNACION_OBJETO
                    | ACCESO_FUNCION_OBJETO
                    | REASIGNACION_VARIABLE_OBJETO
            ;

            SENTENCIAS_FUNCION_RETORNO.Rule = MakeStarRule(SENTENCIAS_FUNCION_RETORNO, SENTENCIA_FUNCION_RETORNO);

            SENTENCIA_FUNCION_RETORNO.Rule = FUNCIONES_NATIVAS
                    | FUNCION_LOCAL
                    | DECLARACION_VARIABLE
                    | ASIGNACION_VARIABLE
                    | DECLARACION_ASIGNACION_VARIABLE
                    | DECLARACION_ARREGLO
                    | DECLARACION_ASIGNACION_ARREGLO
                    | REASIGNACION_VALOR_ARREGLO
                    | AUMENTO_DECREMENTO
                    | SENTENCIA_RETURN
                    | DECLARACION_OBJETO
                    | ASIGNACION_OBJETO
                    | DECLARACION_ASIGNACION_OBJETO
                    | ACCESO_FUNCION_OBJETO
                    | REASIGNACION_VARIABLE_OBJETO
            ;

            SENTENCIA_RETURN.Rule = _return + E + puntoYComa;

            METODO_MAIN.Rule = main + parentesisAbre + parentesisCierra + llaveAbre + SENTENCIAS_MAIN + llaveCierra;

            SENTENCIAS_MAIN.Rule = MakeStarRule(SENTENCIAS_MAIN, SENTENCIA_MAIN);

            SENTENCIA_MAIN.Rule = FUNCIONES_NATIVAS
                    | FUNCION_LOCAL
                    | DECLARACION_VARIABLE
                    | ASIGNACION_VARIABLE
                    | DECLARACION_ASIGNACION_VARIABLE
                    | DECLARACION_ARREGLO
                    | DECLARACION_ASIGNACION_ARREGLO
                    | REASIGNACION_VALOR_ARREGLO
                    | AUMENTO_DECREMENTO
                    | DECLARACION_OBJETO
                    | ASIGNACION_OBJETO
                    | DECLARACION_ASIGNACION_OBJETO
                    | ACCESO_FUNCION_OBJETO
                    | REASIGNACION_VARIABLE_OBJETO
            ;

            FUNCIONES_NATIVAS.Rule = FUNCION_NATIVA_PRINT
                    | FUNCION_NATIVA_SHOW            
                    | FUNCION_NATIVA_WHILE
                    | FUNCION_NATIVA_FOR
                    | FUNCION_NATIVA_REPEAT
                    | FUNCION_NATIVA_DO_WHILE
                    | FUNCION_NATIVA_IF
                    | FUNCION_NATIVA_COMPROBAR
                    | FUNCION_NATIVA_ADDFIGURE
                    | FUNCION_NATIVA_FIGURE
            ;

            FUNCION_NATIVA_PRINT.Rule = print + parentesisAbre + E + parentesisCierra + puntoYComa;

            FUNCION_NATIVA_SHOW.Rule = show + parentesisAbre + E + coma + E + parentesisCierra + puntoYComa;

            FUNCION_NATIVA_WHILE.Rule = _while + parentesisAbre + E + parentesisCierra + llaveAbre  + SENTENCIAS_BUCLE + llaveCierra;

            FUNCION_NATIVA_FOR.Rule = _for + parentesisAbre + DECLARACIONES_FOR + parentesisCierra + llaveAbre + SENTENCIAS_BUCLE + llaveCierra;

            FUNCION_NATIVA_REPEAT.Rule = repeat + parentesisAbre + E + parentesisCierra + llaveAbre + SENTENCIAS_BUCLE + llaveCierra;

            FUNCION_NATIVA_DO_WHILE.Rule = hacer + llaveAbre + SENTENCIAS_BUCLE + llaveCierra + mientras + parentesisAbre + E + parentesisCierra + puntoYComa;

            FUNCION_NATIVA_COMPROBAR.Rule = comprobar + parentesisAbre + E + parentesisCierra + llaveAbre  + LISTA_CASOS + llaveCierra
                    | comprobar + parentesisAbre + E + parentesisCierra + llaveAbre + LISTA_CASOS  + DEFECTO + llaveCierra
            ;

            LISTA_CASOS.Rule = MakePlusRule(LISTA_CASOS, CASO);

            CASO.Rule = _caso + E + dosPuntos + SENTENCIAS_BUCLE;

            DEFECTO.Rule = _defecto + dosPuntos + SENTENCIAS_BUCLE;

            FUNCION_NATIVA_ADDFIGURE.Rule = addFigure + parentesisAbre + FIGURAS + parentesisCierra + puntoYComa;

            FIGURAS.Rule = FUNCION_NATIVA_CIRCLE
                    | FUNCION_NATIVA_TRIANGLE
                    | FUNCION_NATIVA_SQUARE
                    | FUNCION_NATIVA_LINE
            ;

            FUNCION_NATIVA_CIRCLE.Rule = circle + parentesisAbre + E + coma + E + coma + E + coma + E + coma + E + parentesisCierra;

            FUNCION_NATIVA_TRIANGLE.Rule = triangle + parentesisAbre + E + coma + E + coma + E + coma + E + coma + E + coma + E + coma + E + coma + E + parentesisCierra;

            FUNCION_NATIVA_SQUARE.Rule = square + parentesisAbre + E + coma + E + coma + E + coma + E + coma + E + coma + E + parentesisCierra;

            FUNCION_NATIVA_LINE.Rule = line+ parentesisAbre + E + coma + E + coma + E + coma + E + coma + E + coma + E + parentesisCierra;

            FUNCION_NATIVA_IF.Rule = CONDICION_IF
                    | CONDICION_IF + CONDICION_ELSE
                    | CONDICION_IF + CONDIDICIONES_ELSE_IF
                    | CONDICION_IF + CONDIDICIONES_ELSE_IF + CONDICION_ELSE
            ;

            FUNCION_NATIVA_FIGURE.Rule = figure + parentesisAbre + E + parentesisCierra + puntoYComa;

            CONDIDICIONES_ELSE_IF.Rule = MakePlusRule(CONDIDICIONES_ELSE_IF, CONDICION_ELSE_IF);

            CONDICION_IF.Rule = _if + parentesisAbre + E + parentesisCierra + llaveAbre + SENTENCIAS_BUCLE + llaveCierra; //Para la lista de instrucciones usamos las del bucle por ser igual lo que pueden ejecutar ambas

            CONDICION_ELSE_IF.Rule =  _else + _if + parentesisAbre + E + parentesisCierra + llaveAbre + SENTENCIAS_BUCLE + llaveCierra;

            CONDICION_ELSE.Rule = _else + llaveAbre + SENTENCIAS_BUCLE + llaveCierra;

            DECLARACIONES_FOR.Rule = ASIGNACION_DECLARACION_FOR + puntoYComa + E + puntoYComa + ACTUALIZACION_FOR;

            ASIGNACION_DECLARACION_FOR.Rule = TIPO_DATO + identificador + igual + E
                    | identificador + igual + E
            ;

            ACTUALIZACION_FOR.Rule = DATOS_AUMENTO_DECREMENTO + _aumento
                    | DATOS_AUMENTO_DECREMENTO + _decremento
            ;

            SENTENCIAS_BUCLE.Rule = MakeStarRule(SENTENCIAS_BUCLE, SENTENCIA_BUCLE);

            SENTENCIA_BUCLE.Rule = SENTENCIA_SALIR
                    | SENTENCIA_CONTINUAR
                    | FUNCIONES_NATIVAS
                    | FUNCION_LOCAL
                    | DECLARACION_VARIABLE
                    | ASIGNACION_VARIABLE
                    | DECLARACION_ASIGNACION_VARIABLE
                    | DECLARACION_ARREGLO
                    | DECLARACION_ASIGNACION_ARREGLO
                    | AUMENTO_DECREMENTO
                    | DECLARACION_OBJETO
                    | ASIGNACION_OBJETO
                    | DECLARACION_ASIGNACION_OBJETO
                    | ACCESO_FUNCION_OBJETO
                    | REASIGNACION_VARIABLE_OBJETO
            ;

            SENTENCIA_CONTINUAR.Rule = continuar + puntoYComa;

            SENTENCIA_SALIR.Rule = salir + puntoYComa;

            FUNCION_LOCAL.Rule = identificador + parentesisAbre + LISTA_PARAMETROS_LLAMADA + parentesisCierra + puntoYComa;

            AUMENTO_DECREMENTO.Rule = AUMENTO
                    | DECREMENTO
            ;

            AUMENTO.Rule = DATOS_AUMENTO_DECREMENTO + _aumento + puntoYComa;

            DECREMENTO.Rule = DATOS_AUMENTO_DECREMENTO + _decremento + puntoYComa;

            DATOS_AUMENTO_DECREMENTO.Rule = numeroEntero
                    | numeroDecimal
                    | caracter
                    | identificador
            ;

            DECLARACION_OBJETO.Rule = identificador + identificador + puntoYComa;

            ASIGNACION_OBJETO.Rule = identificador + igual + _new + identificador + parentesisAbre + parentesisCierra + puntoYComa;

            DECLARACION_ASIGNACION_OBJETO.Rule = identificador + identificador + igual + _new + identificador + parentesisAbre + parentesisCierra + puntoYComa;

            ACCESO_VARIABLE_OBJETO.Rule = identificador + punto + identificador;

            ACCESO_FUNCION_OBJETO.Rule = identificador + punto + identificador + parentesisAbre + LISTA_PARAMETROS_LLAMADA + parentesisCierra
                    | identificador + punto + identificador + parentesisAbre + LISTA_PARAMETROS_LLAMADA + parentesisCierra + puntoYComa
            ;

            REASIGNACION_VARIABLE_OBJETO.Rule = identificador + punto + identificador + igual + E + puntoYComa;

            E.Rule = E + suma + E
                    | E + resta + E
                    | E + division + E
                    | E + multiplicacion + E
                    | E + potencia + E
                    | resta + E
                    | E + or + E
                    | E + and + E
                    | not + E
                    | E + _aumento
                    | E + _decremento
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
                    | identificador + DIMENSIONES_ARREGLO
                    | identificador + parentesisAbre + LISTA_PARAMETROS_LLAMADA + parentesisCierra
                    | ACCESO_VARIABLE_OBJETO
                    | ACCESO_FUNCION_OBJETO
            ;
        }

        public override void ReportParseError(ParsingContext context)
        {
            base.ReportParseError(context);
            if (context.CurrentToken.ValueString.Contains("Invalid character"))
                errores.Add(new Error("ERROR LEXICO", "NO SE RECONOCIO ESTE SIMBOLO " + context.CurrentToken.ValueString.ToString() , (context.Source.Location.Line + 1), context.Source.Location.Column));
            else
                errores.Add(new Error("ERROR SINTACTICO", "NO SE ESPERABA ESTE SIMBOLO " + context.CurrentToken.ValueString.ToString(), (context.Source.Location.Line + 1), context.Source.Location.Column));
        }
    }
}
