using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MigTransfer
{
    public class ImageLoader
    {
        public async Task<List<string>> LoadImageUrlsAsync()
        {
            var baseUrl = GlobalSettings.URL_base;

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                MessageBox.Show("La URL base no está configurada correctamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Obtener el listado de directorios desde el servidor
                    var response = await httpClient.GetStringAsync(baseUrl);

                    // Extraer los nombres de los directorios usando una expresión regular
                    var directoryNames = ExtractDirectoriesFromHtml(response);

                    // Construir las URLs de los Cover.jpg, convirtiendo solo los espacios en %20
                    var imageUrls = directoryNames
                        .Select(dir => $"{baseUrl}{ConvertSpacesToEncoded(FixAmpersandInUrl(dir))}/Cover.jpg")
                        .ToList();

                    // Imprimir las URLs generadas para depuración
                    foreach (var url in imageUrls)
                    {
                        Console.WriteLine($"URL generada: {url}");
                    }

                    return imageUrls;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las URLs de las imágenes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }
        }

        // Función para convertir solo los espacios por %20
        private string ConvertSpacesToEncoded(string input)
        {
            // Reemplazar solo los espacios por %20
            return input.Replace(" ", "%20");
        }

        // Función para mantener el carácter & tal como está, sin convertirlo en &amp;
        private string FixAmpersandInUrl(string input)
        {
            // Asegurarse de que el & no se convierta en &amp;
            return input.Replace("&amp;", "&");
        }

        private List<string> ExtractDirectoriesFromHtml(string html)
        {
            var directories = new List<string>();

            // Usar una expresión regular para buscar enlaces de directorios en el listado HTML
            var regex = new Regex(@"<a href=""([^""]+)/"">", RegexOptions.IgnoreCase);
            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    directories.Add(match.Groups[1].Value);
                }
            }

            return directories;
        }

    }
}