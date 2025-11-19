# TP Final de Sistemas Operativos - UNaHur 2025C2
## Proyecto de Procesamiento de Imágenes con Hilos  
---    

**Alumno:**     PINTOS, Mauricio  
**Docente:**    Ing. Gabriel Esquivel  
**Materia:**    Sistemas Operativos  
**Año:**        2025  
**Revisión:**   1.1  

---    

## Explicación completa del proyecto de procesamiento de imágenes con hilos en C#    

Este documento explica detalladamente cómo funciona el proyecto de **procesamiento de imágenes en paralelo** usando **hilos en C#**, pensado para el TP Final de la materia **Sistemas Operativos** de la **Universidad Nacional de Hurlingham (UNaHur)**.

La idea central es:

> **Tomar una imagen, dividir su procesamiento entre varios hilos, recorrer sus píxeles y calcular promedios de color (R, G, B), mostrando en consola qué hace cada hilo y cuánto tiempo tarda.**

---

# 🧩 Arquitectura general del proyecto

El código está modularizado en las siguientes clases:

| Archivo               | Rol principal                                                        |
|-----------------------|----------------------------------------------------------------------|
| `Program.cs`          | Punto de entrada. Maneja el flujo de ejecusión.                      |
| `ConsolaUsuario.cs`   | Maneja la interacción con el usuario y la validación de parámetros.  |
| `ConfigProcesador.cs` | Objeto inmutable con la configuración del procesamiento.             |
| `ControlTiempo.cs`    | Genera marcas de tiempo para los logs.                               |
| `ProcesadorImagen.cs` | Contiene toda la lógica de procesamiento en paralelo de la imagen.   |

---    

# 🧷 Program.cs - Punto de entrada del programa

**Archivo:** `Program.cs`

Este archivo implementa el método `Main`, que es el punto de entrada de la aplicación. La idea es que `Main` sea lo más chico y limpio posible:


### Explicación paso a paso

1. **`Console.Clear()`**  
   - Limpia la consola al iniciar el programa, para que la salida sea prolija.

2. **Obtención de configuración**  
   - Llama a `ConsolaUsuario.TryObtenerConfiguracion(args, out var config)`.  
   - Este método:
     - Valida la ruta de la imagen.
     - Pide (o toma de los argumentos) la cantidad de hilos.
     - Pide la opción de análisis (RGB / solo R / solo G / solo B).
   - Si algo sale mal (archivo no existe, etc.), devuelve `false` y el programa termina.

3. **Procesamiento real de la imagen**  
   - Si la configuración es válida, llama:
     ```csharp
     ProcesadorImagen.Procesar(config);
     ```
   - A partir de acá, todo el procesamiento pesado está encapsulado en `ProcesadorImagen`.

> La idea es que un `Main` minimalista se delegan responsabilidades y mejora la legibilidad del código.

---

# 🧠 ConsolaUsuario.cs - Entrada de datos y validación

**Archivo:** `ConsolaUsuario.cs`  
**Tipo:** `static class`

Esta clase se encarga de **"hablar" con el usuario** y de armar una instancia válida de `ConfigProcesador`.

El método principal es:

```csharp
public static bool TryObtenerConfiguracion(
            string[] args, out ConfigProcesador config
            )
```

### Patrón TryXxx y parámetro `out`

- Devuelve `bool` indicando si tuvo **éxito o fracasó** al obtener la configuración inicial.
- Usa un parámetro `out config` para devolver el objeto configuración.
- Tomé la idea del patrón que ***C#*** usa en métodos como `int.TryParse(...)`.

---

## 1) Modo interactivo (sin argumentos)

Cuando `args.Length < 1`, se entra al modo **interactivo**:

### Qué hace

1. Muestra un título en consola.
2. Pide el nombre de la imagen (solo el archivo con su extensión, la carpeta se asume `./assets/`).
3. Construye la ruta con `Path.Combine(".", "assets", nombreImagen)`.
4. Verifica que el archivo exista con `File.Exists(ruta)`.
5. Pide la cantidad de hilos:
   - Usa `int.TryParse` en un `while` para asegurarse de que sea un entero ≥ 1.
6. Llama a `LeerOpcionMenu()` para que el usuario elija qué analizar (RGB / R / G / B).

---

## 2) Modo por argumentos

Si el usuario ejecuta el programa con parámetros, por ejemplo:

```bash
dotnet run -- "./assets/imagen1.jpg" 4
```

se entra al camino del **`else`** y comienza a procesar:

### ¿Qué hace?

1. Toma la ruta directamente de `args[0]`.
2. Si hay un segundo argumento (`args[1]`), intenta usarlo como cantidad de hilos.
   - Si no hay segundo argumento o es inválido, usa:
     ```csharp
     Environment.ProcessorCount
     ```
     como valor base (cantidad de núcleos lógicos del equipo).
3. Siempre fuerza que el número de hilos sea al menos 1 con `Math.Max(1, n)`.
4. Valida que el archivo exista.
5. Llama otra vez a `LeerOpcionMenu()`.

---

## 3) Menú de opciones: `LeerOpcionMenu`

```bash
Seleccione qué procesar:
1- RGB (Análisis global de los colores)
2- R (Análisis de Rojo)
3- G (Análisis de Verde)
4- B (Análisis de Azul)
Opción:
```

### ¿Cómo funciona?

- Es un menú básico de consola de comando (CLI). El usuario interactua mediante el uso del teclado.
- Ustiliza un **bucle de validación** hasta que el usuario ingresa un valor correcto.
- División del código para que la lógica de menú esté separada del resto.

---

## 4) Construcción de la configuración

Al final de `TryObtenerConfiguracion`:

```csharp
config = new ConfigProcesador(
    ruta, hilos, opcion
    );
return true;
```

Se crea una instancia de `ConfigProcesador` con:

- Ruta de la imagen
- Cantidad de hilos
- Opción seleccionada

y se devuelve `true` para indicar que todo salió bien.

---

# 📦 ConfigProcesador.cs - Objeto de configuración inmutable

**Archivo:** `ConfigProcesador.cs`
>Se aplica el concepto de “objeto de configuración” o “DTO inmutable”. Creando un Objeto que no tiene setters, para que solo se pueda consultar con los getters los valores de la configuración, pero no pueda ser alterada en tiempo de ejecusión.

### Características clave

- Es un **contenedor de datos** con 3 propiedades:
  - `Ruta`: ubicación del archivo de imagen.
  - `Hilos`: cantidad de hilos a usar.
  - `Opcion`: qué canal(es) de color analizar.
- Las propiedades son **solo lectura** (`get;` sin `set;`):
  - Una vez creado el objeto, no se puede modificar → **inmutable**.
  - Útil para evitar efectos colaterales entre métodos.


---

# ⏱️ ControlTiempo.cs - Marcas de tiempo para logging

**Archivo:** `ControlTiempo.cs`

### ¿Qué hace?

- Devuelve un string con el formato:
  - `minuto:segundo.milisegundos` (por ejemplo, `03:27.152`).
- Se usa en `ProcesadorImagen` para loguear el **momento exacto** en que un hilo comienza y termina.
- Esto nos permite ver en que tiempo inicia y finaliza cada hilo, para tener una aproximación mas cercana a lo real de la concurrencia.

### Conceptos

- Formateo de fechas y horas con `ToString("formato")`.
- Herramienta simple pero muy útil para seguir la ejecución de hilos.

---

# 🧵 ProcesadorImagen.cs - Lógica de procesamiento en paralelo

**Archivo:** `ProcesadorImagen.cs`  
**Tipo:** `static class`

Acá está el corazón del TP: **procesamiento de la imagen en múltiples hilos**.

---

## 1) Campo de sincronización de consola

```csharp
private static readonly object bloqueoConsola = new object();
```

- Este objeto se usa como **candado** para `lock`.
- Garantiza que dos hilos no escriban en la consola al mismo tiempo y mezclen sus mensajes.

---

## 2) Método principal: `Procesar(ConfigProcesador config)`

```csharp
public static void Procesar(ConfigProcesador config)
```

### 2.1. Cargar y normalizar la imagen

1. **Carga de la imagen** con `new Bitmap(config.Ruta)`.
2. **Normalización de formato** a `PixelFormat.Format24bppRgb`:
   - Asegura que cada píxel son exactamente **3 bytes** (B, G, R).
3. **`LockBits`**:
   - Bloquea los píxeles en memoria.
   - Devuelve un `BitmapData` con información:
     - `Width`, `Height`
     - `Stride` (bytes por fila, incluyendo posible padding)
     - `Scan0` (puntero al inicio del buffer de la imagen).
4. Se inicia un `Stopwatch` para medir el tiempo total del procesamiento.

---

### 2.2. Preparar datos para los hilos

- `ancho`, `alto`: tamaño de la imagen en píxeles.
- `paso` (`Stride`): cuántos **bytes** ocupa una fila completa (puede ser mayor a `ancho * 3` por padding).
- `Scan0`: puntero al inicio del buffer de píxeles.
- Se crean arrays paralelos:
  - `parcialR[i]`, `parcialG[i]`, `parcialB[i]`, `cantPixParcial[i]`
  - Cada hilo escribe solo en su propia posición `i` → **evita condiciones de carrera**.
- `finally` garantiza que `UnlockBits` se ejecute aunque haya alguna excepción.

---

### 2.3. División del trabajo entre hilos

- Se crea un `Thread` por cada índice `idx`.
- A cada hilo se le asigna un **rango de filas**:
  - `filaInicio` inclusive.
  - `filaFin` exclusivo.
- De esta forma, la imagen se divide en **bandas horizontales**, una por hilo.
- Dentro de la lambda del hilo:
  - Se mide el tiempo local (`timerLocal`).
  - Se loguea el inicio del hilo usando `lock` para la consola.
  - Se usa una variable `idx` solo con fines didacticos, para que no asigne un id de hilo de manera automatica.

---

### 2.4. Procesamiento dentro del hilo (bloque `unsafe`)

Dentro del hilo, viene la parte de acceso a memoria cruda:

### Conceptos clave

1. **Bloque `unsafe`**  
   - Permite trabajar con punteros (`byte*`).
   - Se usa para acceder directamente al buffer de la imagen, sin overhead (por encima) de métodos de alto nivel.

2. **Cálculo de direcciones**  
   - `punteroBase`: inicio del buffer completo.
   - Para cada `fila`:
     ```csharp
     punteroFila = punteroBase + fila * paso;
     ```
   - Para cada `columna`:
     ```csharp
     desplazamiento = columna * 3;
     ```
   - Bytes:
     - `B` = `punteroFila[desplazamiento + 0]`
     - `G` = `... + 1`
     - `R` = `... + 2`

3. **Acumuladores locales por hilo**  
   - `sumaR`, `sumaG`, `sumaB`, `cantPix`.
   - Luego se copian a los arrays globales de resultados parciales.

4. **Cálculo de promedios locales**  
   - Cada hilo calcula su promedio de R, G, B para sus filas.

5. **Log de fin del hilo**  
   - Se imprime:
     - Rango de filas.
     - Cantidad de píxeles.
     - Promedios RGB del segmento.
     - Tiempo que tardó el hilo.

---

### 2.5. Espera de hilos (`Join`) y reducción global

Después del `for` que lanza los hilos:

### ¿Qué se hace acá?

1. **`Join()`**  
   - El hilo principal espera a que **todos** los hilos terminen.
   - Esto, evita imprimir resultados antes de tiempo.

2. **Reducción (sumatoria)**  
   - Se suman todos los valores parciales de cada hilo:
     - `parcialR[i]`, `parcialG[i]`, `parcialB[i]`, `cantPixParcial[i]`.
   - Utiloza un patrón **map–reduce**:
     - Cada hilo hace un *map* sobre una parte de los datos.
     - El hilo principal hace la *reduce* (sumatoria) de todos los resultados.

3. **Cálculo de promedios globales**  
   - `promRGlobal`, `promGGlobal`, `promBGlobal`.

4. **Tiempo total**  
   - `timerGlobal.Stop()` mide el tiempo global de todo el procesamiento.

5. **Llamado a `ImprimirResultadoFinal`**  
   - Se delega la presentación de resultados a un método separado.

---

### 2.6. Impresión del resultado final

```csharp
private static void ImprimirResultadoFinal(...) {...}
```

### ¿Qué muestra?

- Ruta de la imagen procesada.
- Dimensiones (ancho x alto).
- Cantidad de hilos utilizados.
- Según la opción elegida:
  - Opción 1: muestra promedios de R, G y B.
  - Opción 2: solo el canal R.
  - Opción 3: solo G.
  - Opción 4: solo B.
- Tiempo total de procesamiento, en milisegundos.

---

# 🧪 Cómo ejecutar y probar el proyecto

## 1) Modo interactivo

En la carpeta del proyecto:

```bash
dotnet run
```

Luego:

1. Ingresar el nombre de la imagen ubicada en `./assets`.
2. Ingresar la cantidad de hilos.
3. Elegir opción de análisis (1 a 4).

---

## 2) Modo por argumentos

```bash
dotnet run -- "./assets/imagen1.jpg" 4
```

- Primer argumento: ruta de la imagen.
- Segundo argumento: cantidad de hilos.

El programa igualmente va a pedir por consola la opción de análisis (RGB / R / G / B).

---

# 🎓 Conceptos de Sistemas Operativos trabajados

- **Hilos de ejecución**
  - Creación de hilos (`new Thread(...)`).
  - Sincronización con `Join()`.
  - Regiones críticas con `lock`.

- **Memoria**
  - Representación cruda de una imagen en memoria.
  - Stride (paso), píxel como estructura de bytes BGR.

- **Concurrencia**
  - División de trabajo en franjas horizontales.
  - Evitar condiciones de carrera al usar arrays separados por hilo.

- **Medición de performance**
  - Uso de `Stopwatch` para medir tiempo global y por hilo.
  - Comparar ejecución con 1, 2, 4, N hilos.

- **Diseño modular**
  - Separar:
    - entrada de datos (`ConsolaUsuario`),
    - configuración (`ConfigProcesador`),
    - lógica de negocio (`ProcesadorImagen`),
    - utilitarios (`ControlTiempo`),
    - punto de entrada (`Program`).

---

# ✔️ Conclusión

Este proyecto es un ejemplo completo para mostrar:

- Cómo procesar una imagen a nivel de píxel.
- Cómo repartir trabajo entre varios hilos.
- Cómo sincronizar salidas y combinar resultados parciales.
- Cómo medir y analizar tiempos de ejecución.

En resume, se intenta "bajar a tierra" los conceptos de **hilos, concurrencia y memoria** con un ejemplo visual y concreto.
