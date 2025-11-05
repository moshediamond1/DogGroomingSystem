-- SQL View: Appointment Details View
-- This view provides a comprehensive view of all appointments with user details

CREATE OR ALTER VIEW vw_AppointmentDetails
AS
SELECT 
    a.Id AS AppointmentId,
    a.UserId,
    u.Username,
    u.FirstName AS CustomerName,
    a.AppointmentTime,
    a.DogSize,
    CASE a.DogSize
        WHEN 1 THEN 'Small'
        WHEN 2 THEN 'Medium'
        WHEN 3 THEN 'Large'
        ELSE 'Unknown'
    END AS DogSizeText,
    a.DurationMinutes,
    a.Price,
    a.FinalPrice,
    a.DiscountApplied,
    CASE 
        WHEN a.DiscountApplied = 1 THEN (a.Price - a.FinalPrice)
        ELSE 0
    END AS DiscountAmount,
    a.CreatedAt AS BookingCreatedAt,
    CASE 
        WHEN a.AppointmentTime < GETUTCDATE() THEN 'Completed'
        WHEN a.AppointmentTime >= GETUTCDATE() AND 
             a.AppointmentTime <= DATEADD(DAY, 1, GETUTCDATE()) THEN 'Today'
        ELSE 'Upcoming'
    END AS AppointmentStatus,
    DATEDIFF(DAY, a.CreatedAt, a.AppointmentTime) AS DaysInAdvance
FROM Appointments a
INNER JOIN Users u ON a.UserId = u.Id;
GO
