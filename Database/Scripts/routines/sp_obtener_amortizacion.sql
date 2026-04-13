create
    definer = root@localhost procedure sp_obtener_amortizacion(IN p_pq_clave int)
BEGIN
    SELECT
        A.A_NUMERO,
        DATE_FORMAT(A.A_FECHA_VENCIMIENTO, '%d-%m-%Y') AS FECHA_VENCIMIENTO,

        -- Capital
        FORMAT(A.A_IMPORTE, 2) AS CAPITAL,

        -- Saldo
        FORMAT(A.A_SALDO_INSOLUTO, 2) AS SALDO_INSOLUTO,

        -- Importe
        FORMAT(A.A_IMPORTE, 2) AS IMPORTE,

        -- Interés por amortización
        FORMAT(t.INTERES_X_AMORT, 2) AS INTERES,

        -- IVA
        FORMAT(t.IVA_X_AMORT, 2) AS IVA,

        -- Cuota
        FORMAT(t.CUOTA, 2) AS TOTAL,

        -- Validación de Estado
        CASE
            WHEN A.A_FECHA_LIQUIDACION != '9999-12-31' THEN 'Pagada'
            WHEN A.A_FECHA_VENCIMIENTO < CURDATE() THEN 'Vencida'
            ELSE 'No Pagada' -- Opcional: podrías cambiar esto a 'Vigente'
        END AS Estado

    FROM Amortiza A

    JOIN (
        SELECT
            PR.PQ_CLAVE,

            ROUND(
                (PR.PQ_IMPORTE * (PR.PQ_PTOS_TASA_NORMAL / 100) / 360 *
                DATEDIFF(PR.PQ_FECHA_VENCIMIENTO, PR.PQ_FECHA_OPERACION))
                / (PR.PQ_NUM_AMORTIZACIONES + PR.PQ_AMORTIZACION_GRACIA)
            ,2) AS INTERES_X_AMORT,

            ROUND(
                (
                    (PR.PQ_IMPORTE * (PR.PQ_PTOS_TASA_NORMAL / 100) / 360 *
                    DATEDIFF(PR.PQ_FECHA_VENCIMIENTO, PR.PQ_FECHA_OPERACION))
                    / (PR.PQ_NUM_AMORTIZACIONES + PR.PQ_AMORTIZACION_GRACIA)
                ) * PR.PQ_IVA
            ,2) AS IVA_X_AMORT,

            ROUND(
                A2.A_IMPORTE +
                (
                    (PR.PQ_IMPORTE * (PR.PQ_PTOS_TASA_NORMAL / 100) / 360 *
                    DATEDIFF(PR.PQ_FECHA_VENCIMIENTO, PR.PQ_FECHA_OPERACION))
                    / (PR.PQ_NUM_AMORTIZACIONES + PR.PQ_AMORTIZACION_GRACIA)
                ) +
                (
                    (
                        (PR.PQ_IMPORTE * (PR.PQ_PTOS_TASA_NORMAL / 100) / 360 *
                        DATEDIFF(PR.PQ_FECHA_VENCIMIENTO, PR.PQ_FECHA_OPERACION))
                        / (PR.PQ_NUM_AMORTIZACIONES + PR.PQ_AMORTIZACION_GRACIA)
                    ) * PR.PQ_IVA
                )
            ,1) AS CUOTA

        FROM Prestamo PR
        LEFT JOIN Amortiza A2
            ON A2.PQ_CLAVE = PR.PQ_CLAVE AND A2.A_NUMERO = 1

        -- Se reemplaza el valor estático por el parámetro
        WHERE PR.PQ_CLAVE = p_pq_clave
    ) t ON t.PQ_CLAVE = A.PQ_CLAVE

    -- Se reemplaza el valor estático por el parámetro
    WHERE A.PQ_CLAVE = p_pq_clave;
END;

