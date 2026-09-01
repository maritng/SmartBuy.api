-- Registra un ítem de captura en UNA sentencia atómica (o entra todo o nada):
--   1. Upsert de la publicación por (cadena_id, codigo_externo).
--   2. Matching automático por EAN: si el EAN publicado existe en el catálogo
--      maestro, la publicación nace/queda vinculada con estado 'auto_ean'.
--      Nunca pisa un matching previo (manual, auto_nombre) ni una 'descartada':
--      solo promociona publicaciones 'pendiente' sin producto asignado.
--   3. Inserta el precio del día contra la publicación y la captura.
-- Devuelve el id de la publicación y su estado de matching resultante.
-- Los parámetros nullables van con CAST explícito: si llegan NULL, Postgres no
-- puede inferir el tipo del placeholder y corta con 42P08.
WITH candidato AS (
    SELECT p.id AS producto_id
    FROM producto p
    WHERE CAST(@eanpublicado AS text) IS NOT NULL
      AND p.ean = CAST(@eanpublicado AS text)
),
pub AS (
    INSERT INTO publicacion (cadena_id, codigo_externo, nombre_publicado, ean_publicado, url, producto_id, estado_matching)
    SELECT @cadenaid,
           @codigoexterno,
           @nombrepublicado,
           CAST(@eanpublicado AS text),
           CAST(@url AS text),
           c.producto_id,
           CASE WHEN c.producto_id IS NOT NULL THEN 'auto_ean' ELSE 'pendiente' END
    FROM (SELECT (SELECT producto_id FROM candidato) AS producto_id) c
    ON CONFLICT (cadena_id, codigo_externo) DO UPDATE
        SET nombre_publicado   = excluded.nombre_publicado,
            ean_publicado      = COALESCE(excluded.ean_publicado, publicacion.ean_publicado),
            url                = COALESCE(excluded.url, publicacion.url),
            producto_id        = CASE
                                     WHEN publicacion.producto_id IS NULL
                                          AND publicacion.estado_matching = 'pendiente'
                                          AND excluded.producto_id IS NOT NULL
                                     THEN excluded.producto_id
                                     ELSE publicacion.producto_id
                                 END,
            estado_matching    = CASE
                                     WHEN publicacion.producto_id IS NULL
                                          AND publicacion.estado_matching = 'pendiente'
                                          AND excluded.producto_id IS NOT NULL
                                     THEN 'auto_ean'
                                     ELSE publicacion.estado_matching
                                 END,
            fecha_modificacion = NOW()
    RETURNING id, estado_matching
),
pre AS (
    INSERT INTO precio (publicacion_id, captura_id, fecha, precio_lista, precio_oferta, tipo_oferta, precio_efectivo)
    SELECT pub.id, @capturaid, CURRENT_DATE, @preciolista,
           CAST(@preciooferta AS numeric), CAST(@tipooferta AS text),
           CAST(@precioefectivo AS numeric)
    FROM pub
)
SELECT pub.id AS publicacion_id,
       pub.estado_matching
FROM pub
