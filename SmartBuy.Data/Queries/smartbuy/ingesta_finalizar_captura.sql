-- Cierra la captura con su resultado. error_detalle solo lleva el resumen del
-- fallo (qué ítem), nunca mensajes internos del motor: el log completo queda
-- del lado de la aplicación.
UPDATE captura
SET estado        = @estado,
    fecha_fin     = NOW(),
    cant_items    = @cantitems,
    error_detalle = CAST(@errordetalle AS text)
WHERE id = @capturaid
