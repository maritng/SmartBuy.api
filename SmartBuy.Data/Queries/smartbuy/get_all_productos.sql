-- Listado paginado del catálogo maestro (solo activos), con marca y categoría
-- resueltas. @filtro busca en nombre y marca, o por EAN exacto. La columna
-- total (window) evita un segundo round-trip para la paginación del FE.
SELECT p.id,
       p.nombre,
       p.marca_id,
       m.nombre  AS marca,
       p.categoria_id,
       c.nombre  AS categoria,
       p.contenido_valor,
       p.contenido_unidad,
       p.ean,
       COUNT(*) OVER () AS total
FROM producto p
LEFT JOIN marca m     ON m.id = p.marca_id
LEFT JOIN categoria c ON c.id = p.categoria_id
WHERE p.activo = true
  AND (CAST(@filtro AS text) IS NULL
       OR p.nombre ILIKE '%' || CAST(@filtro AS text) || '%'
       OR m.nombre ILIKE '%' || CAST(@filtro AS text) || '%'
       OR p.ean = CAST(@filtro AS text))
ORDER BY p.nombre, p.id
LIMIT @limit OFFSET @offset
