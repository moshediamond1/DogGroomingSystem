import { useState, FormEvent } from 'react';
import { appointmentService } from '../services/api';
import { DogSize } from '../types';
import type { Appointment } from '../types';

interface Props {
  appointment: Appointment | null;
  onClose: () => void;
}

export default function AppointmentModal({ appointment, onClose }: Props) {
  const [appointmentTime, setAppointmentTime] = useState(
    appointment ? appointment.appointmentTime.slice(0, 16) : ''
  );
  const [dogSize, setDogSize] = useState<DogSize>(
    appointment ? getDogSizeEnum(appointment.dogSize) : DogSize.Small
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  function getDogSizeEnum(size: string): DogSize {
    switch (size) {
      case 'Small': return DogSize.Small;
      case 'Medium': return DogSize.Medium;
      case 'Large': return DogSize.Large;
      default: return DogSize.Small;
    }
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const data = {
        appointmentTime: new Date(appointmentTime).toISOString(),
        dogSize
      };

      if (appointment) {
        await appointmentService.update(appointment.id, data);
      } else {
        await appointmentService.create(data);
      }
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save appointment');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{appointment ? 'Edit Appointment' : 'New Appointment'}</h2>
          <button onClick={onClose} className="close-btn">&times;</button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="appointmentTime">Appointment Time</label>
            <input
              id="appointmentTime"
              type="datetime-local"
              value={appointmentTime}
              onChange={(e) => setAppointmentTime(e.target.value)}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="dogSize">Dog Size</label>
            <select
              id="dogSize"
              value={dogSize}
              onChange={(e) => setDogSize(Number(e.target.value) as DogSize)}
              disabled={loading}
            >
              <option value={DogSize.Small}>Small (30 min - ₪100)</option>
              <option value={DogSize.Medium}>Medium (45 min - ₪150)</option>
              <option value={DogSize.Large}>Large (60 min - ₪200)</option>
            </select>
          </div>

          {error && <div className="error-message">{error}</div>}

          <div className="modal-actions">
            <button type="button" onClick={onClose} className="btn btn-secondary" disabled={loading}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
