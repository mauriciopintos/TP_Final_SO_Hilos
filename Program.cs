class Program
{
    static void Main(string[] args)
    {
        Console.Clear(); // limpiar consola al inicio

        if (!ConsolaUsuario.TryObtenerConfiguracion(args, out var config))
        {
            // Hubo algún error (imagen no encontrada o entrada inválida)
            return;
        }

        ProcesadorImagen.Procesar(config);
    }
}