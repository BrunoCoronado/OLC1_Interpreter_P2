using Irony.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Arreglo
    {
        private ArrayList _dimensiones;
        private Hashtable _valores;
        private int _visibilidad;
        private String _tipo;
        private String _identificador;

        public Arreglo(String identificador, String tipo)
        {
            _identificador = identificador;
            _tipo = tipo;
            _dimensiones = new ArrayList();
            _valores = new Hashtable();
            _visibilidad = 0;
        }

        public Arreglo(String identificador, String tipo, ArrayList dimensiones)
        {
            _identificador = identificador;
            _tipo = tipo;
            _dimensiones = dimensiones;
            _valores = new Hashtable();
            _visibilidad = 0;
        }

        public Arreglo(String identificador, String tipo, String visibilidad)
        {
            _identificador = identificador;
            _tipo = tipo;
            _visibilidad = (visibilidad.Equals("publico")) ? 0 : 1;
            _dimensiones = new ArrayList();
            _valores = new Hashtable();
        }

        public Arreglo(String identificador, String tipo, String visibilidad, ArrayList dimensiones)
        {
            _identificador = identificador;
            _tipo = tipo;
            _visibilidad = (visibilidad.Equals("publico")) ? 0 : 1;
            _dimensiones = dimensiones;
            _valores = new Hashtable();
        }

        public void agregarValor(String key, Object valor)
        {
            _valores.Add(key, valor);
        }

        public ArrayList dimensiones { get => _dimensiones;  }
        public Hashtable valores { get => _valores; }
        public int visibilidad { get => _visibilidad; set => _visibilidad = value; }
        public String tipo { get => _tipo; set => _tipo = value; }
        public String identificador { get => _identificador; set => _identificador = value; }
    }
}
