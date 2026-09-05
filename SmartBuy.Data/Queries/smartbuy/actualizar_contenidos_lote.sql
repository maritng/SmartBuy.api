-- Completa contenidos EN LOTE: tres CSV paralelos (ids, valores, unidades) que
-- unnest zipea fila a fila — UNA llamada para todo el catálogo. Nació del
-- incidente del 06/09/2026: el update de a uno (N+1) sobre ~900 productos
-- superaba el timeout del proxy y quedaba a medias. El AND contenido_valor IS
-- NULL garantiza que jamás pisa un contenido cargado a mano (curación gana
-- siempre). Devuelve cuántos completó de verdad.
WITH lote AS (
    SELECT *
    FROM unnest(
        string_to_array(@ids, ',')::bigint[],
        string_to_array(@valores, ',')::numeric[],
        string_to_array(@unidades, ',')::text[]
    ) AS t(producto_id, valor, unidad)
),
actualizados AS (
    UPDATE producto p
    SET contenido_valor    = l.valor,
        contenido_unidad   = l.unidad,
        fecha_modificacion = NOW()
    FROM lote l
    WHERE p.id = l.producto_id
      AND p.contenido_valor IS NULL
    RETURNING p.id
)
SELECT COUNT(*)::int AS cantidad
FROM actualizados
