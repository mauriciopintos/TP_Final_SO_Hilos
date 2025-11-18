using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

public static class ProcesadorImagen
{
    private static readonly object bloqueoConsola = new object();

    public static void Procesar(ConfigProcesador config)
    {
        // Cargar la imagen original desde la ruta de la configuración
        using var bmpOriginal = new Bitmap(config.Ruta);
        
        // Aseguro formato 24bpp para acceso lineal BGR
        using var bitmapProcesada =
            bmpOriginal.PixelFormat == PixelFormat.Format24bppRgb
                ? (Bitmap)bmpOriginal.Clone()
                : bmpOriginal.Clone(
                    new Rectangle(0, 0, bmpOriginal.Width, bmpOriginal.Height),
                    PixelFormat.Format24bppRgb
                  );

        var rectangulo = new Rectangle(0, 0, bitmapProcesada.Width, bitmapProcesada.Height);
        var timerGlobal = Stopwatch.StartNew();

        // Con LockBits se mapea el buffer de la imagen en memoria
        BitmapData datosBitmap = bitmapProcesada.LockBits(
            rectangulo,
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb
        );

        try
        {
            int ancho = datosBitmap.Width;
            int alto = datosBitmap.Height;
            int paso = datosBitmap.Stride; // bytes por fila (puede tener padding)
            IntPtr punteroPrimeraFila = datosBitmap.Scan0;

            int cantHilos = config.Hilos;

            long[] parcialR   = new long[cantHilos];
            long[] parcialG  = new long[cantHilos];
            long[] parcialB   = new long[cantHilos];
            long[] cantPixParcial = new long[cantHilos];

            Thread[] hilosTrabajadores = new Thread[cantHilos];

            for (int indiceHilo = 0; indiceHilo < cantHilos; indiceHilo++)
            {
                // Renombro los hilos con fines didácticos para que se vea secuencial
                int idx = indiceHilo;
                int idHilo = idx + 1; // Hilo 01..N

                int filaInicio = (idx * alto) / cantHilos;
                int filaFin    = ((idx + 1) * alto) / cantHilos;

                hilosTrabajadores[indiceHilo] = new Thread(() =>
                {
                    var timerLocal = Stopwatch.StartNew();

                    lock (bloqueoConsola)
                    {
                        Console.WriteLine(
                            $"[Hilo {idHilo:00} - {ControlTiempo.MarcaTiempo()}] INICIO -> procesando filas [{filaInicio}, {filaFin})"
                        );
                    }

                    unsafe
                    {
                        byte* punteroBase = (byte*)punteroPrimeraFila.ToPointer();
                        long sumaR = 0;
                        long sumaG = 0;
                        long sumaB = 0;
                        long cantPix = 0;

                        for (int fila = filaInicio; fila < filaFin; fila++)
                        {
                            byte* punteroFila = punteroBase + (fila * paso);

                            // Formato 24bpp: B(0), G(1), R(2)
                            for (int columna = 0; columna < ancho; columna++)
                            {
                                int desplazamiento = columna * 3;

                                byte B  = punteroFila[desplazamiento + 0];
                                byte G = punteroFila[desplazamiento + 1];
                                byte R  = punteroFila[desplazamiento + 2];

                                sumaR  += R;
                                sumaG += G;
                                sumaB  += B;
                                cantPix++;
                            }
                        }

                        parcialR[idx]   = sumaR;
                        parcialG[idx]  = sumaG;
                        parcialB[idx]   = sumaB;
                        cantPixParcial[idx] = cantPix;

                        timerLocal.Stop();

                        double promRLocal  = cantPix > 0 ? (double)sumaR  / cantPix : 0;
                        double promGLocal = cantPix > 0 ? (double)sumaG / cantPix : 0;
                        double promBLocal  = cantPix > 0 ? (double)sumaB  / cantPix : 0;

                        lock (bloqueoConsola)
                        {
                            Console.WriteLine(
                                $"[Hilo {idHilo:00} - {ControlTiempo.MarcaTiempo()}] FIN -> filas [{filaInicio}, {filaFin}), px: {cantPix:N0}, " +
                                $"prom parciales RGB: R={promRLocal:F2} G={promGLocal:F2} B={promBLocal:F2}, " +
                                $"tiempo: {timerLocal.ElapsedMilliseconds} ms"
                            );
                        }
                    }
                });

                hilosTrabajadores[indiceHilo].IsBackground = true;
                hilosTrabajadores[indiceHilo].Start();
            }

            // Esperar a todos los hilos
            foreach (var hilo in hilosTrabajadores)
            {
                hilo.Join();
            }

            // Reducir resultados globales
            long sumaRTotal  = 0;
            long sumaGTotal = 0;
            long sumaBTotal  = 0;
            long cantPixTotal = 0;

            for (int i = 0; i < cantHilos; i++)
            {
                sumaRTotal  += parcialR[i];
                sumaGTotal += parcialG[i];
                sumaBTotal  += parcialB[i];
                cantPixTotal += cantPixParcial[i];
            }

            double promRGlobal  = cantPixTotal > 0 ? (double)sumaRTotal  / cantPixTotal : 0.0;
            double promGGlobal = cantPixTotal > 0 ? (double)sumaGTotal / cantPixTotal : 0.0;
            double promBGlobal  = cantPixTotal > 0 ? (double)sumaBTotal  / cantPixTotal : 0.0;

            timerGlobal.Stop();

            ImprimirResultadoFinal(
                config,
                ancho,
                alto,
                promRGlobal,
                promGGlobal,
                promBGlobal,
                timerGlobal.ElapsedMilliseconds
            );
        }
        finally
        {
            bitmapProcesada.UnlockBits(datosBitmap);
        }
    }

    private static void ImprimirResultadoFinal(
        ConfigProcesador config,
        int ancho,
        int alto,
        double promR,
        double promG,
        double promB,
        long tiempoEnMs)
    {
        Console.WriteLine();
        Console.WriteLine($"Imagen: {config.Ruta}");
        Console.WriteLine($"Dimensiones: {ancho}x{alto}");
        Console.WriteLine($"Hilos empleados: {config.Hilos}");

        switch (config.Opcion)
        {
            case 1:
                Console.WriteLine(
                    $"prom GLOBAL RGB: R={promR:F2} G={promG:F2} B={promB:F2} (0..255)"
                );
                break;
            case 2:
                Console.WriteLine($"prom R (R): {promR:F2} (0..255)");
                break;
            case 3:
                Console.WriteLine($"prom G (G): {promG:F2} (0..255)");
                break;
            case 4:
                Console.WriteLine($"prom B (B): {promB:F2} (0..255)");
                break;
        }

        Console.WriteLine($"Tiempo total: {tiempoEnMs} ms");
    }
}
