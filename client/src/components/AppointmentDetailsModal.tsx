import { format } from 'date-fns';
import type { Appointment } from '../types';

interface Props {
  appointment: Appointment;
  onClose: () => void;
}

export default function AppointmentDetailsModal({ appointment, onClose }: Props) {
  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content details-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Appointment Details</h2>
          <button onClick={onClose} className="close-btn">&times;</button>
        </div>

        <div className="details-grid">
          <div className="detail-item">
            <label>Customer Name</label>
            <p>{appointment.customerName}</p>
          </div>

          <div className="detail-item">
            <label>Appointment Time</label>
            <p>{format(new Date(appointment.appointmentTime), 'MMMM dd, yyyy - HH:mm')}</p>
          </div>

          <div className="detail-item">
            <label>Dog Size</label>
            <p>{appointment.dogSize}</p>
          </div>

          <div className="detail-item">
            <label>Duration</label>
            <p>{appointment.durationMinutes} minutes</p>
          </div>

          <div className="detail-item">
            <label>Base Price</label>
            <p>₪{appointment.price}</p>
          </div>

          <div className="detail-item">
            <label>Final Price</label>
            <p className={appointment.discountApplied ? 'discount-applied' : ''}>
              ₪{appointment.finalPrice}
            </p>
          </div>

          <div className="detail-item">
            <label>Discount Applied</label>
            <p>
              {appointment.discountApplied ? (
                <span className="badge badge-success">Yes - 10% OFF</span>
              ) : (
                <span>No</span>
              )}
            </p>
          </div>

          <div className="detail-item">
            <label>Booking Created At</label>
            <p>{format(new Date(appointment.createdAt), 'MMMM dd, yyyy - HH:mm:ss')}</p>
          </div>
        </div>

        <div className="modal-actions">
          <button onClick={onClose} className="btn btn-primary">Close</button>
        </div>
      </div>
    </div>
  );
}
