using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.graficador
{
    class Grafica
    {
        private int contadorNodos = 0;
        private String conexiones = "";
        private StreamWriter streamWriter;

        public void graficar(ParseTreeNode raiz)
        {
            try
            {
                using (streamWriter = new StreamWriter("ast.txt"))
                {
                    streamWriter.WriteLine("digraph G{");
                    String idNodo = "nodo" + (++contadorNodos).ToString();
                    streamWriter.WriteLine(idNodo + "[label = \" " + contadorNodos.ToString() + " \\n " + raiz.ToString() + " \"]");

                    if (raiz.ChildNodes.Count != 0)
                    {
                        foreach (ParseTreeNode node in raiz.ChildNodes)
                            graficarHijos(idNodo, node);
                    }
                    streamWriter.Write(conexiones + "\n}");
                }
                String comando = "/C dot.exe -Tsvg ast.txt -o ast.svg";
                System.Diagnostics.Process.Start("CMD.exe", comando);
                System.Diagnostics.Process.Start("ast.svg");
            }
            catch (Exception ex)
            {
                abrirArchivo();
            }
        }

        private void graficarHijos(String padre, ParseTreeNode hijo)
        {
            string idNodo = "nodo" + (++contadorNodos).ToString();
            streamWriter.WriteLine(idNodo + "[label = \" " + contadorNodos.ToString() + " \\n " + hijo.ToString() + " \"]");
            conexiones += padre + "->" + idNodo + "\n";
            if (hijo.ChildNodes.Count != 0)
            {
                foreach (ParseTreeNode node in hijo.ChildNodes)
                    graficarHijos(idNodo, node);
            }
        }

        private void abrirArchivo()
        {
            try
            {
                System.Diagnostics.Process.Start("ast.svg");
            }
            catch (Exception ex)
            {
                abrirArchivo();
            }
        }
    }
}
