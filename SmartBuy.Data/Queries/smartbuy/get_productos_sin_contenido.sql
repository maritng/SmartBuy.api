-- Productos activos sin contenido cargado: el insumo de CompletarContenidos
-- (el ContenidoParser propone valor+unidad desde el nombre).
SELECT id,
       nombre
FROM producto
WHERE activo = true
  AND contenido_valor IS NULL
ORDER BY id
