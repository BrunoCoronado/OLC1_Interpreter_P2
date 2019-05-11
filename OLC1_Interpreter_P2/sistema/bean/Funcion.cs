using Irony.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Funcion
    {
        private String _identificador;
        private int _visibilidad;
        private String _retorno;//va a ser el tipo de dato lo que nos interesa
        private ParseTreeNode _sentencias;
        private ArrayList _parametros;
        private Hashtable _tablaDeSimbolos;
        
        public Funcion(String identificador, ParseTreeNode sentencias)
        {
            _identificador = identificador;
            _sentencias = sentencias;
            _visibilidad = 0;
            _retorno = null;
            _parametros = new ArrayList();
            _tablaDeSimbolos = new Hashtable();
        }

        public Funcion(String identificador, ParseTreeNode sentencias, String visibilidad)
        {
            _identificador = identificador;
            _visibilidad = (visibilidad.Equals("publico")) ? 0 : 1;
            _sentencias = sentencias;
            _retorno = null;
            _parametros = new ArrayList();
            _tablaDeSimbolos = new Hashtable();
        }

        public Boolean agregarParametro(String key, Object value)
        {
            if (!_tablaDeSimbolos.ContainsKey(key))
            {
                _parametros.Add(value);
                _tablaDeSimbolos.Add(key, value);
                return true;
            }
            return false;
        }

        public Boolean agregarValorParametro(String key, Object valor, int indice)
        {
            if (_tablaDeSimbolos.ContainsKey(key))
            {
                ((Variable)_tablaDeSimbolos[key]).valor = valor;
                _parametros[indice] = _tablaDeSimbolos[key];
                return true;
            }
            return false;
        }

        public String identificador { get => _identificador; }
        public int visibilidad { get => _visibilidad; }
        public String retorno { get => _retorno; }
        public ParseTreeNode sentencias { get => _sentencias; }
        public ArrayList parametros { get => _parametros; }
        public Hashtable tablaDeSimbolos { get => _tablaDeSimbolos; }

    }
}
