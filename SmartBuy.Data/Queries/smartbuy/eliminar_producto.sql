-- Baja lógica (ver 03-producto-baja-logica.sql). 0 filas => no existe o ya
-- estaba de baja.
UPDATE producto
SET activo             = false,
    fecha_modificacion = NOW()
WHERE id = @id
  AND activo = true
RETURNING id
