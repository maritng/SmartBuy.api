-- Cola de revisión de matching: publicaciones 'pendiente', las más viejas
-- primero, con todo lo que el revisor necesita para decidir (texto crudo, EAN
-- si vino y último precio conocido). Usa el índice parcial
-- publicacion_pendientes_idx. La columna total (window) alimenta la paginación.
SELECT pub.id,
       pub.cadena_id,
       cad.nombre        AS cadena,
       pub.codigo_externo,
       pub.nombre_publicado,
       pub.ean_publicado,
       pub.url,
       pub.fecha_creacion,
       pv.precio_lista   AS ultimo_precio_lista,
       pv.precio_oferta  AS ultimo_precio_oferta,
       pv.fecha          AS ultima_fecha_precio,
       COUNT(*) OVER ()  AS total
FROM publicacion pub
JOIN cadena cad ON cad.id = pub.cadena_id
LEFT JOIN precio_vigente pv ON pv.publicacion_id = pub.id
WHERE pub.estado_matching = 'pendiente'
  AND (CAST(@cadenaid AS bigint) IS NULL OR pub.cadena_id = CAST(@cadenaid AS bigint))
ORDER BY pub.fecha_creacion, pub.id
LIMIT @limit OFFSET @offset
