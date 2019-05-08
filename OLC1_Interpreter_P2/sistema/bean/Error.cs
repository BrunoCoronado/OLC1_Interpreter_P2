using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Error
    {
        private String _contenido;
        private String _descripcion;
        private int _linea;
        private int _columna;

        public Error(String contenido, String descripcion, int linea, int columna)
        {
            _contenido = contenido;
            _descripcion = descripcion;
            _linea = linea;
            _columna = columna;
        }

        public String contenido { get => _contenido; set => _contenido = value; }
        public String descripcion { get => _descripcion; set => _descripcion = value; }
        public int linea { get => _linea; set => _linea = value; }
        public int columna { get => _columna; set => _columna = value; }
    }
}
