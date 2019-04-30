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
        private Clase claseActual;

        public Interprete()
        {
            raiz = null;
            clases = new ArrayList();
        }

        public bool analizar(String contenido)
        {
            Gramatica gramatica = new Gramatica();
            LanguageData languageData = new LanguageData(gramatica);
            Parser parser = new Parser(languageData);
            ParseTree parseTree = parser.Parse(contenido);

            if(parseTree.Root != null)
            {
                Grafica grafica = new Grafica();
                grafica.graficar(parseTree.Root);
                raiz = parseTree.Root;
                analizarAST();
                foreach (Clase clase in clases)
                {
                    Console.WriteLine(clase.identificador);
                    foreach (String import in clase.imports)
                    {
                        Console.WriteLine("\t"+import);
                    }
                    foreach (Variable variable in clase.variables)
                    {
                        if (variable.valor != null)
                        {
                            Console.WriteLine("\t\t" + variable.identificador + " | " + variable.tipo + " | " + variable.visibilidad + " | " + variable.valor.ToString());
                        }
                        else
                        {
                            Console.WriteLine("\t\t" + variable.identificador + " | " + variable.tipo + " | " + variable.visibilidad);
                        }
                        
                    }
                }
                return true;
            }
            return false;
        }

        private void analizarAST()
        {
            if (raiz != null)
                recorrerAST();
        }

        private void recorrerAST()
        {
            foreach (ParseTreeNode nodo in raiz.ChildNodes)
            {
                recorrerDeclaracionClase(nodo);
            }
        }

        private void recorrerDeclaracionClase(ParseTreeNode declaracionClase)
        {
            //lecutar identificador de la clase
            Clase clase = new Clase(declaracionClase.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim());
            //lectura de import o sentencias internas de la clase segun sea el caso
            if (declaracionClase.ChildNodes.Count == 2)
            {
                if (declaracionClase.ChildNodes.ElementAt(1).ToString().Equals("IMPORTAR"))
                {
                    definirImports(declaracionClase.ChildNodes.ElementAt(1), clase);
                }
                else if(declaracionClase.ChildNodes.ElementAt(1).ToString().Equals("CUERPO_CLASE"))
                {
                    foreach (ParseTreeNode nodo in declaracionClase.ChildNodes.ElementAt(1).ChildNodes)
                    {
                        switch (nodo.ToString())
                        {
                            case "DECLARACION_VARIABLE":
                                clase = declaracionVariable(nodo, clase);
                                break;
                            case "ASIGNACION_VARIABLE":
                                clase = asignacionVariable(nodo, clase);
                                break;
                            default: Console.WriteLine("CASO NO MANEJADO");
                                break;
                        }
                    }
                }
            }else if (declaracionClase.ChildNodes.Count == 3)
            {
                definirImports(declaracionClase.ChildNodes.ElementAt(1), clase);

                foreach (ParseTreeNode nodo in declaracionClase.ChildNodes.ElementAt(2).ChildNodes)
                {
                    switch (nodo.ToString())
                    {
                        case "DECLARACION_VARIABLE":
                            clase = declaracionVariable(nodo, clase);
                            break;
                        case "ASIGNACION_VARIABLE":
                            clase = asignacionVariable(nodo, clase);
                            break;
                        default:
                            Console.WriteLine("CASO NO MANEJADO");
                            break;
                    }
                }
            }
            bool errorExistencia = false;//validacion de existencia de la clase para agregarla a la lista
            foreach (Clase c in clases)
            {
                if (c.identificador.Equals(clase.identificador))
                {
                    //error semantico - clase repetida
                    errorExistencia = true;
                    break;
                }
            }
            if(!errorExistencia)
                clases.Add(clase);
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

        private Clase declaracionVariable(ParseTreeNode declaracionVariable, Clase clase)
        {
            if (declaracionVariable.ChildNodes.Count == 2)
            {
                foreach (ParseTreeNode nodo in declaracionVariable.ChildNodes.ElementAt(1).ChildNodes)
                {
                    String identificador = nodo.ToString().Replace("(identificador)", "").Trim();
                    bool errorExistencia = false;
                    foreach (Variable variable in clase.variables)
                    {
                        if (variable.identificador.Equals(identificador))
                        {
                            //error semantico - variable repetida
                            errorExistencia = true;
                            break;
                        }
                    }
                    if (!errorExistencia)
                        clase.agregarVariable(new Variable(declaracionVariable.ChildNodes.ElementAt(0).ToString().Replace("(Keyword)", "").Trim(), identificador));
                }
            }
            else
            {
                foreach (ParseTreeNode nodo in declaracionVariable.ChildNodes.ElementAt(2).ChildNodes)
                {
                    String identificador = nodo.ToString().Replace("(identificador)", "").Trim();
                    bool errorExistencia = false;
                    foreach (Variable variable in clase.variables)
                    {
                        if (variable.identificador.Equals(identificador))
                        {
                            //error semantico - variable repetida
                            errorExistencia = true;
                            break;
                        }
                    }
                    if (!errorExistencia)
                        clase.agregarVariable(new Variable(declaracionVariable.ChildNodes.ElementAt(1).ToString().Replace("(Keyword)", "").Trim(), identificador, declaracionVariable.ChildNodes.ElementAt(0).ToString().Replace("(visibilidad)", "").Trim()));
                }
            }
            return clase;
        }

        private Clase asignacionVariable(ParseTreeNode asignacionVariable, Clase clase)
        {
            foreach (Variable variable in clase.variables)
            {
                if (variable.identificador.Equals(asignacionVariable.ChildNodes.ElementAt(0).ToString().Replace("(identificador)", "").Trim()))
                {
                    variable.valor = capturarValor(asignacionVariable.ChildNodes.ElementAt(1), variable.tipo);
                }
            }
            return clase;
        }

        private object capturarValor(ParseTreeNode nodo, string tipo)
        {
            if(nodo.ChildNodes.Count == 1)
            {
                string[] contenidoNodo = nodo.ChildNodes.ElementAt(0).ToString().Split(' ');
                if (contenidoNodo[1].Replace("(", "").Replace(")", "").Equals(tipo))
                {
                    return contenidoNodo[0];
                }
            }
            else
            {
                Console.WriteLine("CASO NO MANEJADO");
            }
            return null;
        }
    }
}
