# Proyecto de Estructuras de Datos y Algoritmos

Entrega consolidada de las cuatro actividades descritas en `intrucciones.md`.

## Contenido

- `Algorithms/AvlTree.cs`: árbol AVL con inserción, rotaciones, búsqueda y recorrido en orden.
- `Algorithms/TaskScheduling.cs`: cola de prioridad con prioridad descendente, fecha ascendente y orden estable.
- `Algorithms/Sorting.cs`: ordenación por inserción como referencia `O(n²)` y `Array.Sort` optimizado `O(n log n)`.
- `Algorithms/TaskScheduling.cs`: ejecución resistente a errores, resultados por tarea y registro de excepciones.
- `Algorithms.Tests/AlgorithmsTests.cs`: pruebas automatizadas de los comportamientos principales.
- `Informe de rendimiento.md`: análisis de complejidad y mediciones reproducibles.
- `Reflexion LLM.md`: contribución de Microsoft Copilot/LLM.

## Ejecución

```bash
dotnet test
dotnet run --project Demo
```

El benchmark es orientativo: usa una única ejecución en la máquina local y debe repetirse varias veces para comparar hardware o versiones del runtime.
