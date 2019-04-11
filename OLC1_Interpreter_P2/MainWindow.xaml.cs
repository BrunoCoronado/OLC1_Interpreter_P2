﻿using System;
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

        public MainWindow()
        {
            contadorTabs = 1;
            removiendoTab = false;
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
                        tabItem.Header = "Sin Titulo - " + ++contadorTabs;
                        tabItem.Name = "T" + contadorTabs;
                        //******FIN CREACION DEL NUEVO TAB******
                        //******CREACION DEL TEXTBOX PARA EL NUEVO TAB******
                        TextBox textBox = new TextBox();
                        textBox.Name = "txtT" + contadorTabs;
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
    }
}
