using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Simbolo
    {
        private String _nombre;
        private String _valor;
        private String _clase;

        public Simbolo(String nombre, String valor, String clase)
        {
            _nombre = nombre;
            _valor = valor;
            _clase = clase;
        }

        public String nombre { get => _nombre; set => _nombre = value; }
        public String valor { get => _valor; set => _valor = value; }
        public String clase { get => _clase; set => _clase = value; }
    }
}
