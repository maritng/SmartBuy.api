-- Historia de precios de un producto: por día y cadena, el mejor precio
-- efectivo capturado ese día. El filtro por fecha permite el partition pruning
-- (precio está particionada por mes). COALESCE cubre filas previas a la
-- columna precio_efectivo.
SELECT p.fecha,
       pub.cadena_id,
       ca.nombre AS cadena,
       MIN(COALESCE(p.precio_efectivo, LEAST(p.precio_lista, COALESCE(p.precio_oferta, p.precio_lista)))) AS precio
FROM precio p
JOIN publicacion pub ON pub.id = p.publicacion_id
JOIN cadena ca ON ca.id = pub.cadena_id
WHERE pub.producto_id = @productoid
  AND p.fecha >= CURRENT_DATE - CAST(@dias AS int)
GROUP BY p.fecha, pub.cadena_id, ca.nombre
ORDER BY p.fecha, pub.cadena_id
