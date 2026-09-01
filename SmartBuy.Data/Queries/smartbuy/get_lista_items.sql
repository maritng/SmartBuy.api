-- Una lista con sus ítems. El WHERE por usuario_id es la defensa anti-IDOR:
-- una lista ajena devuelve 0 filas, indistinguible de inexistente.
-- Lista vacía devuelve 1 fila con producto_id null (LEFT JOIN): el servicio
-- la mapea a lista sin ítems.
SELECT l.id       AS lista_id,
       l.nombre   AS lista_nombre,
       i.producto_id,
       p.nombre   AS producto,
       i.cantidad
FROM lista l
LEFT JOIN lista_item i ON i.lista_id = l.id
LEFT JOIN producto p   ON p.id = i.producto_id
WHERE l.id = @listaid
  AND l.usuario_id = @usuarioid
ORDER BY p.nombre NULLS LAST
