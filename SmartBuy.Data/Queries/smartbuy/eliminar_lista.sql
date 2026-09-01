-- Borrado físico (el cascade arrastra los ítems). El WHERE por usuario_id
-- es la defensa anti-IDOR; 0 filas = no existe o no es tuya.
DELETE FROM lista
WHERE id = @listaid
  AND usuario_id = @usuarioid
RETURNING id
