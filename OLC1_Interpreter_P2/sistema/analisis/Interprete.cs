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
                    if (clase.imports.Count > 0)
                    {
                        foreach (String import in clase.imports)
                        {
                            Console.WriteLine("\t"+import);
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
            clases.Add(clase);
            //lectura de import o sentencias internas de la clase segun sea el caso
            if (declaracionClase.ChildNodes.Count > 1)
            {
                if (declaracionClase.ChildNodes.ElementAt(1).ToString().Equals("IMPORTAR"))
                {
                    foreach (ParseTreeNode nodo in declaracionClase.ChildNodes.ElementAt(1).ChildNodes)
                        clase.agregarImport(nodo.ToString().Replace("(identificador)", "").Trim());
                }
            }
        }
    }
}
