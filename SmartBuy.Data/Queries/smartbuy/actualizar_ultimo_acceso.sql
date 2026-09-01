UPDATE usuario
SET ultimo_acceso = NOW()
WHERE id = @usuarioid
