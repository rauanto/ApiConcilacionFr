-- Asegúrate de que el event scheduler esté encendido
SET GLOBAL event_scheduler = ON;

CREATE EVENT e_reporte_mensual_cartera
    ON SCHEDULE EVERY 1 MONTH
        STARTS LAST_DAY(CURDATE()) + INTERVAL 23 HOUR + INTERVAL 59 MINUTE
    DO
    CALL sp_job_generar_historico_cartera();


-- Esto ejecutará toda la lógica, calculará el cierre y lo guardará en la tabla histórica
CALL sp_job_generar_historico_cartera();