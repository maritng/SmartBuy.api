-- Evolución por categoría: los eslabones diarios del índice encadenado.
-- Por publicación, el mejor precio de cada día (puede haber 2 capturas/día);
-- LAG trae su observación anterior dentro de la ventana. Cada eslabón
-- (categoria, fecha) compara la CANASTA COMÚN: la suma de precios de hoy vs.
-- la suma de esos MISMOS ítems en su observación previa — así el catálogo
-- puede crecer o achicarse sin distorsionar el índice. El encadenado a base
-- 100 lo hace el servicio (IndiceCategoria).
-- @conpromos: true = precio efectivo (promos incluidas), false = góndola
-- (sin el serrucho de promos que entran y salen: nivel de precios limpio).
WITH diarios AS (
    SELECT pub.categoria_captura AS categoria,
           p.publicacion_id,
           p.fecha,
           MIN(CASE WHEN CAST(@conpromos AS boolean)
                    THEN COALESCE(p.precio_efectivo, LEAST(p.precio_lista, COALESCE(p.precio_oferta, p.precio_lista)))
                    ELSE LEAST(p.precio_lista, COALESCE(p.precio_oferta, p.precio_lista))
               END) AS precio
    FROM precio p
    JOIN publicacion pub ON pub.id = p.publicacion_id
    WHERE pub.categoria_captura IS NOT NULL
      AND p.fecha >= CURRENT_DATE - CAST(@dias AS int)
    GROUP BY 1, 2, 3
),
con_previo AS (
    SELECT categoria,
           fecha,
           precio,
           LAG(precio) OVER (PARTITION BY publicacion_id, categoria ORDER BY fecha) AS precio_previo
    FROM diarios
)
SELECT categoria,
       fecha,
       SUM(precio_previo) AS suma_previa,
       SUM(precio)        AS suma_actual,
       COUNT(*)::int      AS publicaciones
FROM con_previo
WHERE precio_previo IS NOT NULL
GROUP BY categoria, fecha
ORDER BY categoria, fecha
