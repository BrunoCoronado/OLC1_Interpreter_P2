using Irony.Parsing;
using OLC1_Interpreter_P2.sistema.bean;
using OLC1_Interpreter_P2.sistema.graficador;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OLC1_Interpreter_P2.sistema.analisis
{
    class Interprete : Grammar
    {
        private ParseTreeNode raiz;
        private ArrayList clases;
        private ArrayList errores;
        private Clase /*claseActual,*/ claseMain;
        //private Boolean mainEncontrado;
        private Contexto contextoActual;

        public Interprete()
        {
            raiz = null;
            clases = new ArrayList();
            errores = new ArrayList();
            //claseActual = null;
            claseMain = null;
            contextoActual = null;
        }

        public bool analizar(String contenido)
        {
            Gramatica gramatica = new Gramatica();
            LanguageData languageData = new LanguageData(gramatica);
            Parser parser = new Parser(languageData);
            ParseTree parseTree = parser.Parse(contenido);
            Boolean result = true;
            if (parseTree.Root != null)
            {
                Grafica grafica = new Grafica();
                grafica.graficar(parseTree.Root);
                raiz = parseTree.Root;
                analizarAST();
                result = reportarErrores();
                actualizarMain();
                ejecutarMain();
                result = reportarErrores();
                return result;
            }
            return false;
        }

        private Boolean reportarErrores()
        {
            if (errores.Count > 0)
            {
                Reporte reporte = new Reporte();
                reporte.reporteErrores(errores);
                return false;
            }
            return true;
        }

        private void analizarAST()
        {
            if (raiz != null)
            {
                recorrerAST();
            }

        }

        private void recorrerAST()
        {
            foreach (ParseTreeNode nodo in raiz.ChildNodes)
                recorrerDeclaracionClase(nodo);
        }

        private void recorrerDeclaracionClase(ParseTreeNode declaracionClase)
        {
            if (crearClase(declaracionClase.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim()))
            {
                switch (declaracionClase.ChildNodes.Count)
                {
                    case 2:
                        switch (declaracionClase.ChildNodes.ElementAt(1).ToString())
                        {
                            case "IMPORTAR": //definirImports(declaracionClase.ChildNodes.ElementAt(1), clase);
                                break;
                            case "CUERPO_CLASE":
                                capturarContenidoClase(declaracionClase.ChildNodes.ElementAt(1));
                                break;
                        }
                        break;
                    case 3:
                        //definirImports(declaracionClase.ChildNodes.ElementAt(1), clase);
                        capturarContenidoClase(declaracionClase.ChildNodes.ElementAt(2));
                        break;
                }
                Clase clase = new Clase(contextoActual.identificadorClase, contextoActual.tablaDeSimbolos);
                clases.Add(clase);
            }
        }

        private Boolean crearClase(String identificador)
        {
            foreach (Clase c in clases)
            {
                if (c.identificador.Equals(identificador))
                {
                    errores.Add(new Error("ERROR SEMANTICO", "CLASE " + identificador + " YA EXISTE", 0, 0));
                    return false;
                }
            }
            //claseActual = new Clase(identificador);
            contextoActual = new Contexto(identificador);
            return true;
        }

        private Clase definirImports(ParseTreeNode imports, Clase clase)
        {
            foreach (ParseTreeNode nodo in imports.ChildNodes)
            {
                String identificador = nodo.ToString().Replace("(identificador)", "").Trim();
                bool erroExistencia = false;
                foreach (String c in clase.imports)
                {
                    if (c.Equals(identificador))
                    {
                        //error semantico - import repetido
                        erroExistencia = true;
                        break;
                    }
                }
                if (!erroExistencia)
                {
                    clase.agregarImport(identificador);
                }
            }
            return clase;
        }

        private void capturarContenidoClase(ParseTreeNode cuerpoClase)
        {
            foreach (ParseTreeNode nodo in cuerpoClase.ChildNodes)
            {
                switch (nodo.ToString())
                {
                    case "DECLARACION_VARIABLE": declaracionVariable(nodo);
                        break;
                    case "ASIGNACION_VARIABLE": asignacionVariable(nodo);
                        break;
                    case "DECLARACION_ASIGNACION_VARIABLE": declaracionAsignacionVariable(nodo);
                        break;
                    case "DECLARACION_ARREGLO": declaracionArreglo(nodo);
                        break;
                    case "DECLARACION_ASIGNACION_ARREGLO": declaracionAsignacionArreglo(nodo);
                        break;
                    case "DECLARACION_FUNCION_VACIA": declaracionFuncionVacia(nodo);
                        break;
                    case "METODO_MAIN": declaracionFuncionMain(nodo);
                        break;
                    default: Console.WriteLine("CASO NO MANEJADO");
                        break;
                }
            }
        }

        private void declaracionVariable(ParseTreeNode declaracionVariable)
        {
            switch (declaracionVariable.ChildNodes.Count)
            {
                case 2:
                    foreach (ParseTreeNode nodo in declaracionVariable.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        if (!contextoActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), nodo.ToString().Replace("(identificador)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
                case 3:
                    foreach (ParseTreeNode nodo in declaracionVariable.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        if (!contextoActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), nodo.ToString().Replace("(identificador)", "").Trim(), declaracionVariable.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
            }
        }

        private void asignacionVariable(ParseTreeNode asignacionVariable)
        {
            String tipoDato = "";
            Object valor = null, obj = null;
            Contexto c = contextoActual;
            while (c != null)
            {
                obj = c.obtenerSimbolo(asignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
                if (obj == null)
                    c = c.anterior;
                else
                    break;
            }
            if (obj != null)
            {
                Variable variable = (Variable)obj;
                tipoDato = tipoDatoSistema(variable.tipo);
                valor = calcularValor(asignacionVariable.ChildNodes.ElementAt(1));
                if (valor != null)
                {
                    if (valor.GetType().ToString().Equals(tipoDato))
                    {
                        variable.valor = valor;
                        if (!c.actualizarSimbolo(asignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), variable))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + asignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA", 0, 0));
                        }
                    }
                    else
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO PARA VARIABLE " + variable.identificador + " INVALIDO", 0, 0));
                    }
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + asignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
            }
        }

        private void declaracionAsignacionVariable(ParseTreeNode declaracionAsignacionVariable)
        {
            Object valor;
            String tipoDato = "";
            switch (declaracionAsignacionVariable.ChildNodes.Count)
            {
                case 3:
                    switch (declaracionAsignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim().ToLower())
                    {
                        case "int": tipoDato = "System.Int32";
                            break;
                        case "bool": tipoDato = "System.Boolean";
                            break;
                        case "char": tipoDato = "System.Char";
                            break;
                        case "string": tipoDato = "System.String";
                            break;
                        case "double": tipoDato = "System.Double";
                            break;
                    }
                    foreach (ParseTreeNode nodo in declaracionAsignacionVariable.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        valor = calcularValor(declaracionAsignacionVariable.ChildNodes.ElementAt(2));
                        if (valor != null)
                        {
                            if (valor.GetType().ToString().Equals(tipoDato))
                            {
                                if (!contextoActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionAsignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), nodo.ToString().Replace("(identificador)", "").Trim(), valor)))
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO PARA VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " INVALIDO", 0, 0));
                            }
                        }
                    }
                    break;
                case 4:
                    switch (declaracionAsignacionVariable.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim().ToLower())
                    {
                        case "int": tipoDato = "System.Int32";
                            break;
                        case "bool": tipoDato = "System.Boolean";
                            break;
                        case "char": tipoDato = "System.Char";
                            break;
                        case "string": tipoDato = "System.String";
                            break;
                        case "double": tipoDato = "System.Double";
                            break;
                    }
                    foreach (ParseTreeNode nodo in declaracionAsignacionVariable.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        valor = calcularValor(declaracionAsignacionVariable.ChildNodes.ElementAt(3));
                        if (valor != null)
                        {
                            if (valor.GetType().ToString().Equals(tipoDato))
                            {
                                if (!contextoActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionAsignacionVariable.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), nodo.ToString().Replace("(identificador)", "").Trim(), valor, declaracionAsignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim())))
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE" + nodo.ToString().Replace("(identificador)", "").Trim() + "YA EXISTE", 0, 0));
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO PARA VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " INVALIDO", 0, 0));
                            }
                        }
                    }
                    break;
            }
        }
        
        private void declaracionArreglo(ParseTreeNode declaracionArreglo)
        {
            ArrayList dimensiones = new ArrayList();
            switch (declaracionArreglo.ChildNodes.Count)
            {
                case 3:
                    foreach (ParseTreeNode nodoDimension in declaracionArreglo.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        Object valorDimension = resolverExpresion(nodoDimension);
                        if (valorDimension.GetType().ToString().Equals("System.Int32"))
                        {
                            dimensiones.Add(valorDimension);
                        }
                        else
                        {
                            dimensiones = null;
                            break;
                        }
                    }
                    foreach (ParseTreeNode nodo in declaracionArreglo.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        if (dimensiones != null)
                        {
                            Arreglo arreglo = new Arreglo(nodo.ToString().Replace("(identificador)", "").Trim(), declaracionArreglo.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), dimensiones);
                            if (!contextoActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), arreglo))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VALOR PARA DIMENSION PARA ARREGLO " + nodo.ToString().Replace("(identificador)", "").Trim() + " NO VALIDA - SE ESPERABA INT", 0, 0));
                        }
                    }
                    break;
                case 4:
                    foreach (ParseTreeNode nodoDimension in declaracionArreglo.ChildNodes.ElementAt(3).ChildNodes)
                    {
                        Object valorDimension = resolverExpresion(nodoDimension);
                        if (valorDimension.GetType().ToString().Equals("System.Int32"))
                        {
                            dimensiones.Add(valorDimension);
                        }
                        else
                        {
                            dimensiones = null;
                            break;
                        }
                    }
                    foreach (ParseTreeNode nodo in declaracionArreglo.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        if (dimensiones != null)
                        {
                            Arreglo arreglo = new Arreglo(nodo.ToString().Replace("(identificador)", "").Trim(), declaracionArreglo.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), declaracionArreglo.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim(), dimensiones);
                            if (!contextoActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), arreglo))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VALOR PARA DIMENSION PARA ARREGLO " + nodo.ToString().Replace("(identificador)", "").Trim() + " NO VALIDA - SE ESPERABA INT", 0, 0));
                        }
                    }
                    break;
            }
        }

        private void declaracionAsignacionArreglo(ParseTreeNode declaracionAsignacionArreglo)
        {
            ArrayList dimensiones = new ArrayList();
            switch (declaracionAsignacionArreglo.ChildNodes.Count)
            {
                case 4:
                    foreach (ParseTreeNode nodoDimension in declaracionAsignacionArreglo.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        Object valorDimension = resolverExpresion(nodoDimension);
                        if (valorDimension.GetType().ToString().Equals("System.Int32"))
                        {
                            dimensiones.Add(valorDimension);
                        }
                        else
                        {
                            dimensiones = null;
                            break;
                        }
                    }
                    foreach (ParseTreeNode nodo in declaracionAsignacionArreglo.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        if (dimensiones != null)
                        {
                            Arreglo arreglo = new Arreglo(nodo.ToString().Replace("(identificador)", "").Trim(), declaracionAsignacionArreglo.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), dimensiones);
                            arreglo = llenarArreglo(arreglo, declaracionAsignacionArreglo.ChildNodes.ElementAt(3));
                            if (arreglo != null)
                            {
                                if (!contextoActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), arreglo))
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                                }
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VALOR PARA DIMENSION PARA ARREGLO " + nodo.ToString().Replace("(identificador)", "").Trim() + " NO VALIDA", 0, 0));
                        }
                    }
                    break;
                case 5:
                    foreach (ParseTreeNode nodoDimension in declaracionAsignacionArreglo.ChildNodes.ElementAt(3).ChildNodes)
                    {
                        Object valorDimension = resolverExpresion(nodoDimension);
                        if (valorDimension.GetType().ToString().Equals("System.Int32"))
                        {
                            dimensiones.Add(valorDimension);
                        }
                        else
                        {
                            dimensiones = null;
                            break;
                        }
                    }
                    foreach (ParseTreeNode nodo in declaracionAsignacionArreglo.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        if (dimensiones != null)
                        {
                            Arreglo arreglo = new Arreglo(nodo.ToString().Replace("(identificador)", "").Trim(), declaracionAsignacionArreglo.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), declaracionAsignacionArreglo.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim(), dimensiones);
                            arreglo = llenarArreglo(arreglo, declaracionAsignacionArreglo.ChildNodes.ElementAt(4));
                            if (arreglo != null)
                            {
                                if (!contextoActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), arreglo))
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                                }
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VALOR PARA DIMENSION PARA ARREGLO " + nodo.ToString().Replace("(identificador)", "").Trim() + " NO VALIDA", 0, 0));
                        }
                    }
                    break;
            }
        }

        private Arreglo llenarArreglo(Arreglo arreglo, ParseTreeNode valores)
        {
            Object valor;
            String tipoArreglo = "";
            switch (arreglo.tipo.ToLower())
            {
                case "int": tipoArreglo = "System.Int32";
                    break;
                case "bool": tipoArreglo = "System.Boolean";
                    break;
                case "char": tipoArreglo = "System.Char";
                    break;
                case "string": tipoArreglo = "System.String";
                    break;
                case "double": tipoArreglo = "System.Double";
                    break;
            }
            switch (arreglo.dimensiones.Count)
            {
                case 1:
                    if (Int32.Parse(arreglo.dimensiones[0].ToString()) == valores.ChildNodes.Count)
                    {
                        for (int i = 0 ; i < valores.ChildNodes.Count ; i++)
                        {
                            valor = calcularValor(valores.ChildNodes.ElementAt(i));
                            if (valor != null)
                            {
                                if (valor.GetType().ToString().Equals(tipoArreglo))
                                {
                                    arreglo.agregarValor(i.ToString(), valor);
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO NO VALIDO PARA EL ARREGLO " + arreglo.identificador, 0, 0));
                                    return null;
                                }
                            }
                        }
                        return arreglo;
                    }
                    errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                    return null;
                case 2:
                    if (Int32.Parse(arreglo.dimensiones[0].ToString()) == valores.ChildNodes.Count)
                    {
                        for (int i = 0; i < valores.ChildNodes.Count; i++)
                        {
                            if (Int32.Parse(arreglo.dimensiones[1].ToString()) == valores.ChildNodes.ElementAt(i).ChildNodes.Count)
                            {
                                for (int j = 0 ; j < valores.ChildNodes.ElementAt(i).ChildNodes.Count ; j++)
                                {
                                    valor = calcularValor(valores.ChildNodes.ElementAt(i).ChildNodes.ElementAt(j));
                                    if (valor != null)
                                    {
                                        if (valor.GetType().ToString().Equals(tipoArreglo))
                                        {
                                            arreglo.agregarValor(i.ToString() + "-" + j.ToString(), valor);
                                        }
                                        else
                                        {
                                            errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO NO VALIDO PARA EL ARREGLO " + arreglo.identificador, 0, 0));
                                            return null;
                                        }
                                    }
                                }
                                return arreglo;
                            }
                            errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                            return null;
                        }
                    }
                    errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                    return null;
                case 3:
                    if (Int32.Parse(arreglo.dimensiones[0].ToString()) == valores.ChildNodes.Count)
                    {
                        for (int i = 0; i < valores.ChildNodes.Count; i++)
                        {
                            if (Int32.Parse(arreglo.dimensiones[1].ToString()) == valores.ChildNodes.ElementAt(i).ChildNodes.Count)
                            {
                                for (int j = 0; j < valores.ChildNodes.ElementAt(i).ChildNodes.Count; j++)
                                {
                                    if (Int32.Parse(arreglo.dimensiones[2].ToString()) == valores.ChildNodes.ElementAt(i).ChildNodes.ElementAt(j).ChildNodes.Count)
                                    {
                                        for (int k = 0; k < valores.ChildNodes.ElementAt(i).ChildNodes.ElementAt(j).ChildNodes.Count; k++)
                                        {
                                            valor = calcularValor(valores.ChildNodes.ElementAt(i).ChildNodes.ElementAt(j).ChildNodes.ElementAt(k));
                                            if (valor != null)
                                            {
                                                if (valor.GetType().ToString().Equals(tipoArreglo))
                                                {
                                                    arreglo.agregarValor(i.ToString() + "-" + j.ToString() + "-" + k.ToString(), valor);
                                                }
                                                else
                                                {
                                                    errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO NO VALIDO PARA EL ARREGLO " + arreglo.identificador, 0, 0));
                                                    return null;
                                                }
                                            }
                                        }
                                        return arreglo;
                                    }
                                    errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                                    return null;
                                }
                            }
                            errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                            return null;
                        }
                    }
                    errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                    return null;
            }
            return null;
        }

        private Object calcularValor(ParseTreeNode nodoE)
        {
            try
            {
                return resolverExpresion(nodoE);
            }
            catch(Exception ex)
            {
                //Console.WriteLine("Error al calcular valor");
            }
            return null;
        }

        private Object resolverExpresion(ParseTreeNode nodoE)
        {
            switch (nodoE.ChildNodes.Count)
            {
                case 1:
                    String tipo = nodoE.ChildNodes.ElementAt(0).ToString().Substring(nodoE.ChildNodes.ElementAt(0).ToString().LastIndexOf('(') - 1);
                    String valor = nodoE.ChildNodes.ElementAt(0).ToString().Substring(0 ,(nodoE.ChildNodes.ElementAt(0).ToString().Length - tipo.Length));
                    switch (tipo.Trim())
                    {
                        case "(int)": return Int32.Parse(valor.Trim());
                        case "(double)": return Double.Parse(valor.Trim());
                        case "(char)": return Char.Parse(valor.Trim());
                        case "(string)": return valor;
                        case "(bool)":
                            switch (valor.Trim().ToLower())
                            {
                                case "true": return true;
                                case "false": return false;
                                case "verdadero": return true;
                                case "falso": return false;
                                default: return null;
                            }
                        case "(identificador)":
                            Object obj = buscarSimboloEnContexto(valor);
                            if (obj == null)
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + valor + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
                                return null;
                            }
                            switch (((Variable)obj).tipo)
                            {
                                case "char": return Char.Parse(((Variable)obj).valor.ToString());
                                case "string": return ((Variable)obj).valor.ToString();
                                case "int": return Int32.Parse(((Variable)obj).valor.ToString());
                                case "double": return Double.Parse(((Variable)obj).valor.ToString());
                                case "bool": return Boolean.Parse(((Variable)obj).valor.ToString());
                            }
                            return null;
                        default: Console.WriteLine("OPERACION NO MANEJADA");
                            return null;
                    }
                case 2:
                    Object valOP = resolverExpresion(nodoE.ChildNodes.ElementAt(1));
                    switch (nodoE.ChildNodes.ElementAt(0).ToString().Replace("(Key symbol)", "").Trim())
                    {
                        case "-":
                            switch (valOP.GetType().ToString())
                            {
                                case "System.String": errores.Add(new Error("ERROR SEMANTICO", "SIGNO MENOS CON STRING", 0, 0));
                                    return null;
                                case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "SIGNO MENOS CON CHAR", 0, 0));
                                    return null;
                                case "System.Double": return Double.Parse((Double.Parse(valOP.ToString()) * -1).ToString());
                                case "System.Int32": return Int32.Parse((Int32.Parse(valOP.ToString()) * -1).ToString());
                                case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "SIGNO MENOS CON BOOL", 0, 0));
                                    return null;
                                default: return null;
                            }
                        case "!":
                            switch (valOP.GetType().ToString())
                            {
                                case "System.String": errores.Add(new Error("ERROR SEMANTICO", "NOT NO SOPORTA STRING", 0, 0));
                                    return null;
                                case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "NOT NO SOPORTA CHAR", 0, 0));
                                    return null;
                                case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "NOT NO SOPORTA DOUBLE", 0, 0));
                                    return null;
                                case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "NOT NO SOPORTA INT", 0, 0));
                                    return null;
                                case "System.Boolean": return (!Boolean.Parse(valOP.ToString()));
                                default: return null;
                            }
                        default: Console.WriteLine("CASO NO MANEJADO");
                            return null;
                    }
                case 3:
                    int val;
                    double valP;
                    Object valorA = resolverExpresion(nodoE.ChildNodes.ElementAt(0));
                    Object valorB = resolverExpresion(nodoE.ChildNodes.ElementAt(2));
                    switch (nodoE.ChildNodes.ElementAt(1).ToString().Replace("(Key symbol)", "").Trim())
                    {
                        case "+":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": return (valorA.ToString() + valorB.ToString());
                                        case "System.Char": return (valorA.ToString() + valorB.ToString());
                                        case "System.Double": return (valorA.ToString() + valorB.ToString());
                                        case "System.Int32": return (valorA.ToString() + valorB.ToString());
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "SUMA ENTRE STRING Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": return (valorA.ToString() + valorB.ToString());
                                        case "System.Char": return Int32.Parse(((int)(Char.Parse(valorA.ToString())) + (int)(Char.Parse(valorB.ToString()))).ToString());
                                        case "System.Double": return Double.Parse(((int)(Char.Parse(valorA.ToString())) + Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Int32.Parse(((int)(Char.Parse(valorA.ToString())) + Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return Int32.Parse(((int)(Char.Parse(valorA.ToString())) + val).ToString());
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": return (valorA.ToString() + valorB.ToString());
                                        case "System.Char": return Double.Parse(((int)(Char.Parse(valorB.ToString())) + Double.Parse(valorA.ToString())).ToString());
                                        case "System.Double": return Double.Parse((Double.Parse(valorA.ToString()) + Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Double.Parse((Double.Parse(valorA.ToString()) + Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return Double.Parse((Double.Parse(valorA.ToString()) + val).ToString());
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": return (valorA.ToString() + valorB.ToString());
                                        case "System.Char": return Int32.Parse(((int)(Char.Parse(valorB.ToString())) + Int32.Parse(valorA.ToString())).ToString());
                                        case "System.Double": return Double.Parse((Int32.Parse(valorA.ToString()) + Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Int32.Parse((Int32.Parse(valorA.ToString()) + Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return Int32.Parse((Int32.Parse(valorA.ToString()) + val).ToString());
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "SUMA ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return Int32.Parse(((int)(Char.Parse(valorB.ToString())) + val).ToString());
                                        case "System.Double": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return Double.Parse((val + Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32":
                                            val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return Int32.Parse((val + Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean":
                                            val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            int val2 = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            if ((val + val2) >= 1)
                                                return true;
                                            return false;
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "-":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE STRING Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE STRING Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE STRING Y DOUBLE", 0, 0));
                                            return null;
                                        case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE STRING Y ENTERO", 0, 0));
                                            return null;
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE STRING Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return Int32.Parse(((int)(Char.Parse(valorA.ToString())) - (int)(Char.Parse(valorB.ToString()))).ToString());
                                        case "System.Double": return Double.Parse(((int)(Char.Parse(valorA.ToString())) - Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Int32.Parse(((int)(Char.Parse(valorA.ToString())) - Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE CHAR Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return Double.Parse((Double.Parse(valorA.ToString()) - (int)(Char.Parse(valorB.ToString()))).ToString());
                                        case "System.Double": return Double.Parse((Double.Parse(valorA.ToString()) - Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Double.Parse((Double.Parse(valorA.ToString()) - Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean":
                                            val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return Double.Parse((Double.Parse(valorA.ToString()) - val).ToString());
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return Int32.Parse((Int32.Parse(valorA.ToString()) - (int)(Char.Parse(valorB.ToString()))).ToString());
                                        case "System.Double": return Double.Parse((Int32.Parse(valorA.ToString()) - Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Int32.Parse((Int32.Parse(valorA.ToString()) - Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean":
                                            val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return Int32.Parse((Int32.Parse(valorA.ToString()) - val).ToString());
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return Double.Parse((val - Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return Int32.Parse((val - Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE BOOL Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "*":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE STRING Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE STRING Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE STRING Y DOUBLE", 0, 0));
                                            return null;
                                        case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE STRING Y ENTERO", 0, 0));
                                            return null;
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE STRING Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return Int32.Parse(((int)(Char.Parse(valorA.ToString())) * (int)(Char.Parse(valorB.ToString()))).ToString());
                                        case "System.Double": return Double.Parse(((int)(Char.Parse(valorA.ToString())) * Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Int32.Parse(((int)(Char.Parse(valorA.ToString())) * Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return Int32.Parse(((int)(Char.Parse(valorA.ToString())) * val).ToString());
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return Double.Parse(((int)(Char.Parse(valorB.ToString())) * Double.Parse(valorA.ToString())).ToString());
                                        case "System.Double": return Double.Parse((Double.Parse(valorA.ToString()) * Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Double.Parse((Double.Parse(valorA.ToString()) * Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return Double.Parse((Double.Parse(valorA.ToString()) * val).ToString());
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return Int32.Parse(((int)(Char.Parse(valorB.ToString())) * Int32.Parse(valorA.ToString())).ToString());
                                        case "System.Double": return Double.Parse((Int32.Parse(valorA.ToString()) * Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Int32.Parse((Int32.Parse(valorA.ToString()) * Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return Int32.Parse((Int32.Parse(valorA.ToString()) * val).ToString());
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MULTIPLICACION ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return Int32.Parse((val * (int)(Char.Parse(valorB.ToString()))).ToString());
                                        case "System.Double": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return Double.Parse((val * Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return Int32.Parse((val * Int32.Parse(valorB.ToString())).ToString());
                                        case "System.Boolean": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            int val2 = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            if ((val * val2) >= 1)
                                                return true;
                                            return false;
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "/":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE STRING Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE STRING Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE STRING Y DOUBLE", 0, 0));
                                            return null;
                                        case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE STRING Y ENTERO", 0, 0));
                                            return null;
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE STRING Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char":
                                            try
                                            {
                                                return Double.Parse(((int)(Char.Parse(valorA.ToString())) / (int)(Char.Parse(valorB.ToString()))).ToString());
                                            }catch(Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Double":
                                            try
                                            {
                                                return Double.Parse(((int)(Char.Parse(valorA.ToString())) / Double.Parse(valorB.ToString())).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Int32":
                                            try
                                            {
                                                return Double.Parse(((int)(Char.Parse(valorA.ToString())) / Int32.Parse(valorB.ToString())).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Boolean":
                                            try
                                            {
                                                val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                                return Int32.Parse(((int)(Char.Parse(valorA.ToString())) / val).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "RESTA ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char":
                                            try
                                            {
                                                return Double.Parse((Double.Parse(valorA.ToString()) / (int)(Char.Parse(valorB.ToString()))).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Double": 
                                            try
                                            {
                                                return Double.Parse((Double.Parse(valorA.ToString()) / Double.Parse(valorB.ToString())).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Int32":
                                            try
                                            {
                                                return Double.Parse((Double.Parse(valorA.ToString()) / Int32.Parse(valorB.ToString())).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Boolean":
                                            try
                                            {
                                                val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                                return Double.Parse((Double.Parse(valorA.ToString()) / val).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char":
                                            try
                                            {
                                                return Double.Parse((Int32.Parse(valorA.ToString()) / (int)(Char.Parse(valorB.ToString()))).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Double":
                                            try
                                            {
                                                return Double.Parse((Int32.Parse(valorA.ToString()) / Double.Parse(valorB.ToString())).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Int32":
                                            try
                                            {
                                                return Double.Parse((Int32.Parse(valorA.ToString()) / Int32.Parse(valorB.ToString())).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Boolean":
                                            try
                                            {
                                                val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                                return Int32.Parse((Int32.Parse(valorA.ToString()) / val).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char":
                                            try
                                            {
                                                val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                                return Double.Parse((val / (int)(Char.Parse(valorB.ToString()))).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Double":
                                            try
                                            {
                                                val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                                return Double.Parse((val / Double.Parse(valorB.ToString())).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Int32":
                                            try
                                            {
                                                val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                                return Double.Parse((val / Int32.Parse(valorB.ToString())).ToString());
                                            }
                                            catch (Exception e)
                                            {
                                                errores.Add(new Error("ERROR EJECUCION", "DIVISION INVALIDA", 0, 0));
                                                return null;
                                            }
                                        case "System.Boolean":
                                            errores.Add(new Error("ERROR SEMANTICO", "DIVISION ENTRE BOOL Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "^":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE STRING Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE STRING Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE STRING Y DOUBLE", 0, 0));
                                            return null;
                                        case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE STRING Y ENTERO", 0, 0));
                                            return null;
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE STRING Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE CHAR Y STRING", 0, 0));
                                            return null; 
                                        case "System.Char": return Int32.Parse(Math.Pow(Double.Parse(((int)(Char.Parse(valorA.ToString()))).ToString()), Double.Parse(((int)(Char.Parse(valorB.ToString()))).ToString())).ToString());
                                        case "System.Double": return Double.Parse((Math.Pow(Double.Parse(((int)(Char.Parse(valorA.ToString()))).ToString()), Double.Parse(valorB.ToString()))).ToString());
                                        case "System.Int32": return Int32.Parse(Math.Pow(Double.Parse(((int)(Char.Parse(valorA.ToString()))).ToString()), Double.Parse((Int32.Parse(valorB.ToString())).ToString())).ToString()); 
                                        case "System.Boolean": valP = Boolean.Parse(valorB.ToString()) ? 1.0 : 0.0;
                                            return Int32.Parse(Math.Pow(Double.Parse(((int)(Char.Parse(valorA.ToString()))).ToString()), valP).ToString());
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return Double.Parse(Math.Pow(Double.Parse(valorA.ToString()), Double.Parse(((int)(Char.Parse(valorB.ToString()))).ToString())).ToString());
                                        case "System.Double": return Double.Parse(Math.Pow(Double.Parse(valorA.ToString()), Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Double.Parse(Math.Pow(Double.Parse(valorA.ToString()), Double.Parse((Int32.Parse(valorB.ToString())).ToString())).ToString()); 
                                        case "System.Boolean": valP = Boolean.Parse(valorB.ToString()) ? 1.0 : 0.0;
                                            return Double.Parse(Math.Pow(Double.Parse(valorA.ToString()), valP).ToString());
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE INT Y STRING", 0, 0));
                                            return null; 
                                        case "System.Char": return Int32.Parse(Math.Pow(Double.Parse((Int32.Parse(valorA.ToString())).ToString()), Double.Parse(((int)(Char.Parse(valorB.ToString()))).ToString())).ToString());
                                        case "System.Double": return Double.Parse(Math.Pow(Double.Parse((Int32.Parse(valorA.ToString())).ToString()), Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": return Int32.Parse(Math.Pow(Double.Parse((Int32.Parse(valorA.ToString())).ToString()), Double.Parse((Int32.Parse(valorB.ToString())).ToString())).ToString());
                                        case "System.Boolean": valP = Boolean.Parse(valorB.ToString()) ? 1.0 : 0.0;
                                            return Int32.Parse(Math.Pow(Double.Parse((Int32.Parse(valorA.ToString())).ToString()), valP).ToString());
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": valP = Boolean.Parse(valorA.ToString()) ? 1.0 : 0.0;
                                            return Int32.Parse(Math.Pow(valP, Double.Parse(((int)(Char.Parse(valorB.ToString()))).ToString())).ToString());
                                        case "System.Double": valP = Boolean.Parse(valorA.ToString()) ? 1.0 : 0.0;
                                            return Double.Parse(Math.Pow(valP, Double.Parse(valorB.ToString())).ToString());
                                        case "System.Int32": valP = Boolean.Parse(valorA.ToString()) ? 1.0 : 0.0;
                                            return Int32.Parse(Math.Pow(valP, Double.Parse((Int32.Parse(valorB.ToString())).ToString())).ToString());
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "POTENCIA ENTRE BOOL Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "==":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": return (valorA.ToString().Equals(valorB.ToString()));
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE STRING Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE STRING Y DOUBLE", 0, 0));
                                            return null;
                                        case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE STRING Y INT", 0, 0));
                                            return null;
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE STRING Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Char.Parse(valorA.ToString()).Equals(Char.Parse(valorB.ToString())));
                                        case "System.Double": return (((int)Char.Parse(valorA.ToString())) == Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (((int)Char.Parse(valorA.ToString())) == Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE CHAR Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Double.Parse(valorA.ToString()) == ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Double.Parse(valorA.ToString()) == Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Double.Parse(valorA.ToString()) == Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Double.Parse(valorA.ToString()) == val);
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Int32.Parse(valorA.ToString()) == ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Int32.Parse(valorA.ToString()) == Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Int32.Parse(valorA.ToString()) == Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Int32.Parse(valorA.ToString()) == val);
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "IGUALACION ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val == Double.Parse(valorB.ToString()));
                                        case "System.Int32": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val == Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": return (Boolean.Parse(valorA.ToString()) == Boolean.Parse(valorB.ToString()));
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "!=":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": return (!valorA.ToString().Equals(valorB.ToString()));
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE STRING Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE STRING Y DOUBLE", 0, 0));
                                            return null;
                                        case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE STRING Y INT", 0, 0));
                                            return null;
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE STRING Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (!Char.Parse(valorA.ToString()).Equals(Char.Parse(valorB.ToString())));
                                        case "System.Double": return (((int)Char.Parse(valorA.ToString())) != Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (((int)Char.Parse(valorA.ToString())) != Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE CHAR Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Double.Parse(valorA.ToString()) != ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Double.Parse(valorA.ToString()) != Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Double.Parse(valorA.ToString()) != Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Double.Parse(valorA.ToString()) != val);
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Int32.Parse(valorA.ToString()) != ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Int32.Parse(valorA.ToString()) != Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Int32.Parse(valorA.ToString()) != Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Int32.Parse(valorA.ToString()) != val);
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "DIFERENCIACION ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val != Double.Parse(valorB.ToString()));
                                        case "System.Int32": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val != Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": return (Boolean.Parse(valorA.ToString()) != Boolean.Parse(valorB.ToString()));
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "<":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MENOR QUE NO SOPORTA STRING", 0, 0));
                                    return null;
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String":
                                            errores.Add(new Error("ERROR SEMANTICO", "MENOR QUE ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (((int)Char.Parse(valorA.ToString())) < ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (((int)Char.Parse(valorA.ToString())) < Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (((int)Char.Parse(valorA.ToString())) < Int32.Parse(valorB.ToString()));
                                        case "System.Boolean":
                                            errores.Add(new Error("ERROR SEMANTICO", "MENOR QUE ENTRE CHAR Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String":
                                            errores.Add(new Error("ERROR SEMANTICO", "MENOR QUE ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Double.Parse(valorA.ToString()) < ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Double.Parse(valorA.ToString()) < Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Double.Parse(valorA.ToString()) < Int32.Parse(valorB.ToString()));
                                        case "System.Boolean":
                                            val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Double.Parse(valorA.ToString()) < val);
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String":
                                            errores.Add(new Error("ERROR SEMANTICO", "MENOR QUE ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Int32.Parse(valorA.ToString()) < ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Int32.Parse(valorA.ToString()) < Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Int32.Parse(valorA.ToString()) < Int32.Parse(valorB.ToString()));
                                        case "System.Boolean":
                                            val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Int32.Parse(valorA.ToString()) < val);
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String":
                                            errores.Add(new Error("ERROR SEMANTICO", "MENOR QUE ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char":
                                            errores.Add(new Error("ERROR SEMANTICO", "MENOR QUE ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double":
                                            val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val < Double.Parse(valorB.ToString()));
                                        case "System.Int32":
                                            val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val < Int32.Parse(valorB.ToString()));
                                        case "System.Boolean":
                                            val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            int val2 = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (val < val2);
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "<=":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MENOR IGUAL NO SOPORTA STRING", 0, 0));
                                    return null;
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String":errores.Add(new Error("ERROR SEMANTICO", "MENOR IGUAL ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (((int)Char.Parse(valorA.ToString())) <= ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (((int)Char.Parse(valorA.ToString())) <= Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (((int)Char.Parse(valorA.ToString())) <= Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "MENOR IGUAL ENTRE CHAR Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MENOR IGUAL ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Double.Parse(valorA.ToString()) <= ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Double.Parse(valorA.ToString()) <= Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Double.Parse(valorA.ToString()) <= Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Double.Parse(valorA.ToString()) <= val);
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MENOR IGUAL ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Int32.Parse(valorA.ToString()) <= ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Int32.Parse(valorA.ToString()) <= Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Int32.Parse(valorA.ToString()) <= Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Int32.Parse(valorA.ToString()) <= val);
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MENOR IGUAL ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "MENOR IGUAL ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val <= Double.Parse(valorB.ToString()));
                                        case "System.Int32": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val <= Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            int val2 = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (val <= val2);
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case ">":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR QUE NO SOPORTA STRING", 0, 0));
                                    return null;
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR QUE ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (((int)Char.Parse(valorA.ToString())) > ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (((int)Char.Parse(valorA.ToString())) > Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (((int)Char.Parse(valorA.ToString())) > Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "MAYOR QUE ENTRE CHAR Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR QUE ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Double.Parse(valorA.ToString()) > ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Double.Parse(valorA.ToString()) > Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Double.Parse(valorA.ToString()) > Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Double.Parse(valorA.ToString()) > val);
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR QUE ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Int32.Parse(valorA.ToString()) > ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Int32.Parse(valorA.ToString()) > Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Int32.Parse(valorA.ToString()) > Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Int32.Parse(valorA.ToString()) > val);
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR QUE ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "MAYOR QUE ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val > Double.Parse(valorB.ToString()));
                                        case "System.Int32": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val > Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            int val2 = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (val > val2);
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case ">=":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR IGUAL NO SOPORTA STRING", 0, 0));
                                    return null;
                                case "System.Char":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR IGUAL ENTRE CHAR Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (((int)Char.Parse(valorA.ToString())) >= ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (((int)Char.Parse(valorA.ToString())) >= Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (((int)Char.Parse(valorA.ToString())) >= Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "MAYOR IGUAL ENTRE CHAR Y BOOL", 0, 0));
                                            return null;
                                        default: return null;
                                    }
                                case "System.Double":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR IGUAL ENTRE DOUBLE Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Double.Parse(valorA.ToString()) >= ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Double.Parse(valorA.ToString()) >= Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Double.Parse(valorA.ToString()) >= Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Double.Parse(valorA.ToString()) >= val);
                                        default: return null;
                                    }
                                case "System.Int32":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR IGUAL ENTRE INT Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": return (Int32.Parse(valorA.ToString()) >= ((int)Char.Parse(valorB.ToString())));
                                        case "System.Double": return (Int32.Parse(valorA.ToString()) >= Double.Parse(valorB.ToString()));
                                        case "System.Int32": return (Int32.Parse(valorA.ToString()) >= Int32.Parse(valorB.ToString()));
                                        case "System.Boolean": val = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (Int32.Parse(valorA.ToString()) >= val);
                                        default: return null;
                                    }
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "MAYOR IGUAL ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "MAYOR IGUAL ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val >= Double.Parse(valorB.ToString()));
                                        case "System.Int32":
                                            val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            return (val >= Int32.Parse(valorB.ToString()));
                                        case "System.Boolean":
                                            val = Boolean.Parse(valorA.ToString()) ? 1 : 0;
                                            int val2 = Boolean.Parse(valorB.ToString()) ? 1 : 0;
                                            return (val >= val2);
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "&&":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String": errores.Add(new Error("ERROR SEMANTICO", "AND NO SOPORTA STRING", 0, 0));
                                    return null;
                                case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "AND NO SOPORTA CHAR", 0, 0));
                                    return null;
                                case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "AND NO SOPORTA DOUBLE", 0, 0));
                                    return null;
                                case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "AND NO SOPORTA INT", 0, 0));
                                    return null;
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String": errores.Add(new Error("ERROR SEMANTICO", "AND ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char": errores.Add(new Error("ERROR SEMANTICO", "AND ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double": errores.Add(new Error("ERROR SEMANTICO", "AND ENTRE BOOL Y DOUBLE", 0, 0));
                                            return null;
                                        case "System.Int32": errores.Add(new Error("ERROR SEMANTICO", "AND ENTRE BOOL E INT", 0, 0));
                                            return null;
                                        case "System.Boolean": return (Boolean.Parse(valorA.ToString()) && Boolean.Parse(valorB.ToString()));
                                        default: return null;
                                    }
                                default: return null;
                            }
                        case "||":
                            switch (valorA.GetType().ToString())
                            {
                                case "System.String":
                                    errores.Add(new Error("ERROR SEMANTICO", "OR NO SOPORTA STRING", 0, 0));
                                    return null;
                                case "System.Char":
                                    errores.Add(new Error("ERROR SEMANTICO", "OR NO SOPORTA CHAR", 0, 0));
                                    return null;
                                case "System.Double":
                                    errores.Add(new Error("ERROR SEMANTICO", "OR NO SOPORTA DOUBLE", 0, 0));
                                    return null;
                                case "System.Int32":
                                    errores.Add(new Error("ERROR SEMANTICO", "OR NO SOPORTA INT", 0, 0));
                                    return null;
                                case "System.Boolean":
                                    switch (valorB.GetType().ToString())
                                    {
                                        case "System.String":
                                            errores.Add(new Error("ERROR SEMANTICO", "OR ENTRE BOOL Y STRING", 0, 0));
                                            return null;
                                        case "System.Char":
                                            errores.Add(new Error("ERROR SEMANTICO", "OR ENTRE BOOL Y CHAR", 0, 0));
                                            return null;
                                        case "System.Double":
                                            errores.Add(new Error("ERROR SEMANTICO", "OR ENTRE BOOL Y DOUBLE", 0, 0));
                                            return null;
                                        case "System.Int32":
                                            errores.Add(new Error("ERROR SEMANTICO", "OR ENTRE BOOL E INT", 0, 0));
                                            return null;
                                        case "System.Boolean": return (Boolean.Parse(valorA.ToString()) || Boolean.Parse(valorB.ToString()));
                                        default: return null;
                                    }
                                default: return null;
                            }
                        default: Console.WriteLine("OPERACION NO MANEJADA");
                            return null;
                    }
            }
            return null;
        }

        private void declaracionFuncionVacia(ParseTreeNode declaracion)
        {
            Funcion funcion = null;
            Boolean error = false;
            switch (declaracion.ChildNodes.Count)
            {
                case 3:
                    funcion = new Funcion(declaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), declaracion.ChildNodes.ElementAt(2));
                    foreach (ParseTreeNode parametro in declaracion.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        if (!funcion.agregarParametro(parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(parametro.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "PARAMETRO " + parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " EN FUNCION " + declaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                            error = true;
                            break;
                        }
                    }
                    if (!error)
                    {
                        if (!contextoActual.agregarSimbolo(declaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim().Insert(0, "@"), funcion))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + declaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
                case 4:
                    if (declaracion.ChildNodes.ElementAt(1).ToString().ToLower().Contains("override (keyword)"))//override
                    {
                        funcion = new Funcion(declaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), declaracion.ChildNodes.ElementAt(3));
                        foreach (ParseTreeNode parametro in declaracion.ChildNodes.ElementAt(2).ChildNodes)
                        {
                            if (!funcion.agregarParametro(parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(parametro.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "PARAMETRO " + parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " EN FUNCION " + declaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                                error = true;
                                break;
                            }
                        }
                        if (!error)
                        {
                            if (!contextoActual.agregarSimbolo(declaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim().Insert(0, "@"), funcion))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + declaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                            }
                        }
                    }
                    else
                    {
                        funcion = new Funcion(declaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), declaracion.ChildNodes.ElementAt(3), declaracion.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim());
                        foreach (ParseTreeNode parametro in declaracion.ChildNodes.ElementAt(2).ChildNodes)
                        {
                            if (!funcion.agregarParametro(parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(parametro.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                            {
                                errores.Add(new Error("1ERROR SEMANTICO", "PARAMETRO  " + parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " EN FUNCION " + declaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                                error = true;
                                break;
                            }
                        }
                        if (!error)
                        {
                            if (!contextoActual.agregarSimbolo(declaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim().Insert(0, "@"), funcion))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + declaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                            }
                        }
                    }
                    break;
                case 5: //override
                    funcion = new Funcion(declaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), declaracion.ChildNodes.ElementAt(4), declaracion.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim());
                    foreach (ParseTreeNode parametro in declaracion.ChildNodes.ElementAt(3).ChildNodes)
                    {
                        if (!funcion.agregarParametro(parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(parametro.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "PARAMETRO  " + parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " EN FUNCION " + declaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                            error = true;
                            break;
                        }
                    }
                    if (!error)
                    {
                        if (!contextoActual.agregarSimbolo(declaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim().Insert(0, "@"), funcion))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + declaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
            }
        }

        private void declaracionFuncionMain(ParseTreeNode nodoMain)
        {
            if (claseMain == null)
            {
                if (!contextoActual.agregarSimbolo("@main", nodoMain.ChildNodes.ElementAt(0)))
                {
                    errores.Add(new Error("ERROR SEMANTICO", "FUNCION MAIN YA EXISTE", 0, 0));
                }
                else
                {
                    //claseMain = claseActual;
                    claseMain = new Clase(contextoActual.identificadorClase, contextoActual.tablaDeSimbolos);
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "FUNCION MAIN YA HA SIDO DECLARADA", 0, 0));
            }
        }

        private void actualizarMain()
        {
            foreach (Clase c in clases)
            {
                if (c.identificador.Equals(claseMain.identificador))
                {
                    claseMain = c;
                    //claseActual = c;
                    contextoActual = new Contexto(c.identificador, c.tablaDeSimbolos);
                    //aca se deberian de verificar los imports de la clase para agregar los contextos
                    break;
                }
            }
        }

        private void ejecutarMain()
        {
            Contexto temp = contextoActual;
            contextoActual = new Contexto(temp.identificadorClase + ",@main");
            contextoActual.anterior = temp;
            foreach (ParseTreeNode sentencia in ((ParseTreeNode)claseMain.obtenerSimbolo("@main")).ChildNodes)
            {
                switch (sentencia.ToString())
                {
                    case "FUNCION_NATIVA_PRINT": ejecutarFuncionNativaPrint(sentencia);
                        break;
                    case "FUNCION_NATIVA_SHOW": ejecutarFuncionNativaShow(sentencia);
                        break;
                    case "FUNCION_LOCAL": ejecutarFuncionLocalSinRetorno(sentencia);
                        break;
                    case "DECLARACION_VARIABLE": declaracionVariable(sentencia);
                        break;
                    case "ASIGNACION_VARIABLE": asignacionVariable(sentencia);
                        break;
                    case "DECLARACION_ASIGNACION_VARIABLE": declaracionAsignacionVariable(sentencia);
                        break;
                    case "DECLARACION_ARREGLO": declaracionArreglo(sentencia);
                        break;
                    case "DECLARACION_ASIGNACION_ARREGLO": declaracionAsignacionArreglo(sentencia);
                        break;
                    default: Console.WriteLine("SENTECIAS NO MANEJADAS EN MAIN");
                        break;
                }
            }
            contextoActual = temp;
        }

        private void ejecutarFuncionNativaPrint(ParseTreeNode nodoPrint)
        {
            Object obj = calcularValor(nodoPrint.ChildNodes.ElementAt(0));
            if (obj != null)
            {
                Console.WriteLine(obj.ToString());
            }
        }

        private void ejecutarFuncionNativaShow(ParseTreeNode nodoShow)
        {
            Object titulo = calcularValor(nodoShow.ChildNodes.ElementAt(0));
            Object mensaje = calcularValor(nodoShow.ChildNodes.ElementAt(1));
            if (titulo != null && mensaje != null)
            {
                MessageBox.Show(mensaje.ToString(), titulo.ToString());
            }
        }

        private void ejecutarFuncionLocalSinRetorno(ParseTreeNode nodoFuncion)
        {
            Object obj = buscarSimboloEnContexto("@" + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
            if (obj != null)
            {
                Funcion funcion = (Funcion)obj;
                if (funcion.parametros.Count == nodoFuncion.ChildNodes.ElementAt(1).ChildNodes.Count)
                {
                    bool error = false;
                    String tipoDatoParametroLocal = "";
                    for (int i = 0 ; i < nodoFuncion.ChildNodes.ElementAt(1).ChildNodes.Count; i++)
                    {
                        Object parametro = calcularValor(nodoFuncion.ChildNodes.ElementAt(1).ChildNodes.ElementAt(i));
                        if (parametro != null)
                        {
                            tipoDatoParametroLocal = tipoDatoSistema(((Variable)funcion.parametros[i]).tipo);
                            if (tipoDatoParametroLocal.Equals(parametro.GetType().ToString()))
                            {
                                if(!funcion.agregarValorParametro(((Variable)funcion.parametros[i]).identificador, parametro, i))
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO SE ENCONTRO PARAMETRO", 0, 0));
                                    error = true;
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " ESPERABA PARAMETROS DE DISTINTO TIPO", 0, 0));
                                error = true;
                            }
                        }
                    }
                    if (!error)
                    {
                        Contexto temp = contextoActual, nuevoContexto = actualizarContextoFuncionLocal(contextoActual.identificadorClase.Substring(0, contextoActual.identificadorClase.IndexOf(',')));
                        contextoActual = new Contexto(nuevoContexto.identificadorClase + ",@" + funcion.identificador);
                        contextoActual.anterior = nuevoContexto;
                        contextoActual.tablaDeSimbolos = funcion.tablaDeSimbolos;
                        ejecutarFuncionSinRetorno(funcion.sentencias);
                        contextoActual = temp;
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " ESPERABA PARAMETROS", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO HA SIDO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
            }
        }

        private void ejecutarFuncionSinRetorno(ParseTreeNode nodoSentencias)
        {
            foreach (ParseTreeNode sentencia in nodoSentencias.ChildNodes)
            {
                switch (sentencia.ToString())
                {
                    case "FUNCION_NATIVA_PRINT": ejecutarFuncionNativaPrint(sentencia);
                        break;
                    case "FUNCION_NATIVA_SHOW": ejecutarFuncionNativaShow(sentencia);
                        break;
                    case "FUNCION_LOCAL": ejecutarFuncionLocalSinRetorno(sentencia);
                        break;
                    case "DECLARACION_VARIABLE": declaracionVariable(sentencia);
                        break;
                    case "ASIGNACION_VARIABLE": asignacionVariable(sentencia);
                        break;
                    case "DECLARACION_ASIGNACION_VARIABLE": declaracionAsignacionVariable(sentencia);
                        break;
                    case "DECLARACION_ARREGLO": declaracionArreglo(sentencia);
                        break;
                    case "DECLARACION_ASIGNACION_ARREGLO": declaracionAsignacionArreglo(sentencia);
                        break;
                    default: Console.WriteLine("SENTECIAS NO MANEJADAS EN MAIN");
                        break;
                }
            }
        }

        private Object buscarSimboloEnContexto(String key)
        {
            Contexto c = contextoActual;
            Object obj = null;
            while (c != null)
            {
                obj = c.obtenerSimbolo(key);
                if (obj == null)
                    c = c.anterior;
                else
                    return obj;
            }
            return obj;
        }

        private Contexto actualizarContextoFuncionLocal(String identificadorClase)
        {
            foreach (Clase c in clases)
            {
                if (c.identificador.Equals(identificadorClase))
                    return new Contexto(c.identificador, c.tablaDeSimbolos);
            }
            return null;
        }

        private String tipoDatoSistema(String tipoLenguaje)
        {
            switch (tipoLenguaje.ToLower())
            {
                case "int": return "System.Int32";
                case "bool": return "System.Boolean";
                case "char": return "System.Char"; 
                case "string": return "System.String";
                case "double": return "System.Double";
                default: return "";
            }
        }
    }
}
