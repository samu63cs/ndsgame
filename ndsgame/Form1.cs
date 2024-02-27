using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using fnt;
using System.Security.Policy;


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
            //listView1.Columns.Add("URL", -2, HorizontalAlignment.Left);

            // Agregar más columnas según sea necesario

          web.dwFile("https://raw.githubusercontent.com/samu63cs/ndsgame/main/datos.txt", "g.dt");

            CargarDatos();
           File.Delete("g.dt");


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
            var lineas = File.ReadAllLines("g.dt");
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
             
            
            
        }

        static async void DescargarArchivo(string url, string destino)
        {
            try
            {
               web.dwFile(url, destino);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async void DescargarArchivoSeleccionado()
        {
           
            if (listView1.SelectedItems.Count > 0)
            {
                var selectedItem = listView1.SelectedItems[0];
                string url = (string)selectedItem.Tag;
                string nombreArchivo = selectedItem.Text + ".7z"; // Ajusta según necesites
               MessageBox.Show("Downloading: " + nombreArchivo);

                DescargarArchivo(url, dwFolder.Text + @"\" + nombreArchivo);
               // Directory.CreateDirectory(dwFolder.Text + @"\nds");
                    Descomprimir7Zip(dwFolder.Text + @"\" + nombreArchivo, dwFolder.Text);
                
                File.Delete(dwFolder.Text + @"\" + nombreArchivo);
                MessageBox.Show("Done!");
                   
                
            }
        }

        private void buttonDescargar_Click(object sender, EventArgs e)
        {
            Thread dwd = new Thread(DescargarArchivoSeleccionado);
            dwd.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BuscarYMostrarResultados(textBoxBuscar.Text);
        }
        List<String> listUrl = new List<String>();
        List<String> listNam = new List<String>();



        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var selectedItem = listView1.SelectedItems[0];
                string url = (string)selectedItem.Tag;
                string nombreArchivo = selectedItem.Text + ".7z"; // Ajusta según necesites
                listNam.Add(nombreArchivo);
                listUrl.Add(url);
               


            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string listNamp = "";
            foreach (string name in listNam)
            {
                listNamp += name + Environment.NewLine;
            }
            MessageBox.Show(listNamp);
            Thread listDw = new Thread(listDownloader);
            listDw.Start();
        }

        private void listDownloader()
        {

            int index = 0;
            foreach (string nombreArchivo in listNam) 
            {
                try
                {


                    string url = listUrl[index];
                    pinf(index + 1, listNam.Count, $"Downloading: {nombreArchivo}");
                    DescargarArchivo(url, dwFolder.Text + @"\" + nombreArchivo);

                    pinf(index + 1, listNam.Count, $"Inflating: {nombreArchivo}");
                    Descomprimir7Zip(dwFolder.Text + @"\" + nombreArchivo, dwFolder.Text);


                    File.Delete(dwFolder.Text + @"\" + nombreArchivo);
                    pinf(index + 1, listNam.Count, $"Done: {nombreArchivo}");

                    index++;
                } catch(Exception ex) {
                    pinf(index + 1, listNam.Count, $"Error ({nombreArchivo}): {ex.Message}");
                }
            }
            pinf(index, listNam.Count, $"Done!");
            listNam.Clear();
            listUrl.Clear();
        }


        void pinf(int s, int e, string msg)
        {
            info.Text = $" ({s} / {e}) - {msg}";
        }
    }
}
