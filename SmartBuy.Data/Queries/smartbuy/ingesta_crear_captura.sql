-- Abre una captura en 'en_proceso'. Se cierra con ingesta_finalizar_captura.sql
-- ('ok' o 'error'): una captura que queda en 'en_proceso' delata un bot caído.
INSERT INTO captura (cadena_id, fuente, estado)
VALUES (@cadenaid, @fuente, 'en_proceso')
RETURNING id
