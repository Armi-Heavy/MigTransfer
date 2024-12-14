using System.Text.RegularExpressions;

public static class NaturalOrderSorter
{
    public static IEnumerable<string> SortPathsNaturally(IEnumerable<string> paths)
    {
        // Asegurarse de que paths no contiene valores nulos y manejar posibles excepciones
        return paths
            .Where(path => !string.IsNullOrEmpty(path)) // Ignorar valores nulos o vacíos
            .OrderBy(path => {
                string fileName = Path.GetFileNameWithoutExtension(path) ?? string.Empty;
                return ParseForNaturalOrder(fileName);
            });
    }

    private static IEnumerable<object> ParseForNaturalOrder(string input)
    {
        // Divide la cadena en partes numéricas y alfabéticas
        var chunks = Regex.Split(input, "([0-9]+)").Select(chunk =>
        {
            if (int.TryParse(chunk, out int number))
                return (object)number; // Convertir números en enteros para comparación natural
            return chunk; // Mantener texto como cadenas
        });
        return chunks;
    }
}
