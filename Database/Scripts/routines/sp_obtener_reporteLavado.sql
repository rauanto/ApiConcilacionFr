create
    definer = raul@`%` procedure sp_obtener_reportesLavado(IN p_fecha_inicio date, IN p_fecha_fin date)
BEGIN
    SELECT DISTINCT
        Lavado.*,
        CASE
            WHEN Lavado.T_REPORTE = 1 THEN 'RELEVANTE'
            WHEN Lavado.T_REPORTE = 2 THEN 'INUSUAL'
            ELSE 'OTRO'
        END AS TIPO_REPORTE,
        Socios.S_NOMBRE,
        Prestamo.PQ_CLAVE,
        Pobla.P_NOMBRE
    FROM Lavado
    INNER JOIN Socios
        ON Lavado.S_CLAVE = Socios.S_CLAVE
    INNER JOIN Prestamo
        ON Socios.S_CLAVE = Prestamo.S_CLAVE
    INNER JOIN Pobla
        ON Socios.S_GRUPO = Pobla.P_CLAVE
    WHERE
        Lavado.T_FECHA >= p_fecha_inicio
        AND Lavado.T_FECHA < CURDATE() + INTERVAL 1 DAY
        -- AND Lavado.T_FECHA < p_fecha_fin + INTERVAL 1 DAY
        AND Lavado.T_REPORTE IN (1, 2)
    ORDER BY
        Lavado.T_FECHA ASC;
END;

