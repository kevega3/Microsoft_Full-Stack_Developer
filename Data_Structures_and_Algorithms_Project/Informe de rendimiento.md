# Informe de rendimiento

## Complejidad antes y después

| Componente | Enfoque de referencia | Implementación final | Resultado esperado |
|---|---:|---:|---|
| Árbol binario | Árbol sin balanceo: peor caso `O(n)` en búsqueda | AVL: `O(log n)` en búsqueda e inserción | La altura se mantiene logarítmica mediante rotaciones |
| Planificación | Ordenar toda la lista en cada ejecución: `O(n log n)` repetido | `PriorityQueue`: `O(log n)` por inserción/extracción | No se reordena la colección completa |
| Ordenación | Inserción: `O(n²)` | `Array.Sort`: `O(n log n)` promedio | Mejor crecimiento para entradas grandes |
| Ejecución | Errores sin aislar podían detener el lote | `try/catch` por tarea y registro | Una tarea fallida no interrumpe las siguientes |

## Medición reproducible

El programa `Demo` genera 20.000 enteros con semilla `2026`, clona la misma entrada para cada algoritmo y mide una ejecución con `Stopwatch`.

Salida obtenida en la ejecución de entrega:

```text
AVL: 1000 elementos, altura 10, balanceado: True
Ejecucion: 2 correctas, 1 con error
Registro: Task 'publicar-resultados' failed: Servicio no disponible
Benchmark (20,000 elementos): insercion O(n2) 400.37 ms; Array.Sort O(n log n) 11.94 ms
```

La medición depende del procesador, carga del sistema y versión del runtime. La complejidad asintótica es la comparación principal; los milisegundos sirven como evidencia local reproducible.

## Validaciones funcionales

`dotnet test` comprueba que:

- El AVL mantiene el orden, evita duplicados, encuentra valores y queda balanceado.
- La cola extrae primero la mayor prioridad y resuelve empates por fecha.
- La ordenación optimizada produce el mismo orden esperado.
- El ejecutor registra el error y continúa con la siguiente tarea.
