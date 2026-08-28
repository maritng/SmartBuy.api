-- Detalle de un producto para el form de edición. Devuelve también inactivos
-- (con su flag): el servicio decide qué permitir sobre ellos.
SELECT p.id,
       p.nombre,
       p.marca_id,
       m.nombre  AS marca,
       p.categoria_id,
       c.nombre  AS categoria,
       p.contenido_valor,
       p.contenido_unidad,
       p.ean,
       p.activo
FROM producto p
LEFT JOIN marca m     ON m.id = p.marca_id
LEFT JOIN categoria c ON c.id = p.categoria_id
WHERE p.id = @id
