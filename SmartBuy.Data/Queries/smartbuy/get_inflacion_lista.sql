-- Inflación personal: para cada día de la ventana, el mejor precio efectivo
-- de cada producto de la lista (entre todas las cadenas). El servicio suma por
-- cantidad y detecta cobertura. El filtro por usuario_id en el subquery es el
-- anti-IDOR en la capa de datos: lista ajena = cero filas.
-- @conpromos: true = precio efectivo (promos incluidas), false = góndola.
SELECT p.fecha,
       pub.producto_id,
       MIN(CASE WHEN CAST(@conpromos AS boolean)
                THEN COALESCE(p.precio_efectivo, LEAST(p.precio_lista, COALESCE(p.precio_oferta, p.precio_lista)))
                ELSE LEAST(p.precio_lista, COALESCE(p.precio_oferta, p.precio_lista))
           END) AS precio
FROM precio p
JOIN publicacion pub ON pub.id = p.publicacion_id
WHERE pub.producto_id IN (
          SELECT li.producto_id
          FROM lista_item li
          JOIN lista l ON l.id = li.lista_id
          WHERE l.id = @listaid
            AND l.usuario_id = @usuarioid
      )
  AND p.fecha >= CURRENT_DATE - CAST(@dias AS int)
GROUP BY p.fecha, pub.producto_id
ORDER BY p.fecha
