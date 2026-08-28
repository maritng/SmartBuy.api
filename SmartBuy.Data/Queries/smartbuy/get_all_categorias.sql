-- Árbol de categorías aplanado (cada fila con el nombre de su padre): el FE
-- arma el árbol/agrupado a partir de padre_id.
SELECT c.id,
       c.nombre,
       c.padre_id,
       p.nombre AS padre
FROM categoria c
LEFT JOIN categoria p ON p.id = c.padre_id
ORDER BY COALESCE(p.nombre, c.nombre), c.padre_id NULLS FIRST, c.nombre
