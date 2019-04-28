using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Variable
    {
        private String _tipo;
        private String _identificador;
        private Object _valor;
        private int _visibilidad;

        public Variable(String tipo, String identificador)
        {
            _tipo = tipo;
            _identificador = identificador;
            _visibilidad = 0;
            _valor = null;
        }

        public Variable(String tipo, String identificador, String visibilidad)
        {
            _tipo = tipo;
            _identificador = identificador;
            _visibilidad=(visibilidad.Equals("publico"))?0:1;
            _valor = null;
        }

        public Variable(String tipo, String identificador, Object valor)
        {
            _tipo = tipo;
            _identificador = identificador;
            _visibilidad = 0;
            _valor = valor;
        }

        public Variable(String tipo, String identificador, Object valor, String visibilidad)
        {
            _tipo = tipo;
            _identificador = identificador;
            _visibilidad = (visibilidad.Equals("publico")) ? 0 : 1;
            _valor = valor;
        }

        public String tipo { get => _tipo; set => _tipo = value; }
        public String identificador { get => _identificador; set => _identificador = value; }
        public Object valor { get => _valor; set => _valor = value; }
        public int visibilidad { get => _visibilidad; set => _visibilidad = value; }
    }
}
