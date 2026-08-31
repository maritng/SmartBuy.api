-- Re-matcheo retroactivo por EAN: publicaciones 'pendiente' cuyo ean_publicado
-- coincide con un producto ACTIVO del catálogo pasan a 'auto_ean'. Cubre el
-- caso "el producto se creó después de la captura" (el matching de la ingesta
-- solo corre al capturar).
--
--   * @ean con valor: re-matchea solo ese EAN (hook del ABM al crear/editar
--     un producto). En null: todos (endpoint global a demanda).
--   * Nunca toca 'descartada' ni matchings previos: solo 'pendiente' sin
--     producto. No crea productos: el catálogo crece solo curado.
-- Devuelve cuántas publicaciones engancharon.
WITH upd AS (
    UPDATE publicacion pub
    SET producto_id        = pr.id,
        estado_matching    = 'auto_ean',
        fecha_modificacion = NOW()
    FROM producto pr
    WHERE pub.estado_matching = 'pendiente'
      AND pub.producto_id IS NULL
      AND pub.ean_publicado IS NOT NULL
      AND pr.ean = pub.ean_publicado
      AND pr.activo = true
      AND (CAST(@ean AS text) IS NULL OR pub.ean_publicado = CAST(@ean AS text))
    RETURNING pub.id
)
SELECT COUNT(*) AS cantidad
FROM upd
