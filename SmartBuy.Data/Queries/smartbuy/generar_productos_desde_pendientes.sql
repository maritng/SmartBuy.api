-- Generación masiva de productos desde la cola de pendientes, agrupando por
-- EAN (identidad global real: si varias cadenas publican el mismo código, es
-- un producto concreto del mundo).
--
--   * Solo EANs presentes en al menos @mincadenas cadenas distintas (con 1
--     entra todo; con 2 solo lo ya comparable).
--   * Nombre provisorio: el publicado más corto entre las cadenas (heurística:
--     el texto más corto suele ser el más limpio). Marca/categoría/contenido
--     quedan null: eso es curación posterior (curado = false).
--   * ON CONFLICT sobre el unique de ean saltea los que ya existen.
-- Devuelve cuántos productos se crearon. El re-matcheo de las publicaciones
-- lo dispara el servicio con MatchearPendientesPorEan (mecanismo ya existente).
WITH candidatos AS (
    SELECT ean_publicado AS ean
    FROM publicacion
    WHERE estado_matching = 'pendiente'
      AND producto_id IS NULL
      AND ean_publicado IS NOT NULL
    GROUP BY ean_publicado
    HAVING COUNT(DISTINCT cadena_id) >= @mincadenas
),
nombre_elegido AS (
    SELECT DISTINCT ON (p.ean_publicado)
           p.ean_publicado   AS ean,
           p.nombre_publicado AS nombre
    FROM publicacion p
    JOIN candidatos c ON c.ean = p.ean_publicado
    WHERE p.estado_matching = 'pendiente'
    ORDER BY p.ean_publicado, length(p.nombre_publicado), p.nombre_publicado, p.id
),
ins AS (
    INSERT INTO producto (nombre, ean, curado)
    SELECT n.nombre, n.ean, false
    FROM nombre_elegido n
    ON CONFLICT (ean) DO NOTHING
    RETURNING id
)
SELECT COUNT(*) AS cantidad
FROM ins
