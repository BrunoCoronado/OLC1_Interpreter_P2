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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OLC1_Interpreter_P2.sistema.analisis
{
    class Interprete : Grammar
    {
        public ParseTreeNode raiz;
        private ArrayList clases;
        public ArrayList errores;
        private Clase claseMain;
        private Contexto contextoActual;
        public String consola;
        public List<Simbolo> simbolos;

        public Interprete()
        {
            raiz = null;
            clases = new ArrayList();
            errores = new ArrayList();
            claseMain = null;
            contextoActual = null;
            simbolos = new List<Simbolo>();
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
                raiz = parseTree.Root;
                analizarAST();
                result = reportarErrores(errores);
                if (actualizarMain())
                    ejecutarMain();
                result = reportarErrores(errores);
                return result;
            }
            reportarErrores(gramatica.errores);
            errores = gramatica.errores;
            return false;
        }

        private Boolean reportarErrores(ArrayList errores)
        {
            if (errores.Count > 0)
            {
                //Reporte reporte = new Reporte();
                //reporte.reporteErrores(errores);
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
                            case "IMPORTAR": definirImports(declaracionClase.ChildNodes.ElementAt(1));
                                break;
                            case "CUERPO_CLASE":
                                capturarContenidoClase(declaracionClase.ChildNodes.ElementAt(1));
                                break;
                        }
                        break;
                    case 3:
                        definirImports(declaracionClase.ChildNodes.ElementAt(1));
                        capturarContenidoClase(declaracionClase.ChildNodes.ElementAt(2));
                        break;
                }
                Clase clase = new Clase(contextoActual.identificadorClase, contextoActual.tablaDeSimbolos, contextoActual.imports);
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
            contextoActual = new Contexto(identificador);
            return true;
        }

        private void definirImports(ParseTreeNode nodoImports)
        {
            foreach (ParseTreeNode import in nodoImports.ChildNodes)
            {
                if (!import.ToString().Replace("(identificador)", "").Trim().Equals(contextoActual.identificadorClase))
                {
                    if (!contextoActual.agregarImport(import.ToString().Replace("(identificador)", "").Trim()))
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "IMPORT " + import.ToString().Replace("(identificador)", "").Trim() + " EN LA CLASE" + contextoActual.identificadorClase + " YA EXISTE", 0, 0));
                        break;
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "IMPORT " + import.ToString().Replace("(identificador)", "").Trim() + " EN LA CLASE" + contextoActual.identificadorClase + " NO PUEDE SER ELLA MISMA", 0, 0));
                    break;
                }
            }
        }

        private void capturarContenidoClase(ParseTreeNode cuerpoClase)
        {
            foreach (ParseTreeNode nodo in cuerpoClase.ChildNodes)
            {
                switch (nodo.ToString())
                {
                    case "DECLARACION_VARIABLE": declaracionVariable(nodo);
                        break;
                    /*case "ASIGNACION_VARIABLE": asignacionVariable(nodo);
                        break;*/
                    case "DECLARACION_ASIGNACION_VARIABLE": declaracionAsignacionVariable(nodo);
                        break;
                    case "DECLARACION_ARREGLO": declaracionArreglo(nodo);
                        break;
                    case "DECLARACION_ASIGNACION_ARREGLO": declaracionAsignacionArreglo(nodo);
                        break;
                    /*case "REASIGNACION_VALOR_ARREGLO": ejecutarReasignacionArreglo(nodo);
                        break;*/
                    case "DECLARACION_FUNCION_VACIA": declaracionFuncionVacia(nodo);
                        break;
                    case "DECLARACION_FUNCION_RETORNO": declaracionFuncionRetorno(nodo);
                        break;
                    case "METODO_MAIN": declaracionFuncionMain(nodo);
                        break;
                    case "DECLARACION_OBJETO": ejecutarDeclaracionObjeto(nodo);
                        break;
                    case "DECLARACION_ASIGNACION_OBJETO": ejecutarDeclaracionAsignacionObjeto(nodo);
                        break;
                    case "REASIGNACION_VARIABLE_OBJETO": ejecutarReasignacionVariableObjeto(nodo);
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
                    tipoDato = tipoDatoSistema(declaracionAsignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim());
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
                    tipoDato = tipoDatoSistema(declaracionAsignacionVariable.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim());
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
            String tipoArreglo = tipoDatoSistema(arreglo.tipo);
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
                                            arreglo.agregarValor(i.ToString() + j.ToString(), valor);
                                        }
                                        else
                                        {
                                            errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO NO VALIDO PARA EL ARREGLO " + arreglo.identificador, 0, 0));
                                            return null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                                return null;
                            }
                        }
                        return arreglo;
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
                                                    arreglo.agregarValor(i.ToString() + j.ToString() + k.ToString(), valor);
                                                }
                                                else
                                                {
                                                    errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO NO VALIDO PARA EL ARREGLO " + arreglo.identificador, 0, 0));
                                                    return null;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "CANTIDAD DE ELEMENTOS NO IGUAL AL TAMAÑO DEL ARREGLO " + arreglo.identificador, 0, 0));
                                return null;
                            }
                        }
                        return arreglo;
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
                    if (nodoE.ChildNodes.ElementAt(0).ToString().Equals("ACCESO_VARIABLE_OBJETO"))
                    {
                        return ejecutarAccesoObjetoVariable(nodoE.ChildNodes.ElementAt(0));
                    }
                    else if (nodoE.ChildNodes.ElementAt(0).ToString().Equals("ACCESO_FUNCION_OBJETO"))
                    {
                        ParseTreeNode acceso = nodoE.ChildNodes.ElementAt(0);
                        Object obj = buscarSimboloEnContexto(acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
                        if (obj != null)
                        {
                            if (((Variable)obj).valor != null)
                            {
                                Object func = ((Clase)((Variable)obj).valor).obtenerSimbolo("@" + acceso.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim());
                                if (func != null)
                                {
                                    Funcion funcion = (Funcion)func;
                                    if (funcion.retorno != null)
                                    {
                                        return ejecutarAccesoObjetoFuncionConRetorno(funcion.sentencias, funcion.clasePadre);
                                    }
                                    else
                                    {
                                        errores.Add(new Error("ERROR SEMANTICO", "FUNCION  " + funcion.identificador + " EN VARIABLE " + acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO TIENE RETORNO", 0, 0));
                                        return null;
                                    }
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO TIENE ELEMENTO " + acceso.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), 0, 0));
                                    return null;
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " SIN VALOR ASIGNADO", 0, 0));
                                return null;
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
                            return null;
                        }
                        return null;
                    }
                    else
                    {
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
                                if (((Variable)obj).valor == null)
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + valor + " SIN VALOR ASIGNADO", 0, 0));
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
                    }
                case 2:
                    if (nodoE.ChildNodes.ElementAt(1).ToString().Equals("E"))
                    {
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
                    }
                    else if (nodoE.ChildNodes.ElementAt(0).ToString().Equals("E"))
                    {
                        Object valOP = resolverExpresion(nodoE.ChildNodes.ElementAt(0));
                        switch (nodoE.ChildNodes.ElementAt(1).ToString().Replace("(Key symbol)", "").Trim())
                        {
                            case "++":
                                switch (valOP.GetType().ToString())
                                {
                                    case "System.String": errores.Add(new Error("ERROR SEMANTICO", "AUMENTO NO SOPORTA STRING", 0, 0));
                                        return null;
                                    case "System.Char": return ((int)Char.Parse(valOP.ToString())) + 1;
                                    case "System.Double": return Double.Parse((Double.Parse(valOP.ToString()) + 1).ToString());
                                    case "System.Int32": return Int32.Parse((Int32.Parse(valOP.ToString())  + 1).ToString());
                                    case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "AUMENTO NO SOPORTA CHAR", 0, 0));
                                        return null;
                                    default: return null;
                                }
                            case "--":
                                switch (valOP.GetType().ToString())
                                {
                                    case "System.String": errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO NO SOPORTA STRING", 0, 0));
                                        return null;
                                    case "System.Char": return ((int)Char.Parse(valOP.ToString())) - 1;
                                    case "System.Double": return Double.Parse((Double.Parse(valOP.ToString()) - 1).ToString());
                                    case "System.Int32": return Int32.Parse((Int32.Parse(valOP.ToString()) - 1).ToString());
                                    case "System.Boolean": errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO NO SOPORTA CHAR", 0, 0));
                                        return null;
                                    default: return null;
                                }
                            default:
                                Console.WriteLine("CASO NO MANEJADO");
                                return null;
                        }
                    }
                    else if (nodoE.ChildNodes.ElementAt(1).ToString().Equals("LISTA_PARAMETROS_LLAMADA"))
                    {
                        return ejecutarFuncionLocalConRetorno(nodoE);
                    }
                    else
                    {
                        String identificadorArreglo = nodoE.ChildNodes.ElementAt(0).ToString().Substring(0, (nodoE.ChildNodes.ElementAt(0).ToString().Length - nodoE.ChildNodes.ElementAt(0).ToString().Substring(nodoE.ChildNodes.ElementAt(0).ToString().LastIndexOf('(') - 1).Length));
                        Object obj = buscarSimboloEnContexto(identificadorArreglo);
                        if (obj != null)
                        {
                            Arreglo arreglo = (Arreglo)obj;
                            if (arreglo.valores.Count > 0)
                            {
                                if (arreglo.dimensiones.Count == nodoE.ChildNodes.ElementAt(1).ChildNodes.Count)
                                {
                                    String key = "";
                                    for (int i = 0 ; i < arreglo.dimensiones.Count ; i++)
                                    {
                                        ParseTreeNode posicion = nodoE.ChildNodes.ElementAt(1).ChildNodes.ElementAt(i);
                                        Object pos = calcularValor(posicion);
                                        if (pos.GetType().ToString().Equals("System.Int32"))
                                        {
                                            if (Int32.Parse(pos.ToString()) < Int32.Parse(arreglo.dimensiones[i].ToString()))
                                            {
                                                key += pos.ToString();
                                            }
                                            else
                                            {
                                                errores.Add(new Error("ERROR SEMANTICO", "POSICION EN ARREGLO " + identificadorArreglo + " FUERA DE RANGO", 0, 0));
                                                return null;
                                            }
                                        }
                                        else
                                        {
                                            errores.Add(new Error("ERROR SEMANTICO", "POSICION EN ARREGLO " + identificadorArreglo + " CON VALOR DISTINTO A INT", 0, 0));
                                            return null;
                                        }
                                    }
                                    return arreglo.obtenerValor(key);
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "ARREGLO " + identificadorArreglo + " SIN VALORES ASIGNADOS", 0, 0));
                                return null;
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "ARREGLO " + identificadorArreglo + " NO DECLARADO EN EL CONTEXTO ACTUAL", 0, 0));
                            return null;
                        }
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
                    funcion.clasePadre = contextoActual.identificadorClase;
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
                        funcion.clasePadre = contextoActual.identificadorClase;
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
                        funcion.clasePadre = contextoActual.identificadorClase;
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
                    funcion.clasePadre = contextoActual.identificadorClase;
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

        private void declaracionFuncionRetorno(ParseTreeNode nodoDeclaracion)
        {
            Funcion funcion = null;
            Boolean error = false;
            switch (nodoDeclaracion.ChildNodes.Count)
            {
                case 4:
                    funcion = new Funcion(nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), nodoDeclaracion.ChildNodes.ElementAt(3));
                    funcion.clasePadre = contextoActual.identificadorClase;
                    foreach (ParseTreeNode parametro in nodoDeclaracion.ChildNodes.ElementAt(2).ChildNodes)
                    {
                        if (!funcion.agregarParametro(parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(parametro.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "PARAMETRO " + parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " EN FUNCION " + nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                            error = true;
                            break;
                        }
                    }
                    if (!error)
                    {
                        if (!contextoActual.agregarSimbolo(funcion.identificador.Insert(0, "@"), funcion))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + funcion.identificador + " YA EXISTE", 0, 0));
                        }
                    }
                    break;
                case 5:
                    if (nodoDeclaracion.ChildNodes.ElementAt(2).ToString().ToLower().Contains("override (keyword)"))
                    {
                        funcion = new Funcion(nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), nodoDeclaracion.ChildNodes.ElementAt(4));
                        funcion.clasePadre = contextoActual.identificadorClase;
                        foreach (ParseTreeNode parametro in nodoDeclaracion.ChildNodes.ElementAt(3).ChildNodes)
                        {
                            if (!funcion.agregarParametro(parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(parametro.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "PARAMETRO " + parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " EN FUNCION " + funcion.identificador + " YA EXISTE", 0, 0));
                                error = true;
                                break;
                            }
                        }
                        if (!error)
                        {
                            if (!contextoActual.agregarSimbolo(funcion.identificador.Insert(0, "@"), funcion))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + funcion.identificador + " YA EXISTE", 0, 0));
                            }
                        }
                    }
                    else
                    {
                        funcion = new Funcion(nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), nodoDeclaracion.ChildNodes.ElementAt(2).ToString().Replace("(Keyword)", "").Trim() , nodoDeclaracion.ChildNodes.ElementAt(4), nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim());
                        funcion.clasePadre = contextoActual.identificadorClase;
                        foreach (ParseTreeNode parametro in nodoDeclaracion.ChildNodes.ElementAt(3).ChildNodes)
                        {
                            if (!funcion.agregarParametro(parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(parametro.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                            {
                                errores.Add(new Error("1ERROR SEMANTICO", "PARAMETRO  " + parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " EN FUNCION " + nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                                error = true;
                                break;
                            }
                        }
                        if (!error)
                        {
                            if (!contextoActual.agregarSimbolo(funcion.identificador.Insert(0, "@"), funcion))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + funcion.identificador + " YA EXISTE", 0, 0));
                            }
                        }
                    }
                    break;
                case 6:
                    funcion = new Funcion(nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), nodoDeclaracion.ChildNodes.ElementAt(2).ToString().Replace("(Keyword)", "").Trim(), nodoDeclaracion.ChildNodes.ElementAt(5), nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim());
                    funcion.clasePadre = contextoActual.identificadorClase;
                    foreach (ParseTreeNode parametro in nodoDeclaracion.ChildNodes.ElementAt(4).ChildNodes)
                    {
                        if (!funcion.agregarParametro(parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(parametro.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "PARAMETRO  " + parametro.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " EN FUNCION " + nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                            error = true;
                            break;
                        }
                    }
                    if (!error)
                    {
                        if (!contextoActual.agregarSimbolo(funcion.identificador.Insert(0, "@"), funcion))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + funcion.identificador + " YA EXISTE", 0, 0));
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

        private Boolean actualizarMain()
        {
            if (claseMain != null)
            {
                foreach (Clase c in clases)
                {
                    if (c.identificador.Equals(claseMain.identificador))
                    {
                        claseMain = c;
                        contextoActual = new Contexto(c.identificador, c.tablaDeSimbolos);
                        return agregarImports(claseMain.imports);
                    }
                }
            }
            errores.Add(new Error("ERROR SEMANTICO", "NO SE ENCONTRO MAIN PARA INICIAR EJECUCION", 0, 0));
            return false;
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
                    case "FUNCION_NATIVA_WHILE": ejecutarFuncionNativaWhile(sentencia);
                        break;
                    case "FUNCION_NATIVA_REPEAT": ejecutarFuncionNativaRepeat(sentencia);
                        break;
                    case "FUNCION_NATIVA_DO_WHILE": ejecutarFuncionNativaDoWhile(sentencia);
                        break;
                    case "FUNCION_NATIVA_FOR": ejecutarFuncionNativaFor(sentencia);
                        break;
                    case "FUNCION_NATIVA_IF": ejecutarFuncionNativaIf(sentencia);
                        break;
                    case "FUNCION_NATIVA_COMPROBAR": ejecutarFuncionNativaComprobar(sentencia);
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
                    case "REASIGNACION_VALOR_ARREGLO": ejecutarReasignacionArreglo(sentencia);
                        break;
                    case "AUMENTO_DECREMENTO": ejecutarAumentoDecremento(sentencia);
                        break;
                    case "DECLARACION_OBJETO": ejecutarDeclaracionObjeto(sentencia);
                        break;
                    case "ASIGNACION_OBJETO": ejecutarAsignacionObjeto(sentencia);
                        break;
                    case "DECLARACION_ASIGNACION_OBJETO": ejecutarDeclaracionAsignacionObjeto(sentencia);
                        break;
                    case "ACCESO_FUNCION_OBJETO": ejecutarAccesoObjetoFuncionSinRetorno(sentencia);
                        break;
                    case "REASIGNACION_VARIABLE_OBJETO": ejecutarReasignacionVariableObjeto(sentencia);
                        break;
                    case "FUNCION_NATIVA_ADDFIGURE": ejecutarAddFigure(sentencia);
                        break;
                    case "FUNCION_NATIVA_FIGURE": ejecutarFigure(sentencia);
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
                consola += obj.ToString() + "\n";
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

        private void ejecutarFuncionNativaWhile(ParseTreeNode nodoWhile)
        {
            Contexto tmp = contextoActual;
            Object valor = calcularValor(nodoWhile.ChildNodes.ElementAt(0));
            if (valor != null)
            {
                if (valor.GetType().ToString().Equals("System.Boolean"))
                {
                    Boolean condicion = Boolean.Parse(valor.ToString());
                    while (condicion)
                    {
                        contextoActual = new Contexto(tmp.identificadorClase + ",@while");
                        contextoActual.anterior = tmp;
                        if (!ejecutarSentenciasBucle(nodoWhile.ChildNodes.ElementAt(1)))
                        {
                            condicion = Boolean.Parse(calcularValor(nodoWhile.ChildNodes.ElementAt(0)).ToString());
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "CONDICION CICLO WHILE NO ES DEL TIPO BOOL", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "CONDICION CICLO WHILE NO ES DEL TIPO BOOL", 0, 0));
            }
            contextoActual = tmp;
        }

        private void ejecutarFuncionNativaRepeat(ParseTreeNode nodoRepeat)
        {
            Contexto tmp = contextoActual;
            Object valor = calcularValor(nodoRepeat.ChildNodes.ElementAt(0));
            if (valor != null)
            {
                if (valor.GetType().ToString().Equals("System.Int32"))
                {
                    for (int i = 0 ; i < (int)valor; i++)
                    {
                        contextoActual = new Contexto(tmp.identificadorClase + ",@repeat");
                        contextoActual.anterior = tmp;
                        if (ejecutarSentenciasBucle(nodoRepeat.ChildNodes.ElementAt(1)))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "CONDICION CICLO REPEAT NO ES DEL TIPO INT", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "CONDICION CICLO WHILE NO ES DEL TIPO BOOL", 0, 0));
            }
            contextoActual = tmp;
        }

        private void ejecutarFuncionNativaFor(ParseTreeNode nodoFor)
        {
            ParseTreeNode declaracionesFor = nodoFor.ChildNodes.ElementAt(0);
            Object valor = null, obj = null, valorCondicion = null;
            String tipoDato = "";
            Contexto c = null, tmp = contextoActual;
            contextoActual = new Contexto(tmp.identificadorClase + ",@for");
            contextoActual.anterior = tmp;
            c = contextoActual;
            tmp = contextoActual;
            bool errorDeclaracionesFor = false;
            switch (declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.Count)
            {
                case 2:
                    while (c != null)
                    {
                        obj = c.obtenerSimbolo(declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
                        if (obj == null)
                            c = c.anterior;
                        else
                            break;
                    }
                    if (obj != null)
                    {
                        Variable variable = (Variable)obj;
                        tipoDato = tipoDatoSistema(variable.tipo);
                        valor = calcularValor(declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1));
                        if (valor != null)
                        {
                            if (valor.GetType().ToString().Equals(tipoDato))
                            {
                                variable.valor = valor;
                                if (!c.actualizarSimbolo(declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), variable))
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA", 0, 0));
                                    errorDeclaracionesFor = true;
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO PARA VARIABLE " + variable.identificador + " INVALIDO", 0, 0));
                                errorDeclaracionesFor = true;
                            }
                        }
                        else
                        {
                            errorDeclaracionesFor = true;
                        }
                    }
                    else
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
                        errorDeclaracionesFor = true;
                    }
                    break;
                case 3:
                    tipoDato = tipoDatoSistema(declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim());
                    valor = calcularValor(declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(2));
                    if (valor != null)
                    {   
                        if (valor.GetType().ToString().Equals(tipoDato))
                        {
                            if (!contextoActual.agregarSimbolo(declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), valor)))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE PARA DECLARACION CICLO FOR", 0, 0));
                                errorDeclaracionesFor = true;
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO PARA VARIABLE " + declaracionesFor.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " INVALIDO EN DECLARACION CICLO FOR", 0, 0));
                            errorDeclaracionesFor = true;
                        }
                    }
                    else
                    {
                        errorDeclaracionesFor = true;
                    }
                    break;
            }
            valorCondicion = calcularValor(declaracionesFor.ChildNodes.ElementAt(1));
            if (!valorCondicion.GetType().ToString().Equals("System.Boolean"))
                errorDeclaracionesFor = true;
            if (!errorDeclaracionesFor)
            {
                Boolean condicion = Boolean.Parse(valorCondicion.ToString());
                while (condicion)
                {
                    contextoActual = new Contexto(tmp.identificadorClase + ",@for-iteracion");
                    contextoActual.anterior = tmp;
                    c = contextoActual;
                    bool errorActualizacionFor = false;
                    if (!ejecutarSentenciasBucle(nodoFor.ChildNodes.ElementAt(1)))
                    {
                        while (c != null)
                        {
                            obj = c.obtenerSimbolo(declaracionesFor.ChildNodes.ElementAt(2).ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
                            if (obj == null)
                                c = c.anterior;
                            else
                                break;
                        }
                        if (obj != null)
                        {
                            Variable variable = (Variable)obj;
                            switch (declaracionesFor.ChildNodes.ElementAt(2).ChildNodes.ElementAt(1).ToString().Replace("(Key symbol)", "").Trim())
                            {
                                case "++":
                                    switch (variable.valor.GetType().ToString())
                                    {
                                        case "System.String":
                                            errores.Add(new Error("ERROR SEMANTICO", "AUMENTO DENTRO DE FOR NO SOPORTA STRING", 0, 0));
                                            errorActualizacionFor = true;
                                            break;
                                        case "System.Char":
                                            errores.Add(new Error("ERROR SEMANTICO", "AUMENTO DENTRO DE FOR NO SOPORTA CHAR", 0, 0));
                                            errorActualizacionFor = true;
                                            break;
                                        case "System.Double":
                                            variable.valor = (Double.Parse(variable.valor.ToString())) + 1;
                                            if (!c.actualizarSimbolo(variable.identificador, variable))
                                            {
                                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + variable.identificador + " NO DECLARADA EN FOR", 0, 0));
                                                errorActualizacionFor = true;
                                            }
                                            break;
                                        case "System.Int32":
                                            variable.valor = (Int32.Parse(variable.valor.ToString())) + 1;
                                            if (!c.actualizarSimbolo(variable.identificador, variable))
                                            {
                                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + variable.identificador + " NO DECLARADA EN FOR", 0, 0));
                                                errorActualizacionFor = true;
                                            }
                                            break;
                                        case "System.Boolean":
                                            errores.Add(new Error("ERROR SEMANTICO", "AUMENTO DENTRO DE FOR NO SOPORTA BOOL", 0, 0));
                                            errorActualizacionFor = true;
                                            break;
                                    }
                                    break;
                                case "--":
                                    switch (variable.valor.GetType().ToString())
                                    {
                                        case "System.String":
                                            errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO DENTRO DE FOR NO SOPORTA STRING", 0, 0));
                                            errorActualizacionFor = true;
                                            break;
                                        case "System.Char":
                                            errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO DENTRO DE FOR NO SOPORTA CHAR", 0, 0));
                                            errorActualizacionFor = true;
                                            break;
                                        case "System.Double":
                                            variable.valor = (Double.Parse(variable.valor.ToString())) - 1;
                                            if (!c.actualizarSimbolo(variable.identificador, variable))
                                            {
                                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + variable.identificador + " NO DECLARADA EN FOR", 0, 0));
                                                errorActualizacionFor = true;
                                            }
                                            break;
                                        case "System.Int32":
                                            variable.valor = (Int32.Parse(variable.valor.ToString())) - 1;
                                            if (!c.actualizarSimbolo(variable.identificador, variable))
                                            {
                                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + variable.identificador + " NO DECLARADA EN FOR", 0, 0));
                                                errorActualizacionFor = true;
                                            }
                                            break;
                                        case "System.Boolean":
                                            errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO DENTRO DE FOR NO SOPORTA BOOL", 0, 0));
                                            errorActualizacionFor = true;
                                            break;
                                    }
                                    break;
                            }
                            condicion = Boolean.Parse(calcularValor(declaracionesFor.ChildNodes.ElementAt(1)).ToString());
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "ERROR AL ACTUALIZAR FOR", 0, 0));
                        }
                    }
                    else
                    {
                        break;
                    }
                    if (errorActualizacionFor)
                    {
                        break;
                    }
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "ERROR EN LAS DECLARACIONES DEL FOR ", 0, 0));
            }
        }

        private void ejecutarFuncionNativaDoWhile(ParseTreeNode nodoHacer)
        {
            Contexto tmp = contextoActual;
            Object valor = calcularValor(nodoHacer.ChildNodes.ElementAt(1));
            if (valor != null)
            {
                if (valor.GetType().ToString().Equals("System.Boolean"))
                {
                    Boolean condicion = Boolean.Parse(valor.ToString());
                    do
                    {
                        contextoActual = new Contexto(tmp.identificadorClase + ",@hacer");
                        contextoActual.anterior = tmp;
                        if (!ejecutarSentenciasBucle(nodoHacer.ChildNodes.ElementAt(0)))
                        {
                            condicion = Boolean.Parse(calcularValor(nodoHacer.ChildNodes.ElementAt(1)).ToString());
                        }
                        else
                        {
                            break;
                        }
                    } while (condicion);
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "CONDICION CICLO HACER-MIENTRAS NO ES DEL TIPO BOOL", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "CONDICION CICLO WHILE NO ES DEL TIPO BOOL", 0, 0));
            }
            contextoActual = tmp;
        }

        private void ejecutarFuncionNativaComprobar(ParseTreeNode nodoComprobar)
        {
            Contexto tmp = contextoActual;
            Object valCaso = null, condicion = calcularValor(nodoComprobar.ChildNodes.ElementAt(0));
            bool salir = false;
            if (condicion != null)
            {
                switch (nodoComprobar.ChildNodes.Count)
                {
                    case 2:
                        foreach (ParseTreeNode caso in nodoComprobar.ChildNodes.ElementAt(1).ChildNodes)
                        {
                            valCaso = calcularValor(caso.ChildNodes.ElementAt(0));
                            if (valCaso != null)
                            {
                                if (condicion.GetType().ToString().Equals(valCaso.GetType().ToString()))
                                {
                                    if (valCaso.Equals(condicion))
                                    {
                                        salir = ejecutarSentenciasBucle(caso.ChildNodes.ElementAt(1));
                                        if (salir)
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO EN CASO DISTINTO A LA CONDICION", 0, 0));
                                    break;
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO EN CASO NULO", 0, 0));
                                break;
                            }
                        }
                        break;
                    case 3:
                        bool error = false;
                        foreach (ParseTreeNode caso in nodoComprobar.ChildNodes.ElementAt(1).ChildNodes)
                        {
                            valCaso = calcularValor(caso.ChildNodes.ElementAt(0));
                            if (valCaso != null)
                            {
                                if (condicion.GetType().ToString().Equals(valCaso.GetType().ToString()))
                                {
                                    if (valCaso.Equals(condicion))
                                    {
                                        salir = ejecutarSentenciasBucle(caso.ChildNodes.ElementAt(1));
                                        if (salir)
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO EN CASO DISTINTO A LA CONDICION", 0, 0));
                                    error = true;
                                    break;
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO EN CASO NULO", 0, 0));
                                error = true;
                                break;
                            }
                        }
                        if (!(salir | error))
                        {
                            ejecutarSentenciasBucle(nodoComprobar.ChildNodes.ElementAt(2).ChildNodes.ElementAt(0));
                        }
                        break;
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "CONDICION COMPROBAR NULA", 0, 0));
            }
            contextoActual = tmp;
        }

        private void ejecutarFuncionNativaIf(ParseTreeNode nodoIf)
        {
            Object valor;
            Contexto tmp = contextoActual;
            switch (nodoIf.ChildNodes.Count)
            {
                case 1:
                    valor = calcularValor(nodoIf.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0));
                    if (valor != null)
                    {
                        if (valor.GetType().ToString().Equals("System.Boolean"))
                        {
                            if (Boolean.Parse(valor.ToString()))
                            {
                                contextoActual = new Contexto(tmp.identificadorClase + ",@if");
                                contextoActual.anterior = tmp;
                                ejecutarSentenciasBucle(nodoIf.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1));
                            }   
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                        }
                    }
                    else
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                    }
                    break;
                case 2:
                    switch (nodoIf.ChildNodes.ElementAt(1).ToString())
                    {
                        case "CONDIDICIONES_ELSE_IF":
                            valor = calcularValor(nodoIf.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0));
                            if (valor != null)
                            {
                                if (valor.GetType().ToString().Equals("System.Boolean"))
                                {
                                    if (Boolean.Parse(valor.ToString()))
                                    {
                                        contextoActual = new Contexto(tmp.identificadorClase + ",@if");
                                        contextoActual.anterior = tmp;
                                        ejecutarSentenciasBucle(nodoIf.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1));
                                    }
                                    else
                                    {
                                        foreach (ParseTreeNode nodoElseIf in nodoIf.ChildNodes.ElementAt(1).ChildNodes)
                                        {
                                            valor = calcularValor(nodoElseIf.ChildNodes.ElementAt(0));
                                            if (valor != null)
                                            {
                                                if (valor.GetType().ToString().Equals("System.Boolean"))
                                                {
                                                    if (Boolean.Parse(valor.ToString()))
                                                    {
                                                        contextoActual = new Contexto(tmp.identificadorClase + ",@else-if");
                                                        contextoActual.anterior = tmp;
                                                        ejecutarSentenciasBucle(nodoElseIf.ChildNodes.ElementAt(1));
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA ELSE-IF NO ES DEL TIPO BOOL", 0, 0));
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                            }
                            break;
                        case "CONDICION_ELSE":
                            valor = calcularValor(nodoIf.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0));
                            if (valor != null)
                            {
                                if (valor.GetType().ToString().Equals("System.Boolean"))
                                {
                                    if (Boolean.Parse(valor.ToString()))
                                    {
                                        contextoActual = new Contexto(tmp.identificadorClase + ",@if");
                                        contextoActual.anterior = tmp;
                                        ejecutarSentenciasBucle(nodoIf.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1));
                                    }
                                    else
                                    {
                                        contextoActual = new Contexto(tmp.identificadorClase + ",@else");
                                        contextoActual.anterior = tmp;
                                        ejecutarSentenciasBucle(nodoIf.ChildNodes.ElementAt(1).ChildNodes.ElementAt(0));
                                    }
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                            }
                            break;
                    }
                    break;
                case 3:
                    valor = calcularValor(nodoIf.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0));
                    if (valor != null)
                    {
                        if (valor.GetType().ToString().Equals("System.Boolean"))
                        {   
                            if (Boolean.Parse(valor.ToString()))
                            {
                                contextoActual = new Contexto(tmp.identificadorClase + ",@if");
                                contextoActual.anterior = tmp;
                                ejecutarSentenciasBucle(nodoIf.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1));
                            }
                            else
                            {
                                bool elseIfEjecutado = false;
                                foreach (ParseTreeNode nodoElseIf in nodoIf.ChildNodes.ElementAt(1).ChildNodes)
                                {
                                    valor = calcularValor(nodoElseIf.ChildNodes.ElementAt(0));
                                    if (valor != null)
                                    {
                                        if (valor.GetType().ToString().Equals("System.Boolean"))
                                        {
                                            if (Boolean.Parse(valor.ToString()))
                                            {
                                                contextoActual = new Contexto(tmp.identificadorClase + ",@else-if");
                                                contextoActual.anterior = tmp;
                                                ejecutarSentenciasBucle(nodoElseIf.ChildNodes.ElementAt(1));
                                                elseIfEjecutado = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA ELSE-IF NO ES DEL TIPO BOOL", 0, 0));
                                            elseIfEjecutado = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                                        elseIfEjecutado = true;
                                        break;
                                    }
                                }
                                if (!elseIfEjecutado)
                                {
                                    contextoActual = new Contexto(tmp.identificadorClase + ",@else");
                                    contextoActual.anterior = tmp;
                                    ejecutarSentenciasBucle(nodoIf.ChildNodes.ElementAt(2).ChildNodes.ElementAt(0));
                                }
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                        }
                    }
                    else
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "CONDICION EN SENTENCIA IF NO ES DEL TIPO BOOL", 0, 0));
                    }
                    break;
            }
            contextoActual = tmp;
        }

        private Boolean ejecutarSentenciasBucle(ParseTreeNode nodoSentencias)
        {
            for (int i = 0 ; i < nodoSentencias.ChildNodes.Count ; i++)
            {
                ParseTreeNode sentencia = nodoSentencias.ChildNodes.ElementAt(i);
                switch (sentencia.ToString())
                {
                    case "SENTENCIA_CONTINUAR": i = nodoSentencias.ChildNodes.Count;
                        break;
                    case "SENTENCIA_SALIR":
                        return true;
                    case "FUNCION_NATIVA_PRINT": ejecutarFuncionNativaPrint(sentencia);
                        break;
                    case "FUNCION_NATIVA_SHOW": ejecutarFuncionNativaShow(sentencia);
                        break;
                    case "FUNCION_NATIVA_WHILE": ejecutarFuncionNativaWhile(sentencia);
                        break;
                    case "FUNCION_NATIVA_REPEAT": ejecutarFuncionNativaRepeat(sentencia);
                        break;
                    case "FUNCION_NATIVA_FOR": ejecutarFuncionNativaFor(sentencia);
                        break;
                    case "FUNCION_NATIVA_DO_WHILE": ejecutarFuncionNativaDoWhile(sentencia);
                        break;
                    case "FUNCION_NATIVA_IF": ejecutarFuncionNativaIf(sentencia);
                        break;
                    case "FUNCION_NATIVA_COMPROBAR": ejecutarFuncionNativaComprobar(sentencia);
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
                    case "AUMENTO_DECREMENTO": ejecutarAumentoDecremento(sentencia);
                        break;
                    case "DECLARACION_OBJETO": ejecutarDeclaracionObjeto(sentencia);
                        break;
                    case "ASIGNACION_OBJETO": ejecutarAsignacionObjeto(sentencia);
                        break;
                    case "DECLARACION_ASIGNACION_OBJETO": ejecutarDeclaracionAsignacionObjeto(sentencia);
                        break;
                    case "ACCESO_FUNCION_OBJETO": ejecutarAccesoObjetoFuncionSinRetorno(sentencia);
                        break;
                    case "REASIGNACION_VARIABLE_OBJETO": ejecutarReasignacionVariableObjeto(sentencia);
                        break;
                    case "FUNCION_NATIVA_ADDFIGURE": ejecutarAddFigure(sentencia);
                        break;
                    case "FUNCION_NATIVA_FIGURE": ejecutarFigure(sentencia);
                        break;
                    default: Console.WriteLine("SENTECIAS NO MANEJADAS EN MAIN");
                        break;
                }
            }
            return false;
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
                    case "FUNCION_NATIVA_WHILE": ejecutarFuncionNativaWhile(sentencia);
                        break;
                    case "FUNCION_NATIVA_REPEAT": ejecutarFuncionNativaRepeat(sentencia);
                        break;
                    case "FUNCION_NATIVA_DO_WHILE": ejecutarFuncionNativaDoWhile(sentencia);
                        break;
                    case "FUNCION_NATIVA_FOR": ejecutarFuncionNativaFor(sentencia);
                        break;
                    case "FUNCION_NATIVA_IF": ejecutarFuncionNativaIf(sentencia);
                        break;
                    case "FUNCION_NATIVA_COMPROBAR": ejecutarFuncionNativaComprobar(sentencia);
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
                    case "REASIGNACION_VALOR_ARREGLO": ejecutarReasignacionArreglo(sentencia);
                        break;
                    case "AUMENTO_DECREMENTO": ejecutarAumentoDecremento(sentencia);
                        break;
                    case "DECLARACION_OBJETO": ejecutarDeclaracionObjeto(sentencia);
                        break;
                    case "ASIGNACION_OBJETO": ejecutarAsignacionObjeto(sentencia);
                        break;
                    case "DECLARACION_ASIGNACION_OBJETO": ejecutarDeclaracionAsignacionObjeto(sentencia);
                        break;
                    case "ACCESO_FUNCION_OBJETO": ejecutarAccesoObjetoFuncionSinRetorno(sentencia);
                        break;
                    case "REASIGNACION_VARIABLE_OBJETO": ejecutarReasignacionVariableObjeto(sentencia);
                        break;
                    case "FUNCION_NATIVA_ADDFIGURE": ejecutarAddFigure(sentencia);
                        break;
                    case "FUNCION_NATIVA_FIGURE": ejecutarFigure(sentencia);
                        break;
                    default: Console.WriteLine("SENTECIAS NO MANEJADAS EN FUNCION VACIA");
                        break;
                }
            }
        }

        private Object ejecutarFuncionLocalConRetorno(ParseTreeNode nodoFuncion)
        {
            Object obj = buscarSimboloEnContexto("@" + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
            if (obj != null)
            {
                Funcion funcion = (Funcion)obj;
                if (funcion.parametros.Count == nodoFuncion.ChildNodes.ElementAt(1).ChildNodes.Count)
                {
                    String tipoDatoParametroLocal = "";
                    for (int i = 0; i < nodoFuncion.ChildNodes.ElementAt(1).ChildNodes.Count; i++)
                    {
                        Object parametro = calcularValor(nodoFuncion.ChildNodes.ElementAt(1).ChildNodes.ElementAt(i));
                        if (parametro != null)
                        {
                            tipoDatoParametroLocal = tipoDatoSistema(((Variable)funcion.parametros[i]).tipo);
                            if (tipoDatoParametroLocal.Equals(parametro.GetType().ToString()))
                            {
                                if (!funcion.agregarValorParametro(((Variable)funcion.parametros[i]).identificador, parametro, i))
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO SE ENCONTRO PARAMETRO", 0, 0));
                                    return null;
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " ESPERABA PARAMETROS DE DISTINTO TIPO", 0, 0));
                                return null;
                            }
                        }
                    }
                    Contexto temp = contextoActual, nuevoContexto = actualizarContextoFuncionLocal(contextoActual.identificadorClase.Substring(0, contextoActual.identificadorClase.IndexOf(',')));
                    contextoActual = new Contexto(nuevoContexto.identificadorClase + ",@" + funcion.identificador);
                    contextoActual.anterior = nuevoContexto;
                    contextoActual.tablaDeSimbolos = funcion.tablaDeSimbolos;
                    Object result = ejecutarFuncionConRetorno(funcion.sentencias);
                    if (result != null)
                    {
                        if (!result.GetType().ToString().Equals(tipoDatoSistema(funcion.retorno)))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + funcion.identificador + " ESPERABA DISTINTO TIPO DE DATO EN RETORNO", 0, 0));
                            result = null;
                        }
                    }
                    contextoActual = temp;
                    return result;
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " ESPERABA PARAMETROS", 0, 0));
                    return null;
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + nodoFuncion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO HA SIDO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
                return null;
            }
        }

        private Object ejecutarFuncionConRetorno(ParseTreeNode nodoSentencias)
        {
            foreach (ParseTreeNode sentencia in nodoSentencias.ChildNodes)
            {
                switch (sentencia.ToString())
                {
                    case "FUNCION_NATIVA_PRINT": ejecutarFuncionNativaPrint(sentencia);
                        break;
                    case "FUNCION_NATIVA_SHOW": ejecutarFuncionNativaShow(sentencia);
                        break;
                    case "FUNCION_NATIVA_REPEAT": ejecutarFuncionNativaRepeat(sentencia);
                        break;
                    case "FUNCION_NATIVA_DO_WHILE": ejecutarFuncionNativaDoWhile(sentencia);
                        break;
                    case "FUNCION_NATIVA_WHILE": ejecutarFuncionNativaWhile(sentencia);//podria retornar valor
                        break;
                    case "FUNCION_NATIVA_FOR": ejecutarFuncionNativaFor(sentencia);//podria retornar valor
                        break;
                    case "FUNCION_NATIVA_IF": ejecutarFuncionNativaIf(sentencia);//podria retornar valor
                        break;
                    case "FUNCION_NATIVA_COMPROBAR": ejecutarFuncionNativaComprobar(sentencia);
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
                    case "REASIGNACION_VALOR_ARREGLO": ejecutarReasignacionArreglo(sentencia);
                        break;
                    case "AUMENTO_DECREMENTO": ejecutarAumentoDecremento(sentencia);
                        break;
                    case "SENTENCIA_RETURN": return calcularValor(sentencia.ChildNodes.ElementAt(0));

                    case "DECLARACION_OBJETO": ejecutarDeclaracionObjeto(sentencia);
                        break;
                    case "ASIGNACION_OBJETO": ejecutarAsignacionObjeto(sentencia);
                        break;
                    case "DECLARACION_ASIGNACION_OBJETO": ejecutarDeclaracionAsignacionObjeto(sentencia);
                        break;
                    case "ACCESO_FUNCION_OBJETO": ejecutarAccesoObjetoFuncionSinRetorno(sentencia);
                        break;
                    case "REASIGNACION_VARIABLE_OBJETO": ejecutarReasignacionVariableObjeto(sentencia);
                        break;
                    case "FUNCION_NATIVA_ADDFIGURE": ejecutarAddFigure(sentencia);
                        break;
                    case "FUNCION_NATIVA_FIGURE": ejecutarFigure(sentencia);
                        break;
                    default: Console.WriteLine("SENTECIAS NO MANEJADAS EN FUNCION CON RETORNO");
                        break;
                }
            }
            errores.Add(new Error("ERROR SEMANTICO", "FUNCION " + contextoActual.identificadorClase.Substring(contextoActual.identificadorClase.LastIndexOf("@") + 1) + " ESPERABA SENTENCIA RETURN", 0, 0));
            return null;
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

        private void ejecutarAumentoDecremento(ParseTreeNode nodoOperacion)
        {
            String tipo = nodoOperacion.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Substring(nodoOperacion.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().LastIndexOf('(') - 1);
            String contenido = nodoOperacion.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Substring(0, (nodoOperacion.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).ToString().Length - tipo.Length));
            switch (nodoOperacion.ChildNodes.ElementAt(0).ToString())
            {
                case "AUMENTO":
                    switch (tipo.Trim())
                    {
                        case "(int)": //valor + 1
                            break;
                        case "(double)": //valor + 1
                            break;
                        case "(char)": errores.Add(new Error("ERROR SEMANTICO", "AUMENTO NO SOPORTA CHAR", 0, 0));
                            break;
                        case "(string)": errores.Add(new Error("ERROR SEMANTICO", "AUMENTO NO SOPORTA STRING", 0, 0));
                            break;
                        case "(bool)": errores.Add(new Error("ERROR SEMANTICO", "AUMENTO NO SOPORTA BOOL", 0, 0));
                            break;
                        case "(identificador)":
                            Object obj = null;
                            Contexto c = contextoActual;
                            while (c != null)
                            {
                                obj = c.obtenerSimbolo(contenido);
                                if (obj == null)
                                    c = c.anterior;
                                else
                                    break;
                            }
                            if (obj != null)
                            {
                                Variable variable = (Variable)obj;
                                if (variable.valor != null)
                                {
                                    switch (variable.tipo.ToLower().Trim())
                                    {
                                        case "int":
                                            variable.valor = (Int32.Parse(variable.valor.ToString())) + 1;
                                            if (!c.actualizarSimbolo(contenido, variable))
                                            {
                                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + contenido + " NO DECLARADA", 0, 0));
                                            }
                                            break;
                                        case "double":
                                            variable.valor = (Double.Parse(variable.valor.ToString())) + 1;
                                            if (!c.actualizarSimbolo(contenido, variable))
                                            {
                                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + contenido + " NO DECLARADA", 0, 0));
                                            }
                                            break;
                                        case "char":
                                            errores.Add(new Error("ERROR SEMANTICO", "AUMENTO NO SOPORTA CHAR", 0, 0));
                                            break;
                                        case "string":
                                            errores.Add(new Error("ERROR SEMANTICO", "AUMENTO NO SOPORTA STRING", 0, 0));
                                            break;
                                        case "bool":
                                            errores.Add(new Error("ERROR SEMANTICO", "AUMENTO NO SOPORTA BOOL", 0, 0));
                                            break;
                                    }
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + variable.identificador + " SIN VALOR ASIGNADO", 0, 0));
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + contenido + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
                            }
                            break;
                    }
                    break;
                case "DECREMENTO":
                    switch (tipo.Trim())
                    {
                        case "(int)": //valor + 1
                            break;
                        case "(double)": //valor + 1
                            break;
                        case "(char)":
                            errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO NO SOPORTA CHAR", 0, 0));
                            break;
                        case "(string)":
                            errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO NO SOPORTA STRING", 0, 0));
                            break;
                        case "(bool)":
                            errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO NO SOPORTA BOOL", 0, 0));
                            break;
                        case "(identificador)":
                            String tipoDato = "";
                            Object valor = null, obj = null;
                            Contexto c = contextoActual;
                            while (c != null)
                            {
                                obj = c.obtenerSimbolo(contenido);
                                if (obj == null)
                                    c = c.anterior;
                                else
                                    break;
                            }
                            if (obj != null)
                            {
                                Variable variable = (Variable)obj;
                                if (variable.valor != null)
                                {
                                    switch (variable.tipo.ToLower().Trim())
                                {
                                    case "int":
                                        variable.valor = (Int32.Parse(variable.valor.ToString())) - 1;
                                        if (!c.actualizarSimbolo(contenido, variable))
                                        {
                                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + contenido + " NO DECLARADA", 0, 0));
                                        }
                                        break;
                                    case "double":
                                        variable.valor = (Double.Parse(variable.valor.ToString())) - 1;
                                        if (!c.actualizarSimbolo(contenido, variable))
                                        {
                                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + contenido + " NO DECLARADA", 0, 0));
                                        }
                                        break;
                                    case "char":
                                        errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO NO SOPORTA CHAR", 0, 0));
                                        break;
                                    case "string":
                                        errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO NO SOPORTA STRING", 0, 0));
                                        break;
                                    case "bool":
                                        errores.Add(new Error("ERROR SEMANTICO", "DECREMENTO NO SOPORTA BOOL", 0, 0));
                                        break;
                                }
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + variable.identificador + " SIN VALOR ASIGNADO", 0, 0));
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + contenido + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
                            }
                            break;
                    }
                    break;
            }
        }

        private void ejecutarReasignacionArreglo(ParseTreeNode nodoReasignacion)
        {
            String identificadorArreglo = nodoReasignacion.ChildNodes.ElementAt(0).ToString().Substring(0, (nodoReasignacion.ChildNodes.ElementAt(0).ToString().Length - nodoReasignacion.ChildNodes.ElementAt(0).ToString().Substring(nodoReasignacion.ChildNodes.ElementAt(0).ToString().LastIndexOf('(') - 1).Length));
            Object obj = buscarSimboloEnContexto(identificadorArreglo);
            if (obj != null)
            {
                Arreglo arreglo = (Arreglo)obj;
                if (arreglo.valores.Count > 0)
                {
                    if (arreglo.dimensiones.Count == nodoReasignacion.ChildNodes.ElementAt(1).ChildNodes.Count)
                    {
                        String key = "";
                        for (int i = 0; i < arreglo.dimensiones.Count; i++)
                        {
                            Object pos = calcularValor(nodoReasignacion.ChildNodes.ElementAt(1).ChildNodes.ElementAt(i));
                            if (pos.GetType().ToString().Equals("System.Int32"))
                            {
                                if (Int32.Parse(pos.ToString()) < Int32.Parse(arreglo.dimensiones[i].ToString()))
                                {
                                    key += pos.ToString();
                                }
                                else
                                {
                                    errores.Add(new Error("ERROR SEMANTICO", "POSICION EN ARREGLO " + identificadorArreglo + " FUERA DE RANGO", 0, 0));
                                    key = null;
                                    break;
                                }
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "POSICION EN ARREGLO " + identificadorArreglo + " CON VALOR DISTINTO A INT", 0, 0));
                                key = null;
                                break;
                            }
                        }
                        if (key != null)
                        {
                            Object val = calcularValor(nodoReasignacion.ChildNodes.ElementAt(2));
                            if (val.GetType().ToString().Equals(tipoDatoSistema(arreglo.tipo)))
                            {
                                arreglo.actualizarValor(key, val);
                                Contexto c = contextoActual;
                                while (c != null)
                                {
                                    if (c.obtenerSimbolo(identificadorArreglo) == null)
                                        c = c.anterior;
                                    else
                                        break;
                                }
                                c.actualizarSimbolo(identificadorArreglo, arreglo);
                            }
                            else
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "ARREGLO " + identificadorArreglo + " DE DISTINTO TIPO DE DATO AL VALOR ASIGNADO", 0, 0));
                            }
                        }
                    }
                    else
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "ARREGLO " + identificadorArreglo + " SIN VALORES ASIGNADOS", 0, 0));
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "ARREGLO " + identificadorArreglo + " SIN VALORES ASIGNADOS", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "ARREGLO " + identificadorArreglo + " NO DECLARADO EN EL CONTEXTO ACTUAL", 0, 0));
            }
        }

        private Boolean agregarImports(ArrayList imports)
        {
            Contexto tmp = contextoActual;
            bool existeClase;
            foreach (String import in imports)
            {
                existeClase = false;
                foreach (Clase clase in clases)
                {
                    if (import.Equals(clase.identificador))
                    {
                        existeClase = true;
                        while (tmp.anterior != null)
                            tmp = tmp.anterior;
                        tmp.anterior = new Contexto(clase.identificador, clase.tablaDeSimbolos);
                    }
                }
                if (!existeClase)
                {
                    errores.Add(new Error("ERROR SEMANTICO", "NO SE ENCONTRO IMPORT EN CLASE " + contextoActual.identificadorClase, 0, 0));
                    return false;
                }
            }
            return true; ;
        }

        private void ejecutarDeclaracionObjeto(ParseTreeNode nodoDeclaracion)
        {
            if (!nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim().ToLower().Equals(nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim().ToLower()))
            {
                bool existeClase = false;
                for (int i = 0; i < clases.Count; i++)
                {
                    if (((Clase)clases[i]).identificador.Equals(nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim()))
                    {
                        existeClase = true;
                    }
                }
                if (existeClase)
                {
                    if (!contextoActual.agregarSimbolo(nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim())))
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "CLASE " + nodoDeclaracion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO EXISTE PARA DECLARACION DE OBJETO", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "OBJETO " + nodoDeclaracion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " NO PUEDE TENER EL MISMO NOMBRE QUE LA CLASE", 0, 0));
            }
        }

        private void ejecutarAsignacionObjeto(ParseTreeNode nodoAsignacion)
        {
            Object obj = null;
            Contexto c = contextoActual;
            while (c != null)
            {
                obj = c.obtenerSimbolo(nodoAsignacion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
                if (obj == null)
                    c = c.anterior;
                else
                    break;
            }
            if (obj != null)
            {
                Variable variable = (Variable)obj;
                int i;
                for (i = 0 ; i < clases.Count ; i++)
                {
                    if (((Clase)clases[i]).identificador.Equals(variable.tipo))
                        break;

                }
                variable.valor = clases[i];
                if (!c.actualizarSimbolo(variable.identificador, variable))
                {
                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + variable.identificador + " NO DECLARADA", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodoAsignacion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
            }
        }

        private void ejecutarDeclaracionAsignacionObjeto(ParseTreeNode nodoDA)
        {
            if (!nodoDA.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim().ToLower().Equals(nodoDA.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim().ToLower()))
            {
                if (nodoDA.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim().ToLower().Equals(nodoDA.ChildNodes.ElementAt(2).ToString().Replace("(identificador)", "").Trim().ToLower()))
                {
                    bool existeClase = false;
                    int i;
                    for (i = 0; i < clases.Count; i++)
                    {
                        if (((Clase)clases[i]).identificador.Equals(nodoDA.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim()))
                        {
                            existeClase = true;
                            break;
                        }
                    }
                    if (existeClase)
                    {
                        if (!contextoActual.agregarSimbolo(nodoDA.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), new Variable(nodoDA.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim(), nodoDA.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), clases[i])))
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodoDA.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " YA EXISTE", 0, 0));
                        }
                    }
                    else
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "CLASE " + nodoDA.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO EXISTE PARA DECLARACION DE OBJETO", 0, 0));
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "OBJETO " + nodoDA.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " NO PUEDE TENER DISTINTAS CLASES EN DECLARACION", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "OBJETO " + nodoDA.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim() + " NO PUEDE TENER EL MISMO NOMBRE QUE LA CLASE", 0, 0));
            }
        }

        private Object ejecutarAccesoObjetoVariable(ParseTreeNode nodoAcceso)
        {
            Object obj = buscarSimboloEnContexto(nodoAcceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
            if (obj == null)
            {
                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodoAcceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
                return null;
            }
            if (((Variable)obj).valor == null)
            {
                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodoAcceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " SIN VALOR ASIGNADO", 0, 0));
                return null;
            }
            try
            {
                Object val = ((Clase)((Variable)obj).valor).obtenerVariable(nodoAcceso.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim()).valor;
                if (val == null)
                {
                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodoAcceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO TIENE ELEMENTO " + nodoAcceso.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), 0, 0));
                    return null;
                }
                else
                {
                    return val;
                }
            }
            catch (Exception ex)
            {
                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + nodoAcceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO TIENE ELEMENTO " + nodoAcceso.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), 0, 0));
                return null;
            }
        }

        private Object ejecutarAccesoObjetoFuncionConRetorno(ParseTreeNode sentencias, String identificadorClase)
        {
            Contexto tmp = contextoActual;
            Object result = null;
            foreach (Clase clase in clases)
            {
                if (clase.identificador.Equals(identificadorClase))
                {
                    contextoActual = new Contexto(clase.identificador, clase.tablaDeSimbolos);
                    agregarImports(clase.imports);
                    result = ejecutarFuncionConRetorno(sentencias);
                }
            }
            contextoActual = tmp;
            return result;
        }

        private void ejecutarAccesoObjetoFuncionSinRetorno(ParseTreeNode acceso)
        {
            Object obj = buscarSimboloEnContexto(acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
            if (obj != null)
            {
                if (((Variable)obj).valor != null)
                {
                    Object func = ((Clase)((Variable)obj).valor).obtenerSimbolo("@" + acceso.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim());
                    if (func != null)
                    {
                        Funcion funcion = (Funcion)func;
                        if (funcion.retorno == null)
                        {
                            Contexto tmp = contextoActual;
                            foreach (Clase clase in clases)
                            {
                                if (clase.identificador.Equals(funcion.clasePadre))
                                {
                                    contextoActual = new Contexto(clase.identificador, clase.tablaDeSimbolos);
                                    agregarImports(clase.imports);
                                    ejecutarFuncionSinRetorno(funcion.sentencias);
                                }
                            }
                            contextoActual = tmp;
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "FUNCION  " + funcion.identificador + " EN VARIABLE " + acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " TIENE RETORNO", 0, 0));
                        }
                    }
                    else
                    {
                        errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO TIENE ELEMENTO " + acceso.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim(), 0, 0));
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " SIN VALOR ASIGNADO", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + acceso.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
            }
        }

        private void ejecutarReasignacionVariableObjeto(ParseTreeNode reasignacion)
        {
            Object obj = buscarSimboloEnContexto(reasignacion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
            if (obj != null)
            {
                if (((Variable)obj).valor != null)
                {
                    Variable variable = ((Clase)((Variable)obj).valor).obtenerVariable(reasignacion.ChildNodes.ElementAt(1).ToString().Replace("(identificador)", "").Trim());
                    String tipoDato = tipoDatoSistema(variable.tipo);
                    Object valor = calcularValor(reasignacion.ChildNodes.ElementAt(2));
                    if (valor != null)
                    {
                        if (valor.GetType().ToString().Equals(tipoDato))
                        {
                            if (!((Clase)((Variable)obj).valor).actualizarVariable(variable.identificador, valor))
                            {
                                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + variable.identificador + " NO DECLARADA", 0, 0));
                            }
                        }
                        else
                        {
                            errores.Add(new Error("ERROR SEMANTICO", "TIPO DE DATO PARA " + reasignacion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + "." + variable.identificador + " INVALIDO", 0, 0));
                        }
                    }
                }
                else
                {
                    errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + reasignacion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " SIN VALOR ASIGNADO", 0, 0));
                }
            }
            else
            {
                errores.Add(new Error("ERROR SEMANTICO", "VARIABLE " + reasignacion.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim() + " NO DECLARADA EN EL CONTEXTO ACTUAL", 0, 0));
            }
        }

        private void ejecutarAddFigure(ParseTreeNode nodoAdd)
        {

            ParseTreeNode figura = nodoAdd.ChildNodes.ElementAt(0);
            switch (figura.ToString())
            {
                case "FUNCION_NATIVA_CIRCLE": 
                    break;
                case "FUNCION_NATIVA_TRIANGLE":
                    break;
                case "FUNCION_NATIVA_SQUARE":
                    break;
                case "FUNCION_NATIVA_LINE":
                    break;

            }
        }

        private void ejecutarFigure(ParseTreeNode nodoFigure)
        {

        }

        private void listarSimbolos()
        {
            foreach (Clase clase in clases)
            {
                foreach (String key in clase.tablaDeSimbolos.Keys)
                {
                    if (!key.Contains("@"))
                    {
                        simbolos.Add(new Simbolo(key, ((Variable)clase.tablaDeSimbolos[key]).valor.ToString(), clase.identificador));
                    }
                    else
                    {
                        try
                        {
                            if (((Funcion)clase.tablaDeSimbolos[key]).retorno == null)
                            {
                                simbolos.Add(new Simbolo(key, "void", clase.identificador));
                            }
                            else
                            {
                                simbolos.Add(new Simbolo(key, ((Funcion)clase.tablaDeSimbolos[key]).retorno.ToString(), clase.identificador));
                            }
                        }
                        catch (Exception ex)
                        {
                            simbolos.Add(new Simbolo(key, clase.tablaDeSimbolos[key].ToString(), clase.identificador));
                        }
                    }
                }
            }
        }

        public List<Simbolo> listaSimbolos()
        {
            listarSimbolos();
            return simbolos;
        }
    }
}
