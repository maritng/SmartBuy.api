-- Las listas del usuario, la tocada más recientemente primero.
SELECT l.id,
       l.nombre,
       COUNT(i.id) AS cant_items,
       COALESCE(l.fecha_modificacion, l.fecha_creacion) AS fecha
FROM lista l
LEFT JOIN lista_item i ON i.lista_id = l.id
WHERE l.usuario_id = @usuarioid
GROUP BY l.id, l.nombre, l.fecha_modificacion, l.fecha_creacion
ORDER BY COALESCE(l.fecha_modificacion, l.fecha_creacion) DESC
