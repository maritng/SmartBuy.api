-- Pasada 2 del recálculo: aplica el factor de una promo por cantidad (que
-- OfertaCalculator reconoció en C#) a todas las filas con ese descriptor.
-- LEAST: la promo solo puede mejorar el efectivo, nunca empeorarlo.
WITH upd AS (
    UPDATE precio
    SET precio_efectivo = LEAST(
            COALESCE(precio_efectivo, precio_lista),
            ROUND(precio_lista * CAST(@factor AS numeric), 2))
    WHERE tipo_oferta = @tipooferta
    RETURNING id
)
SELECT COUNT(*) AS cantidad
FROM upd
