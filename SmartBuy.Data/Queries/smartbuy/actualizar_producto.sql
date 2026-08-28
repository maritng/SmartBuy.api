-- Solo se editan productos activos. 0 filas => no existe o está de baja:
-- ProductoServices lo traduce a un mensaje claro.
UPDATE producto
SET nombre             = @nombre,
    marca_id           = CAST(@marcaid AS bigint),
    categoria_id       = CAST(@categoriaid AS bigint),
    contenido_valor    = CAST(@contenidovalor AS numeric),
    contenido_unidad   = CAST(@contenidounidad AS text),
    ean                = CAST(@ean AS text),
    fecha_modificacion = NOW()
WHERE id = @id
  AND activo = true
RETURNING id
