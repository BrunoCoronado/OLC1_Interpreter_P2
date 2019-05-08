using Irony.Parsing;
using OLC1_Interpreter_P2.sistema.bean;
using OLC1_Interpreter_P2.sistema.graficador;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.analisis
{
    class Interprete : Grammar
    {
        private ParseTreeNode raiz;
        private ArrayList clases;
        private ArrayList errores;
        private Clase claseActual;

        public Interprete()
        {
            raiz = null;
            clases = new ArrayList();
            errores = new ArrayList();
            claseActual = null;
        }

        public bool analizar(String contenido)
        {
            Gramatica gramatica = new Gramatica();
            LanguageData languageData = new LanguageData(gramatica);
            Parser parser = new Parser(languageData);
            ParseTree parseTree = parser.Parse(contenido);

            if (parseTree.Root != null)
            {
                Grafica grafica = new Grafica();
                grafica.graficar(parseTree.Root);
                raiz = parseTree.Root;
                analizarAST();
                if (errores.Count > 0)
                {
                    Reporte reporte = new Reporte();
                    reporte.reporteErrores(errores);
                    return false;
                }
                return true;
            }
            return false;
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
                clases.Add(claseActual);
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
            claseActual = new Clase(identificador);
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
                        if (!claseActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), nodo.ToString().Replace("(identificador)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
                case 3:
                    foreach (ParseTreeNode nodo in declaracionVariable.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        if (!claseActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), nodo.ToString().Replace("(identificador)", "").Trim(), declaracionVariable.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
            }
        }

        private void asignacionVariable(ParseTreeNode asignacionVariable)
        {
            if (!claseActual.actualizarVariable(asignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), asignacionVariable.ChildNodes.ElementAt(1)))
            {
                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + asignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA", 0, 0));
            }
            calcularValor(asignacionVariable.ChildNodes.ElementAt(1));
        }

        private void declaracionAsignacionVariable(ParseTreeNode declaracionAsignacionVariable)
        {
            switch (declaracionAsignacionVariable.ChildNodes.Count)
            {
                case 3:
                    foreach (ParseTreeNode nodo in declaracionAsignacionVariable.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        if (!claseActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionAsignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), nodo.ToString().Replace("(identificador)", "").Trim(), declaracionAsignacionVariable.ChildNodes.ElementAt(2))))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                        calcularValor(declaracionAsignacionVariable.ChildNodes.ElementAt(2));
                    }
                    break;
                case 4:
                    foreach (ParseTreeNode nodo in declaracionAsignacionVariable.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        if (!claseActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionAsignacionVariable.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), nodo.ToString().Replace("(identificador)", "").Trim(), declaracionAsignacionVariable.ChildNodes.ElementAt(3), declaracionAsignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE" + nodo.ToString().Replace("(identificador)", "").Trim() + "YA EXISTE", 0, 0));
                        }
                        calcularValor(declaracionAsignacionVariable.ChildNodes.ElementAt(3));
                    }
                    break;
            }
        }

        private void declaracionArreglo(ParseTreeNode declaracionArreglo)
        {
            switch (declaracionArreglo.ChildNodes.Count)
            {
                case 3:
                    foreach (ParseTreeNode nodo in declaracionArreglo.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        Arreglo arreglo = new Arreglo(nodo.ToString().Replace("(identificador)", "").Trim(), declaracionArreglo.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim());
                        foreach (ParseTreeNode nodoDimension in declaracionArreglo.ChildNodes.ElementAt(2).ChildNodes)
                        {
                            arreglo.agregarDimension(nodoDimension);
                        }
                        if (!claseActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), arreglo))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
                case 4:
                    foreach (ParseTreeNode nodo in declaracionArreglo.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        Arreglo arreglo = new Arreglo(nodo.ToString().Replace("(identificador)", "").Trim(), declaracionArreglo.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), declaracionArreglo.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim());
                        foreach (ParseTreeNode nodoDimension in declaracionArreglo.ChildNodes.ElementAt(3).ChildNodes)
                        {
                            arreglo.agregarDimension(nodoDimension);
                        }
                        if (!claseActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), arreglo))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
            }
        }

        private void declaracionAsignacionArreglo(ParseTreeNode declaracionAsignacionArreglo)
        {
            switch (declaracionAsignacionArreglo.ChildNodes.Count)
            {
                case 4:
                    foreach (ParseTreeNode nodo in declaracionAsignacionArreglo.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        Arreglo arreglo = new Arreglo(nodo.ToString().Replace("(identificador)", "").Trim(), declaracionAsignacionArreglo.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim());
                        foreach (ParseTreeNode nodoDimension in declaracionAsignacionArreglo.ChildNodes.ElementAt(2).ChildNodes)
                        {
                            arreglo.agregarDimension(nodoDimension);
                        }
                        //LLENADO DE VALORES, TERMINAR DESPUES DE REALIZAR EL RECORRIDO DE NODOS E PARA OBTENER UN VALOR
                        switch (arreglo.dimensiones.Count)
                        {
                            case 1:
                                //validar que la cantidad de elementos E sea la misma que el valor de la dimension ingresada calcularDimension(arreglo.getDimension[1])
                                /*for (int i = 0; i < declaracionAsignacionArreglo.ChildNodes.ElementAt(3).ChildNodes.Count ; i++)
                                {
                                    arreglo.agregarValor(i.ToString(), declaracionAsignacionArreglo.ChildNodes.ElementAt(3).ChildNodes.ElementAt(i));
                                }*/
                                break;
                            case 2:
                                break;
                            case 3:
                                break;
                        }
                        if (!claseActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), arreglo))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
                case 5:
                    foreach (ParseTreeNode nodo in declaracionAsignacionArreglo.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        Arreglo arreglo = new Arreglo(nodo.ToString().Replace("(identificador)", "").Trim(), declaracionAsignacionArreglo.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), declaracionAsignacionArreglo.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim());
                        foreach (ParseTreeNode nodoDimension in declaracionAsignacionArreglo.ChildNodes.ElementAt(3).ChildNodes)
                        {
                            arreglo.agregarDimension(nodoDimension);
                        }
                        //LLENADO DE VALORES, TERMINAR DESPUES DE REALIZAR EL RECORRIDO DE NODOS E PARA OBTENER UN VALOR
                        switch (arreglo.dimensiones.Count)
                        {
                            case 1:
                                //validar que la cantidad de elementos E sea la misma que el valor de la dimension ingresada calcularDimension(arreglo.getDimension[1])
                                /*for (int i = 0; i < declaracionAsignacionArreglo.ChildNodes.ElementAt(3).ChildNodes.Count ; i++)
                                {
                                    arreglo.agregarValor(i.ToString(), declaracionAsignacionArreglo.ChildNodes.ElementAt(3).ChildNodes.ElementAt(i));
                                }*/
                                break;
                            case 2:
                                break;
                            case 3:
                                break;
                        }
                        if (!claseActual.agregarSimbolo(nodo.ToString().Replace("(identificador)", "").Trim(), arreglo))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodo.ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
            }
        }

        private Object calcularValor(ParseTreeNode nodoE)
        {
            //return resolverExpresion(nodoE);
            try
            {
                Console.WriteLine(resolverExpresion(nodoE).ToString());
            }
            catch(Exception ex)
            {

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
                        default: Console.WriteLine("CASO NO MANEJADO");
                            return null;
                    }
                    return null;
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
                        default: Console.WriteLine("OPERACION NO MANEJADA");
                            return null;
                    }
            }
            return null;
        }
    }
}
