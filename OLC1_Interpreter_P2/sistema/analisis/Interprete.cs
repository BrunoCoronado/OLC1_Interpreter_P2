using Irony.Parsing;
using OLC1_Interpreter_P2.sistema.graficador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.analisis
{
    class Interprete : Grammar
    {
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
                return true;
            }
            return false;
        }
    }
}
