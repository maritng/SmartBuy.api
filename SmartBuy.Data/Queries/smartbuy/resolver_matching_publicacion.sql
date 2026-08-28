-- Resuelve una publicación de la cola: 'manual' (con producto) o 'descartada'
-- (sin producto). El WHERE sobre 'pendiente' protege contra dos revisores
-- pisándose: 0 filas => ya fue resuelta (o no existe) y el servicio lo informa.
-- El estado lo fija el servicio (nunca viene del cliente) y el check
-- publicacion_matching_coherente de la tabla garantiza la coherencia
-- estado/producto.
UPDATE publicacion
SET producto_id        = CAST(@productoid AS bigint),
    estado_matching    = @estado,
    fecha_modificacion = NOW()
WHERE id = @publicacionid
  AND estado_matching = 'pendiente'
RETURNING id
