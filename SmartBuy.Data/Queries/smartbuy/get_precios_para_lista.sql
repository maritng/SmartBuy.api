-- Consulta central de la app: para cada producto de la lista, el mejor precio
-- vigente EN CADA cadena. El servicio elige después el mínimo por producto
-- (reparto óptimo) y arma los totales por cadena única.
--
--   * Solo publicaciones matcheadas (auto_ean/auto_nombre/manual) de productos
--     activos: las 'pendiente' y 'descartada' no participan.
--   * precio_efectivo = menor entre lista y oferta directa. Las promos tipo
--     "2x1" están en tipo_oferta pero todavía no se computan: se devuelven
--     crudas para mostrar (la fórmula de normalización es mejora futura y,
--     como el dato se guarda crudo, aplicará retroactivamente).
--   * Si una cadena tiene varias publicaciones del mismo producto, gana la más
--     barata (ROW_NUMBER por producto x cadena).
--   * @productoids viaja como CSV parametrizado y se abre con string_to_array:
--     un solo parámetro, sin depender del manejo de arrays del binder.
--   * @cadenasids (CSV, nullable): restringe el universo a las cadenas
--     accesibles para el usuario. En null, todas. Filtrar acá y no en C#
--     mantiene intacta la lógica de agregación del servicio.
WITH candidatos AS (
    SELECT pr.id             AS producto_id,
           pr.nombre         AS producto,
           pr.contenido_valor,
           pr.contenido_unidad,
           pub.cadena_id,
           cad.nombre        AS cadena,
           pub.id            AS publicacion_id,
           pub.nombre_publicado,
           pv.fecha,
           pv.precio_lista,
           pv.precio_oferta,
           pv.tipo_oferta,
           -- precio_efectivo persistido (incluye promos por cantidad computadas);
           -- COALESCE cubre filas históricas aún sin recalcular.
           COALESCE(pv.precio_efectivo, LEAST(pv.precio_lista, COALESCE(pv.precio_oferta, pv.precio_lista))) AS precio_efectivo,
           ROW_NUMBER() OVER (
               PARTITION BY pr.id, pub.cadena_id
               ORDER BY COALESCE(pv.precio_efectivo, LEAST(pv.precio_lista, COALESCE(pv.precio_oferta, pv.precio_lista))),
                        pv.fecha DESC,
                        pub.id
           ) AS rn
    FROM producto pr
    JOIN publicacion pub   ON pub.producto_id = pr.id
                          AND pub.estado_matching IN ('auto_ean', 'auto_nombre', 'manual')
    JOIN cadena cad        ON cad.id = pub.cadena_id
    JOIN precio_vigente pv ON pv.publicacion_id = pub.id
    WHERE pr.activo = true
      AND pr.id = ANY (string_to_array(@productoids, ',')::bigint[])
      AND (CAST(@cadenasids AS text) IS NULL
           OR pub.cadena_id = ANY (string_to_array(CAST(@cadenasids AS text), ',')::bigint[]))
)
SELECT producto_id,
       producto,
       contenido_valor,
       contenido_unidad,
       cadena_id,
       cadena,
       publicacion_id,
       nombre_publicado,
       fecha,
       precio_lista,
       precio_oferta,
       tipo_oferta,
       precio_efectivo
FROM candidatos
WHERE rn = 1
ORDER BY producto_id, precio_efectivo, cadena_id
