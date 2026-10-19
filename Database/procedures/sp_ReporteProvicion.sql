DELIMITER $$
CREATE DEFINER=`prosigo`@`%` PROCEDURE `sp_ReporteProvicion`(IN p_lista_prestamos text, IN p_fecha_inicio DATE, IN p_fecha_fin DATE)
BEGIN
    DECLARE v_fecha_corte DATE;
    SET v_fecha_corte = COALESCE(p_fecha_fin, CURDATE());

    SELECT
        S.S_GRUPO,
        PB.P_NOMBRE AS GRUPO,
        P.PQ_CLAVE AS PRESTAMO,
        P.PQ_FECHA_OPERACION AS CREDITO_OTORGADO,
        P.PQ_FECHA_LIQUIDACION AS CREDITO_LIQUIDADO,
        A.A_NUMERO AS AMORTIZA_NUMERO,
        A.A_FECHA_INICIO AS AMORTIZA_INICIO,
        A.A_FECHA_VENCIMIENTO AS AMORTIZA_VENCIMIENTO,
        S.S_CLAVE,
        S.S_NOMBRE,
        A.A_NUMERO AS PRIMERA_AMORTIZACION,
        A.A_IMPORTE AS IMPORTE_AMORTIZACION,
        IFNULL(SUM(CO.CO_IMPORTE),0) AS ABONADO,
        (A.A_IMPORTE-IFNULL(SUM(CO.CO_IMPORTE),0)) AS SALDO_AMORTIZACION,

        /* ============================================================
           CÁLCULOS DE INTERÉS E IVA POR AMORTIZACIÓN (PHP)
           ============================================================ */
        -- Interés correspondiente a cada amortización:
        -- ((PQ_IMPORTE * (TASA / 100)) / 360 * DIAS) / AMORT_TOTAL
        ROUND(
            (P.PQ_IMPORTE * (P.PQ_PTOS_TASA_NORMAL / 100.0) / 360.0 * DATEDIFF(P.PQ_FECHA_VENCIMIENTO, P.PQ_FECHA_OPERACION))
            / NULLIF(COALESCE(P.PQ_NUM_AMORTIZACIONES, 0) + COALESCE(P.PQ_AMORTIZACION_GRACIA, 0), 0)
        , 2) AS INTERES_AMORTIZACION,

        -- IVA correspondiente al interés de cada amortización:
        -- INTERES_AMORTIZACION * PQ_IVA
        ROUND(
            ROUND(
                (P.PQ_IMPORTE * (P.PQ_PTOS_TASA_NORMAL / 100.0) / 360.0 * DATEDIFF(P.PQ_FECHA_VENCIMIENTO, P.PQ_FECHA_OPERACION))
                / NULLIF(COALESCE(P.PQ_NUM_AMORTIZACIONES, 0) + COALESCE(P.PQ_AMORTIZACION_GRACIA, 0), 0)
            , 2) * COALESCE(P.PQ_IVA, 0)
        , 2) AS IVA_AMORTIZACION,
        DATEDIFF(v_fecha_corte, A.A_FECHA_VENCIMIENTO) AS DIAS_ATRASO,
        FLOOR(DATEDIFF(v_fecha_corte, A.A_FECHA_VENCIMIENTO)/30) AS MESES_ATRASO,
        (
            SELECT MAX(C2.CO_FECHA_COBRO)
            FROM Cobros C2
            WHERE C2.PQ_CLAVE=P.PQ_CLAVE
        ) AS ULTIMO_PAGO,
        DATEDIFF(
            v_fecha_corte,
            (
                SELECT MAX(C2.CO_FECHA_COBRO)
                FROM Cobros C2
                WHERE C2.PQ_CLAVE=P.PQ_CLAVE
            )
        ) AS DIAS_SIN_PAGAR,
        CASE
            WHEN P.PQ_FECHA_OPERACION >= DATE_SUB(v_fecha_corte, INTERVAL 3 MONTH)
                THEN 'CREDITO RECIENTE'
            WHEN EXISTS (
                SELECT 1
                FROM Cobros C3
                WHERE C3.PQ_CLAVE=P.PQ_CLAVE
                  AND C3.CO_IMPORTE>0
                  AND C3.CO_FECHA_COBRO >= DATE_SUB(v_fecha_corte, INTERVAL 3 MONTH)
            ) THEN 'SI'
            ELSE 'NO'
        END AS PAGO_SOSTENIDO
    FROM Prestamo P
    INNER JOIN Socios S ON S.S_CLAVE=P.S_CLAVE
    INNER JOIN Pobla PB ON PB.P_CLAVE=S.S_GRUPO
    INNER JOIN Amortiza A ON A.PQ_CLAVE=P.PQ_CLAVE
    LEFT JOIN Cobros CO
        ON CO.PQ_CLAVE=A.PQ_CLAVE
       AND CO.A_NUMERO=A.A_NUMERO
    WHERE
    (
        GREATEST
        (
            DATE_ADD(A.A_FECHA_INICIO,INTERVAL 1 DAY),
            p_fecha_inicio
        )
        <
        LEAST
        (
            A.A_FECHA_VENCIMIENTO,
            p_fecha_fin
        )
    )
    AND FIND_IN_SET(P.PQ_CLAVE, p_lista_prestamos) > 0

    GROUP BY
        P.PQ_CLAVE,S.S_CLAVE,S.S_NOMBRE,A.A_NUMERO
    ORDER BY
        S.S_CLAVE,S.S_NOMBRE,MESES_ATRASO DESC,DIAS_ATRASO DESC,S.S_GRUPO;
END$$
DELIMITER ;
