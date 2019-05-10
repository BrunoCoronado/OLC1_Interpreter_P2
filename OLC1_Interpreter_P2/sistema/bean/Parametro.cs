using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Parametro
    {
        private String _tipo;
        private Object _valor;

        public Parametro(String tipo, Object valor)
        {
            _tipo = tipo;
            _valor = valor;
        }

        public String tipo { get => _tipo; set => _tipo = value; }
        public Object valor { get => _valor; set => _valor = value; }
    }
}
