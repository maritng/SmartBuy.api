-- Historia de precios de un producto: por día y cadena, el mejor precio del
-- día. @conpromos elige la lectura: true = precio efectivo (promos por
-- cantidad incluidas), false = precio de góndola pagando una unidad (lista u
-- oferta directa) — la serie limpia de "nivel de precios". El filtro por fecha
-- permite el partition pruning (precio está particionada por mes).
SELECT p.fecha,
       pub.cadena_id,
       ca.nombre AS cadena,
       MIN(CASE WHEN CAST(@conpromos AS boolean)
                THEN COALESCE(p.precio_efectivo, LEAST(p.precio_lista, COALESCE(p.precio_oferta, p.precio_lista)))
                ELSE LEAST(p.precio_lista, COALESCE(p.precio_oferta, p.precio_lista))
           END) AS precio
FROM precio p
JOIN publicacion pub ON pub.id = p.publicacion_id
JOIN cadena ca ON ca.id = pub.cadena_id
WHERE pub.producto_id = @productoid
  AND p.fecha >= CURRENT_DATE - CAST(@dias AS int)
GROUP BY p.fecha, pub.cadena_id, ca.nombre
ORDER BY p.fecha, pub.cadena_id
