# Reflexión sobre la contribución del LLM

Microsoft Copilot se utilizó como apoyo de revisión y diseño, no como sustituto de las pruebas.

- **Árbol binario:** ayudó a identificar que un árbol de búsqueda sin balanceo puede degradarse a `O(n)` y propuso las rotaciones AVL. Las pruebas verificaron la altura y el recorrido ordenado.
- **Planificación:** ayudó a reemplazar la ordenación repetida por una cola de prioridad y a definir un desempate determinista por fecha y secuencia de inserción.
- **Ordenación:** señaló que la ordenación por inserción es `O(n²)` y que `Array.Sort` proporciona una implementación optimizada del runtime con crecimiento `O(n log n)` promedio.
- **Ejecución:** ayudó a localizar el riesgo de detener todo el lote ante una excepción y a separar el resultado exitoso del resultado fallido, registrando el mensaje sin perder las tareas restantes.
- **Rendimiento:** sugirió usar `Stopwatch`, una entrada reproducible y copias de la misma colección para evitar comparar datos distintos.

La validación final se realizó con pruebas xUnit y una ejecución independiente del benchmark. Las sugerencias del LLM fueron revisadas contra las complejidades y los casos límite del enunciado.
