import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { appointmentService } from '../services/api';
import { format } from 'date-fns';
import type { Appointment, AppointmentFilters } from '../types';
import AppointmentModal from '../components/AppointmentModal';
import AppointmentDetailsModal from '../components/AppointmentDetailsModal';
import AppointmentFiltersComponent from '../components/AppointmentFilters';

export default function AppointmentList() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingAppointment, setEditingAppointment] = useState<Appointment | null>(null);
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null);
  const [filters, setFilters] = useState<AppointmentFilters>({});
  const { logout, user } = useAuth();

  const loadAppointments = async () => {
    try {
      setLoading(true);
      const data = await appointmentService.getAll(filters);
      setAppointments(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load appointments');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAppointments();
  }, [filters]);

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this appointment?')) return;

    try {
      await appointmentService.delete(id);
      loadAppointments();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to delete appointment');
    }
  };

  const handleEdit = (appointment: Appointment) => {
    setEditingAppointment(appointment);
    setShowModal(true);
  };

  const handleModalClose = () => {
    setShowModal(false);
    setEditingAppointment(null);
    loadAppointments();
  };

  const handleRowClick = (appointment: Appointment) => {
    setSelectedAppointment(appointment);
  };

  return (
    <div className="appointments-page">
      <header className="page-header">
        <div>
          <h1>Dog Grooming Appointments</h1>
          <p>Welcome, {user?.firstName || user?.username}!</p>
        </div>
        <div className="header-actions">
          <button onClick={() => setShowModal(true)} className="btn btn-primary">
            + New Appointment
          </button>
          <button onClick={logout} className="btn btn-secondary">
            Logout
          </button>
        </div>
      </header>

      <AppointmentFiltersComponent filters={filters} onFiltersChange={setFilters} />

      {loading && <div className="loading">Loading appointments...</div>}
      {error && <div className="error-message">{error}</div>}

      {!loading && appointments.length === 0 && (
        <div className="empty-state">
          <p>No appointments found. Create your first appointment!</p>
        </div>
      )}

      {!loading && appointments.length > 0 && (
        <div className="table-container">
          <table className="appointments-table">
            <thead>
              <tr>
                <th>Customer Name</th>
                <th>Appointment Time</th>
                <th>Dog Size</th>
                <th>Duration</th>
                <th>Price</th>
                <th>Final Price</th>
                <th>Discount</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {appointments.map((appointment) => (
                <tr 
                  key={appointment.id}
                  onClick={() => handleRowClick(appointment)}
                  className="clickable-row"
                >
                  <td>{appointment.customerName}</td>
                  <td>{format(new Date(appointment.appointmentTime), 'MMM dd, yyyy HH:mm')}</td>
                  <td>{appointment.dogSize}</td>
                  <td>{appointment.durationMinutes} min</td>
                  <td>₪{appointment.price}</td>
                  <td className={appointment.discountApplied ? 'discount-applied' : ''}>
                    ₪{appointment.finalPrice}
                  </td>
                  <td>
                    {appointment.discountApplied && (
                      <span className="badge badge-success">10% OFF</span>
                    )}
                  </td>
                  <td onClick={(e) => e.stopPropagation()}>
                    {appointment.userId === user?.userId && (
                      <>
                        <button
                          onClick={() => handleEdit(appointment)}
                          className="btn btn-sm btn-secondary"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(appointment.id)}
                          className="btn btn-sm btn-danger"
                        >
                          Delete
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showModal && (
        <AppointmentModal
          appointment={editingAppointment}
          onClose={handleModalClose}
        />
      )}

      {selectedAppointment && (
        <AppointmentDetailsModal
          appointment={selectedAppointment}
          onClose={() => setSelectedAppointment(null)}
        />
      )}
    </div>
  );
}
