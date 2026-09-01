-- Completa el contenido propuesto por el parser. El AND contenido_valor IS NULL
-- garantiza que jamás pisa un contenido cargado a mano (curación gana siempre).
UPDATE producto
SET contenido_valor    = @contenidovalor,
    contenido_unidad   = @contenidounidad,
    fecha_modificacion = NOW()
WHERE id = @id
  AND contenido_valor IS NULL
RETURNING id
