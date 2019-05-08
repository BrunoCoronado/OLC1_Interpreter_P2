using Irony.Parsing;
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
        private Hashtable _tablaDeSimbolos;

        public Clase(String identificador)
        {
            _identificador = identificador;
            _imports = new ArrayList();
            _tablaDeSimbolos = new Hashtable();
        }

        public void agregarImport(String clase)
        {
            _imports.Add(clase);
        }

        public Boolean agregarSimbolo(String key, Object value)
        {
            if (!_tablaDeSimbolos.ContainsKey(key))
            {
                _tablaDeSimbolos.Add(key, value);
                return true;
            }
            return false;
        }

        public Boolean actualizarVariable(String key, ParseTreeNode nodo)
        {
            if (_tablaDeSimbolos.ContainsKey(key))
            {
                ((Variable)tablaDeSimbolos[key]).valor = nodo;
                return true;
            }
            return false;
        }

        public string identificador { get => _identificador; set => _identificador = value; }
        public ArrayList imports { get => _imports; }
        public Hashtable tablaDeSimbolos { get => _tablaDeSimbolos; }
    }
}
