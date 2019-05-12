using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Contexto
    {
        private String _identificadorClase;
        private Hashtable _tablaDeSimbolos;
        private Contexto _anterior;
        private ArrayList _imports;

        public Contexto(String identificadorClase)
        {
            _identificadorClase = identificadorClase;
            _tablaDeSimbolos = new Hashtable();
            _anterior = null;
            _imports = new ArrayList();
        }

        public Contexto(String identificadorClase, Hashtable simbolos)
        {
            _identificadorClase = identificadorClase;
            _tablaDeSimbolos = simbolos;
            _anterior = null;
            _imports = new ArrayList();
        }

        public Contexto(String identificadorClase, Hashtable simbolos, Contexto anterior)
        {
            _identificadorClase = identificadorClase;
            _tablaDeSimbolos = simbolos;
            _anterior = anterior;
            _imports = new ArrayList();
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

        public Boolean actualizarSimbolo(String key, Object valor)
        {
            if (_tablaDeSimbolos.ContainsKey(key))
            {
                _tablaDeSimbolos[key] = valor;
                return true;
            }
            return false;
        }

        public Object obtenerSimbolo(String key)
        {
            return _tablaDeSimbolos[key];
        }

        public Boolean agregarImport(String identificador)
        {
            if (!_imports.Contains(identificador))
            {
                _imports.Add(identificador);
                return true;
            }
            return false;
        }

        public String identificadorClase { get => _identificadorClase; set => _identificadorClase = value; }
        public Hashtable tablaDeSimbolos { get => _tablaDeSimbolos; set => _tablaDeSimbolos = value; }
        public Contexto anterior { get => _anterior; set => _anterior = value; }
        public ArrayList imports { get => _imports; }
    }
}
