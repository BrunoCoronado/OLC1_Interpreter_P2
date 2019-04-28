using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Clase
    {
        private string _identificador;
        private ArrayList _imports;

        public Clase(String identificador)
        {
            _identificador = identificador;
            _imports = new ArrayList();
        }

        public void agregarImport(String clase)
        {
            _imports.Add(clase);
        }

        public string identificador { get => _identificador; set => _identificador = value; }
        public ArrayList imports { get => _imports; }
    }
}
