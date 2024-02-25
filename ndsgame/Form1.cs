using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net.Http;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using fnt;
using System.Net;
using System.Diagnostics;

namespace ndsgame
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            
            textBoxBuscar.Name = "txtBusqueda";

            // Asignar nombre a un ListView
            listView1.Name = "lstJuegos";

            // Asignar nombre a un Button
            buttonDescargar.Name = "btnDescargar";
        }
        static void Descomprimir7Zip(string archivo7zip, string directorioDestino)
        {
            using (var archivo = SevenZipArchive.Open(archivo7zip))
            {
                foreach (var entrada in archivo.Entries)
                {
                    if (!entrada.IsDirectory)
                    {
                        entrada.WriteToDirectory(directorioDestino, new SharpCompress.Common.ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }
            private void Form1_Load(object sender, EventArgs e)
        {
            // Agregar columnas al ListView
            listView1.Columns.Add("Name", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("URL", -2, HorizontalAlignment.Left);

            // Agregar más columnas según sea necesario

            CargarDatos();



            BuscarYMostrarResultados("");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dwFolderobj.ShowDialog();
            if(dwFolderobj.ShowDialog() == DialogResult.OK)
            {
                dwFolder.Text = dwFolderobj.SelectedPath;
            }
        }

        List<KeyValuePair<string, string>> juegos = new List<KeyValuePair<string, string>>();

        // Cargar datos desde archivo
        void CargarDatos()
        {
            var lineas = File.ReadAllLines("datos.txt");
            foreach (var linea in lineas)
            {
                var partes = linea.Split('|');
                if (partes.Length == 2)
                {
                    juegos.Add(new KeyValuePair<string, string>(partes[0], partes[1]));
                }
            }
        }

       
        async Task BuscarYMostrarResultados(string busqueda)
        {
            var resultadosFiltrados = juegos
                .Where(juego => juego.Value.IndexOf(busqueda, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(juego => juego.Value.IndexOf(busqueda, StringComparison.OrdinalIgnoreCase))
                .ToList();

            listView1.Items.Clear(); // Asumiendo que estás usando un ListView

            foreach (var juego in resultadosFiltrados)
            {
                var item = new ListViewItem(juego.Value);
                item.Tag = juego.Key; // Guarda la URL en el Tag para usarla después
                listView1.Items.Add(item);
            }
            
            
        }
        
        
        private void textBoxBuscar_TextChangedAsync(object sender, EventArgs e)
        {
             BuscarYMostrarResultados(textBoxBuscar.Text);
            
            
        }

        static async void DescargarArchivo(string url, string destino)
        {
            try
            {
               web.dwFile(url, destino);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al ejecutar curl: {ex.Message}");
            }
        }

        async void DescargarArchivoSeleccionado()
        {
           
            if (listView1.SelectedItems.Count > 0)
            {
                var selectedItem = listView1.SelectedItems[0];
                string url = (string)selectedItem.Tag;
                string nombreArchivo = selectedItem.Text + ".7z"; // Ajusta según necesites
               MessageBox.Show(url + " to: " + dwFolder.Text + @"\" + nombreArchivo);

                DescargarArchivo(url, dwFolder.Text + @"\" + nombreArchivo);
                Directory.CreateDirectory(dwFolder.Text + @"\nds");
                    Descomprimir7Zip(dwFolder.Text + @"\" + nombreArchivo, dwFolder.Text + @"\nds");
                MessageBox.Show("Done!");
                   
                
            }
        }

        private void buttonDescargar_Click(object sender, EventArgs e)
        {
            Thread dwd = new Thread(DescargarArchivoSeleccionado);
            dwd.Start();
        }
    }
}
