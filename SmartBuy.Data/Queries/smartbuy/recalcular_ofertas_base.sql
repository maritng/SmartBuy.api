-- Pasada 1 del recálculo: la fórmula base (min entre lista y oferta directa)
-- para TODO el histórico. La pasada 2 (por tipo de oferta computable) puede
-- solo bajar este valor.
WITH upd AS (
    UPDATE precio
    SET precio_efectivo = LEAST(precio_lista, COALESCE(precio_oferta, precio_lista))
    RETURNING id
)
SELECT COUNT(*) AS cantidad
FROM upd
