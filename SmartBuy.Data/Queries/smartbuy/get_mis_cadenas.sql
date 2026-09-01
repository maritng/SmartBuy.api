-- Preferencia "mis cadenas" del usuario. Sin filas = todas.
SELECT cadena_id
FROM usuario_cadena
WHERE usuario_id = @usuarioid
ORDER BY cadena_id
