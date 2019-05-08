using OLC1_Interpreter_P2.sistema.bean;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.graficador
{
    class Reporte
    {
        public void reporteErrores(ArrayList errores)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter("errores.html"))
                {
                    streamWriter.WriteLine("<!DOCTYPE html>" + "\n" + "<html>" + "\n" + "<head>" + "\n" + "<title>Reporte de Errores</title>" + "\n" + "</head>" + "\n" + "<body>" + "\n" + "<h1 align=\"center\">Reporte de Tokens</h1>" + "\n" + "<style type=\"text/css\">" + "\n" + ".tg  {border-collapse:collapse;border-spacing:0;margin:0px auto;}" + "\n" + ".tg td{font-family:Arial, sans-serif;font-size:14px;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:black;}" + "\n" + ".tg th{font-family:Arial, sans-serif;font-size:14px;font-weight:normal;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:black;}" + "\n" + ".tg .tg-4tse{background-color:#cb0d00;color:#000000;text-align:left;vertical-align:top}" + "\n" + ".tg .tg-0lax{text-align:left;vertical-align:top}" + "\n" + "h1{font-family:Arial, sans-serif}" + "\n" + "</style>");

                    streamWriter.WriteLine("<table class=\"tg\">");
                    streamWriter.WriteLine("<tr>" + "\n" + "<th class=\"tg-4tse\">No</th>" + "\n" + "<th class=\"tg-4tse\">Lexema</th>" + "\n" + "<th class=\"tg-4tse\">Tipo</th>" + "\n" + "<th class=\"tg-4tse\">Columna</th>" + "\n" + "<th class=\"tg-4tse\">Linea</th>" + "\n" + "</tr>");


                    for (int i = 0 ; i < errores.Count ; i++)
                    {
                        streamWriter.WriteLine("<tr>");
                        streamWriter.WriteLine("<td class=\"tg-0Lax\">" + i.ToString() + "<br></td>");
                        streamWriter.WriteLine("<td Class=\"tg-0Lax\">" + ((Error)errores[i]).contenido + "</td>");
                        streamWriter.WriteLine("<td class=\"tg-0Lax\">" + ((Error)errores[i]).descripcion + "</td>");
                        streamWriter.WriteLine("<td Class=\"tg-0Lax\">" + ((Error)errores[i]).columna + "</td>");
                        streamWriter.WriteLine("<td class=\"tg-0Lax\">" + ((Error)errores[i]).linea + "</td>");
                        streamWriter.WriteLine("</tr>");
                    }

                    streamWriter.WriteLine("</body>" + "\n" + "</html>");
                    streamWriter.Close();
                }
                System.Diagnostics.Process.Start("errores.html");
            }
            catch (Exception ex)
            {
                abrirArchivo();
            }
        }

        private void abrirArchivo()
        {
            try
            {
                System.Diagnostics.Process.Start("errores.html");
            }
            catch (Exception ex)
            {
                abrirArchivo();
            }
        }
    }
}
