using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLC1_Interpreter_P2.sistema.bean
{
    class Entorno
    {
        private Entorno anterior;
        private Hashtable tablaDeSimbolos;
        
        public Entorno()
        {
            this.anterior = null;
            this.tablaDeSimbolos = new Hashtable();
        }

        public Entorno(Hashtable tablaDeSimbolos)
        {
            this.anterior = null;
            this.tablaDeSimbolos = tablaDeSimbolos;
        }

        public Entorno(Entorno anterior, Hashtable tablaDeSimbolos)
        {
            this.anterior = anterior;
            this.tablaDeSimbolos = tablaDeSimbolos;
        }
    }
}
