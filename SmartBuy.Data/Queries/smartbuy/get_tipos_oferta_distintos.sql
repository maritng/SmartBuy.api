-- Los descriptores de promo distintos del histórico: el recálculo itera por
-- tipo (pocos textos únicos), no por fila.
SELECT DISTINCT tipo_oferta
FROM precio
WHERE tipo_oferta IS NOT NULL
ORDER BY tipo_oferta
