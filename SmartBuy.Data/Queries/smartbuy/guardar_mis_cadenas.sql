-- Reemplaza la preferencia completa en una sentencia atómica. @cadenasids
-- viaja como CSV (null o vacío = "todas": queda sin filas).
WITH borrado AS (
    DELETE FROM usuario_cadena
    WHERE usuario_id = @usuarioid
),
alta AS (
    INSERT INTO usuario_cadena (usuario_id, cadena_id)
    SELECT @usuarioid, x::bigint
    FROM unnest(string_to_array(CAST(@cadenasids AS text), ',')) AS x
    WHERE CAST(@cadenasids AS text) IS NOT NULL
      AND CAST(@cadenasids AS text) <> ''
    RETURNING cadena_id
)
SELECT COUNT(*) AS cantidad
FROM alta
