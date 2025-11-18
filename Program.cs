class Program
{
    static void Main(string[] args)
    {

        //var config = null!;

        // Modo interactivo
        Console.WriteLine("=== Procesamiento de imagen con hilos ===");
        
        // Definir path
        Console.Write("Ingrese el nombre de la imagen (con extensión), ubicada en .\\assets\\: ");
        // string nombreImagen = Console.ReadLine() ?? string.Empty;
        string nombreImagen = "imagen1.jpg";
        string ruta = Path.Combine(".", "assets", nombreImagen);

        // Definir cantidad de hilos  (default prueba es 4)
        int hilos;

        // Definir opcion de analisis (default prueba es rojo)
        int opcion=2;


        var config = new ConfigProcesador
         {
            Ruta = ruta,
            Hilos = hilos,
            Opcion = opcion
        };

        ProcesadorImagen.Procesar(config);
    }
}