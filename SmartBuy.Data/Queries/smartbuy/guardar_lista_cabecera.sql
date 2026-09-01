-- Paso 1 del guardado (de 2): renombra la lista (validando pertenencia:
-- 0 filas = no existe o no es tuya) y vacía sus ítems. El paso 2
-- (insertar_lista_items.sql) los recarga. Van en sentencias separadas porque
-- las CTEs data-modifying no garantizan orden DELETE→INSERT sobre la misma
-- tabla y el unique (lista_id, producto_id) podría chocar al re-guardar.
WITH duenio AS (
    UPDATE lista
    SET nombre = @nombre,
        fecha_modificacion = NOW()
    WHERE id = @listaid
      AND usuario_id = @usuarioid
    RETURNING id
),
vaciado AS (
    DELETE FROM lista_item
    WHERE lista_id IN (SELECT id FROM duenio)
)
SELECT id
FROM duenio
