-- Auto-saneo del orquestador: una captura 'en_proceso' con más de
-- @horasmaximas horas es un bot que murió a mitad de camino (crash, kill,
-- corte). Se cierra como 'error' con detalle honesto y la cantidad de precios
-- que alcanzó a insertar (que son válidos: la ingesta es atómica por ítem).
WITH upd AS (
    UPDATE captura
    SET estado        = 'error',
        fecha_fin     = NOW(),
        cant_items    = (SELECT COUNT(*) FROM precio p WHERE p.captura_id = captura.id),
        error_detalle = 'Abandonada: quedo en_proceso mas de ' || CAST(@horasmaximas AS text)
                        || ' horas (bot caido o proceso detenido). Los precios ya insertados son validos.'
    WHERE estado = 'en_proceso'
      AND fecha_inicio < NOW() - make_interval(hours => CAST(@horasmaximas AS int))
    RETURNING id
)
SELECT COUNT(*) AS cantidad
FROM upd
