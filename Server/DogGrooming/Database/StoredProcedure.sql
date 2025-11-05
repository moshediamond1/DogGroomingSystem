-- Stored Procedure: Get User Appointment Statistics
-- This procedure calculates statistics for a specific user

CREATE OR ALTER PROCEDURE GetUserAppointmentStats
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.Id AS UserId,
        u.Username,
        u.FirstName,
        COUNT(a.Id) AS TotalAppointments,
        COUNT(CASE WHEN a.AppointmentTime < GETUTCDATE() THEN 1 END) AS CompletedAppointments,
        COUNT(CASE WHEN a.AppointmentTime >= GETUTCDATE() THEN 1 END) AS UpcomingAppointments,
        SUM(a.FinalPrice) AS TotalSpent,
        AVG(a.FinalPrice) AS AverageSpent,
        MAX(a.AppointmentTime) AS LastAppointmentDate,
        CASE 
            WHEN COUNT(CASE WHEN a.AppointmentTime < GETUTCDATE() THEN 1 END) > 3 THEN 1
            ELSE 0
        END AS IsEligibleForDiscount
    FROM Users u
    LEFT JOIN Appointments a ON u.Id = a.UserId
    WHERE u.Id = @UserId
    GROUP BY u.Id, u.Username, u.FirstName;
END
GO
