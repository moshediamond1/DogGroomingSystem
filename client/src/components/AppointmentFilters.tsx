import { useState } from 'react';
import type { AppointmentFilters } from '../types';

interface Props {
  filters: AppointmentFilters;
  onFiltersChange: (filters: AppointmentFilters) => void;
}

export default function AppointmentFiltersComponent({ filters, onFiltersChange }: Props) {
  const [startDate, setStartDate] = useState(filters.startDate || '');
  const [endDate, setEndDate] = useState(filters.endDate || '');
  const [customerName, setCustomerName] = useState(filters.customerName || '');

  const handleApply = () => {
    onFiltersChange({
      startDate: startDate || undefined,
      endDate: endDate || undefined,
      customerName: customerName || undefined
    });
  };

  const handleClear = () => {
    setStartDate('');
    setEndDate('');
    setCustomerName('');
    onFiltersChange({});
  };

  return (
    <div className="filters-container">
      <div className="filters-grid">
        <div className="form-group">
          <label htmlFor="startDate">Start Date</label>
          <input
            id="startDate"
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="endDate">End Date</label>
          <input
            id="endDate"
            type="date"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="customerName">Customer Name</label>
          <input
            id="customerName"
            type="text"
            placeholder="Search by name..."
            value={customerName}
            onChange={(e) => setCustomerName(e.target.value)}
          />
        </div>

        <div className="form-group filters-actions">
          <button onClick={handleApply} className="btn btn-primary">
            Apply Filters
          </button>
          <button onClick={handleClear} className="btn btn-secondary">
            Clear
          </button>
        </div>
      </div>
    </div>
  );
}
