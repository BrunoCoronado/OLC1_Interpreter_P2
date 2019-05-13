using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.administracion
{
    class Archivo
    {
        public String abrirArchivo(String ruta)
        {
            String contenido = "";
            try
            {
                using (StreamReader streamReader = new StreamReader(ruta))
                {
                    String linea;
                    while ((linea = streamReader.ReadLine()) != null)
                        contenido += linea + "\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR NO ESPERADO AL LEER ARCHIVO");
            }
            return contenido;
        }

        public void guardarArchivo(String ruta, String contenido)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(ruta))
                {
                    streamWriter.Write(contenido);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR NO ESPERADO AL GUARDAR ARCHIVO");
            }
        }

        public void guardarComoArchivo(String ruta, String contenido)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(ruta))
                {
                    streamWriter.Write(contenido);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR NO ESPERADO AL GUARDAR ARCHIVO");
            }
        }

        public void nuevoArchivo(String ruta)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(ruta))
                {
                    streamWriter.Write("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR NO ESPERADO AL CREAR ARCHIVO");
            }
        }
    }
}
