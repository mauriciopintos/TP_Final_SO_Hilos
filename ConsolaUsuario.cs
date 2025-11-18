using System;
using System.IO;

public static class ConsolaUsuario
{
    public static bool TryObtenerConfiguracion(string[] args, out ConfigProcesador config)
    {
        config = null!;

        string ruta;
        int hilos;
        int opcion;

        if (args.Length < 1)
        {
            // Modo interactivo
            Console.WriteLine("""
            
            *****************************************
            *** Procesamiento de imagen con hilos ***
            *****************************************

            """);
            Console.Write("Ingrese el nombre de la imagen (con extensión), ubicada en .\\assets\\: ");
            string nombreImagen = Console.ReadLine() ?? string.Empty;

            ruta = Path.Combine(".", "assets", nombreImagen);

            if (!File.Exists(ruta))
            {
                Console.WriteLine($"No se encontró la imagen en el path: {ruta}");
                return false;
            }

            Console.Write("Ingrese la cantidad de hilos a emplear: ");
            while (!int.TryParse(Console.ReadLine(), out hilos) || hilos < 1)
            {
                Console.Write("Valor inválido. Ingrese un entero >= 1: ");
            }

            opcion = LeerOpcionMenu();
        }
        else
        {
            // Modo por argumentos
            ruta = args[0];
            hilos = (args.Length >= 2 && int.TryParse(args[1], out var n))
                ? Math.Max(1, n)
                : Math.Max(1, Environment.ProcessorCount);

            if (!File.Exists(ruta))
            {
                Console.WriteLine($"No se encontró la imagen en el path: {ruta}");
                return false;
            }

            opcion = LeerOpcionMenu();
        }

        config = new ConfigProcesador(ruta, hilos, opcion);
        return true;
    }

    private static int LeerOpcionMenu()
    {

        Console.WriteLine("""
        Seleccione qué procesar:
        1- RGB (Análisis global de los colores)
        2- R (Análisis de Rojo)
        3- G (Análisis de Verde)
        4- B (Análisis de Azul)
        Opción:
        """);

        int opcion;
        while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < 1 || opcion > 4)
        {
            Console.Write("Opción inválida. Ingrese 1, 2, 3 o 4: ");
        }

        return opcion;
    }
}
