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

        public Clase(String identificador, Hashtable tablaDeSimbolos)
        {
            _identificador = identificador;
            _tablaDeSimbolos = tablaDeSimbolos;
            _imports = new ArrayList();
        }

        public Clase(String identificador, Hashtable tablaDeSimbolos, ArrayList imports)
        {
            _identificador = identificador;
            _tablaDeSimbolos = tablaDeSimbolos;
            _imports = imports;
        }

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

        public Boolean actualizarVariable(String key, Object valor)
        {
            if (_tablaDeSimbolos.ContainsKey(key))
            {
                ((Variable)_tablaDeSimbolos[key]).valor = valor;
                return true;
            }
            return false;
        }

        public Variable obtenerVariable(String key)
        {
            return ((Variable)tablaDeSimbolos[key]);
        }
        
        public Object obtenerSimbolo(String key)
        {
            return _tablaDeSimbolos[key];
        }

        public string identificador { get => _identificador; set => _identificador = value; }
        public ArrayList imports { get => _imports; }
        public Hashtable tablaDeSimbolos { get => _tablaDeSimbolos; }
    }
}
