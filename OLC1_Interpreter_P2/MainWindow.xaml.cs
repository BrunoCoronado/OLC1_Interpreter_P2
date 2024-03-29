﻿using Microsoft.Win32;
using OLC1_Interpreter_P2.sistema.administracion;
using OLC1_Interpreter_P2.sistema.analisis;
using OLC1_Interpreter_P2.sistema.graficador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OLC1_Interpreter_P2
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int contadorTabs;
        private Boolean removiendoTab;
        private Archivo archivo;
        private Interprete interprete;

        public MainWindow()
        {
            inicializacionVariablesLocales();
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)//manejamos el cambio en la seleccion de tabs
        {
            try
            {
                TabControl tabControl = sender as TabControl;
                if (!(tabControl.SelectedItem is null))
                {
                    //******MANEJO DEL EVENTO "+"******
                    if (((tabControl.SelectedItem as TabItem).Header as string).Equals("+") && !removiendoTab)//verificamos que el tab seleccionado sea el "+"
                    {
                        //MessageBox.Show("Agreganddo TAB");
                        //******CREACION DEL NUEVO TAB******
                        TabItem tabItem = new TabItem();
                        tabItem.Header = "sin titulo - " + ++contadorTabs;
                        //******FIN CREACION DEL NUEVO TAB******
                        //******CREACION DEL TEXTBOX PARA EL NUEVO TAB******
                        TextBox textBox = new TextBox();
                        textBox.AcceptsTab = true;
                        textBox.TextWrapping = TextWrapping.Wrap;
                        textBox.AcceptsReturn = true;
                        textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                        textBox.Height = Double.NaN;
                        textBox.Width = Double.NaN;
                        //******COPIA DEL TAB "+"******
                        TabItem copia = (sender as TabControl).SelectedItem as TabItem;
                        //******FIN COPIA DEL TAB "+"******
                        //******FIN CREACION DEL TEXTBOX PARA EL NUEVO TAB******
                        //******UNION DE LOS ELEMENTOS******
                        tabItem.Content = textBox;
                        this.tabEditor.Items.Remove((sender as TabControl).SelectedItem as TabItem);
                        this.tabEditor.Items.Add(tabItem);
                        this.tabEditor.Items.Add(copia);
                        this.tabEditor.SelectedItem = tabItem;
                        //******FIN UNION DE LOS ELEMENTOS******
                    }
                    //******FIN MANEJO DEL EVENTO "+"******
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR NO ESPERADO CREANDO NUEVAS TAB");
            }
        }

        private void CerrarTab_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(contadorTabs <= 1))
                {
                    removiendoTab = true;//bandera para definir que se esta removiendo una tab
                    this.tabEditor.Items.Remove(this.tabEditor.SelectedItem);//remover tab actual
                    this.tabEditor.SelectedIndex = (this.tabEditor.Items.Count - 2);//posicionar la actual en en la anterior a la eliminada
                    removiendoTab = false;
                    contadorTabs--;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR NO ESPERADO CERRANDO TAB ACTUAL");
            }
        }

        private void GuardarComo_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "archivo fi|*.fi";
            saveFileDialog.ShowDialog();
            if(!saveFileDialog.FileName.Equals(""))
            {
                archivo.guardarComoArchivo(saveFileDialog.FileName, ((this.tabEditor.SelectedItem as TabItem).Content as TextBox).Text);
                (this.tabEditor.SelectedItem as TabItem).Header = saveFileDialog.FileName;
            }
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (!((this.tabEditor.SelectedItem as TabItem).Header.ToString()).Contains("sin titulo -"))
            {
                archivo.guardarArchivo((this.tabEditor.SelectedItem as TabItem).Header.ToString(), ((this.tabEditor.SelectedItem as TabItem).Content as TextBox).Text);
            }
            else
            {
                GuardarComo_Click(sender, e);
            }
        }

        private void Abrir_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "archivo fi|*.fi";
            openFileDialog.ShowDialog();
            if (!openFileDialog.FileName.Equals(""))
            {
                ((this.tabEditor.SelectedItem as TabItem).Content as TextBox).Text = archivo.abrirArchivo(openFileDialog.FileName);
                (this.tabEditor.SelectedItem as TabItem).Header = openFileDialog.FileName;
            }
        }

        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "archivo fi|*.fi";
            saveFileDialog.ShowDialog();
            if (!saveFileDialog.FileName.Equals(""))
            {
                archivo.nuevoArchivo(saveFileDialog.FileName);
                (this.tabEditor.SelectedItem as TabItem).Header = saveFileDialog.FileName;
                ((this.tabEditor.SelectedItem as TabItem).Content as TextBox).Text = "";
            }
        }

        private void Compilar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                interprete = new Interprete();
                String contenido = "";
                for (int i = 0 ; i < (tabEditor.Items.Count - 1) ; i++)
                    contenido += ((TextBox)((TabItem)(tabEditor.Items[i])).Content).Text + "\n";
                if (interprete.analizar(contenido + "$")) { 
                    txtConsola.Text += interprete.consola;
                    dgSimbolos.ItemsSource = interprete.listaSimbolos();
                    MessageBox.Show("cadena valida");
                }
                else
                    MessageBox.Show("cadena invalida");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR AL COMPILAR EL CODIGO");
            }
        }

        private void AST_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (interprete != null)
                {
                    if (interprete.raiz != null)
                    {
                        Grafica grafica = new Grafica();
                        grafica.graficar(interprete.raiz);
                    }
                    else
                    {
                        MessageBox.Show("ERRORES EN EL CODIGO");
                    }
                }
                else
                {
                    MessageBox.Show("NO HAY CONTENIDO ANALIZADO");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR AL GRAFICAR");
            }
        }

        private void Errores_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (interprete != null)
                {
                    if (interprete.errores.Count > 0)
                    {
                        Reporte reporte = new Reporte();
                        reporte.reporteErrores(interprete.errores);
                    }
                    else
                    {
                        MessageBox.Show("CODIGO SIN ERRORES");
                    }
                }
                else
                {
                    MessageBox.Show("NO HAY CONTENIDO ANALIZADO");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR AL REPORTAR ERRORES");
            }
        }

        private void inicializacionVariablesLocales()
        {
            archivo = new Archivo();
            contadorTabs = 1;
            removiendoTab = false;
        }
    }
}
