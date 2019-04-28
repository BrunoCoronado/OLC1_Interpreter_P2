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
        private ArrayList _variables;

        public Clase(String identificador)
        {
            _identificador = identificador;
            _imports = new ArrayList();
            _variables = new ArrayList();
        }

        public void agregarImport(String clase)
        {
            _imports.Add(clase);
        }

        public void agregarVariable(Variable variable)
        {
            _variables.Add(variable);
        }

        public string identificador { get => _identificador; set => _identificador = value; }
        public ArrayList imports { get => _imports; }
        public ArrayList variables { get => _variables; }
    }
}
