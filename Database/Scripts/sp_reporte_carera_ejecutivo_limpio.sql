DELIMITER //

CREATE PROCEDURE sp_job_generar_historico_limpio()
BEGIN
    DECLARE v_fecha_corte DATE;
    SET v_fecha_corte = LAST_DAY(CURDATE());

    -- 1. Limpieza de memoria temporal
    DROP TEMPORARY TABLE IF EXISTS TmpPrestamosLimpio;
    DROP TEMPORARY TABLE IF EXISTS TmpCobrosLimpio;
    DROP TEMPORARY TABLE IF EXISTS TmpAmortizaLimpio;

    -- 2. Universo de Préstamos Activos con la lógica CASE para ejecutivos y nuevos filtros
    CREATE TEMPORARY TABLE TmpPrestamosLimpio AS
    SELECT
        Socios.S_CLAVE,
        Prestamo.PQ_CLAVE,
        Prestamo.PQ_IMPORTE,
        IFNULL(u.NombreUsuario, 'Sin Agrupar') AS ejecutivo_asignado
    FROM Pobla
             INNER JOIN Socios ON Socios.S_GRUPO = Pobla.P_CLAVE
             INNER JOIN Prestamo ON Prestamo.S_CLAVE = Socios.S_CLAVE
             LEFT JOIN autentificacion.grupo_asignado ga ON ga.S_GRUPO = Socios.S_GRUPO
             LEFT JOIN autentificacion.Usuarios u ON ga.usuario_id = u.Id
    WHERE Pobla.P_CLAVE BETWEEN 990000 AND 994999
      AND Prestamo.PQ_FECHA_LIQUIDACION = '9999-12-31'
      -- TUS NUEVOS FILTROS
      AND Prestamo.PQ_LITIGIO <> 1
      AND Prestamo.PQ_CART_VENCIDA <> 1
      AND Prestamo.PQ_DOCUMENTO_RENOVO = 0;

    -- Añadimos índices temporales para dar velocidad a los cálculos posteriores
    ALTER TABLE TmpPrestamosLimpio ADD INDEX idx_pq_clave (PQ_CLAVE);

    -- 3. Cobros (solo de los préstamos limpios)
    CREATE TEMPORARY TABLE TmpCobrosLimpio AS
    SELECT C.PQ_CLAVE, SUM(C.CO_IMPORTE) AS total_cobrado
    FROM Cobros C
             INNER JOIN TmpPrestamosLimpio PA ON C.PQ_CLAVE = PA.PQ_CLAVE
    GROUP BY C.PQ_CLAVE;

    -- 4. Amortizaciones Vencidas (solo de los préstamos limpios)
    CREATE TEMPORARY TABLE TmpAmortizaLimpio AS
    SELECT A.PQ_CLAVE, SUM(A.A_IMPORTE) AS capital_vencido
    FROM Amortiza A
             INNER JOIN TmpPrestamosLimpio PA ON A.PQ_CLAVE = PA.PQ_CLAVE
    WHERE A.A_FECHA_LIQUIDACION = '9999-12-31'
      AND A.A_FECHA_VENCIMIENTO <= CURDATE()
    GROUP BY A.PQ_CLAVE;

    -- 5. Inserción en el Histórico
    INSERT INTO autentificacion.reporte_cartera_historico
    (fecha_corte, ejecutivo_asignado, total_clientes, total_prestamos, monto_colocado, saldo_cartera, capital_vencido, saldo_final, porcentaje_calidad,tipo)
    SELECT
        v_fecha_corte,
        PA.ejecutivo_asignado,
        COUNT(DISTINCT PA.S_CLAVE),
        COUNT(DISTINCT PA.PQ_CLAVE),
        SUM(PA.PQ_IMPORTE),
        SUM(PA.PQ_IMPORTE - IFNULL(C.total_cobrado, 0)),
        SUM(IFNULL(A.capital_vencido, 0)),
        SUM(PA.PQ_IMPORTE - IFNULL(C.total_cobrado, 0)) - SUM(IFNULL(A.capital_vencido, 0)),
        ((SUM(PA.PQ_IMPORTE - IFNULL(C.total_cobrado, 0)) - SUM(IFNULL(A.capital_vencido, 0)))
            / NULLIF(SUM(PA.PQ_IMPORTE - IFNULL(C.total_cobrado, 0)), 0)) * 100,
        2
    FROM TmpPrestamosLimpio PA
             LEFT JOIN TmpCobrosLimpio C ON PA.PQ_CLAVE = C.PQ_CLAVE
             LEFT JOIN TmpAmortizaLimpio A ON PA.PQ_CLAVE = A.PQ_CLAVE
    GROUP BY PA.ejecutivo_asignado;

    -- 6. Limpieza
    DROP TEMPORARY TABLE IF EXISTS TmpPrestamosLimpio;
    DROP TEMPORARY TABLE IF EXISTS TmpCobrosLimpio;
    DROP TEMPORARY TABLE IF EXISTS TmpAmortizaLimpio;

END //
DELIMITER ;