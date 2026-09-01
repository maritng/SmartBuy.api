-- Panel de capturas: la bitácora de los bots, corridas más recientes primero.
-- La duración solo existe para capturas terminadas (fecha_fin null = en curso).
SELECT c.id,
       c.cadena_id,
       ca.nombre AS cadena,
       c.fuente,
       c.estado,
       c.fecha_inicio,
       c.fecha_fin,
       c.cant_items,
       c.error_detalle,
       CAST(EXTRACT(EPOCH FROM (c.fecha_fin - c.fecha_inicio)) AS int) AS duracion_segundos
FROM captura c
JOIN cadena ca ON ca.id = c.cadena_id
ORDER BY c.id DESC
LIMIT CAST(@limite AS int)
