public class ConfigProcesador
{
    public ConfigProcesador(string ruta, int hilos, int opcion)
    {
        Ruta = ruta;
        Hilos = hilos;
        Opcion = opcion;
    }

    public string Ruta { get; }
    public int Hilos { get; }
    public int Opcion { get; }
}
