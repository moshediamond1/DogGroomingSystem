export interface User {
  token: string;
  username: string;
  firstName: string;
  userId: number;
}

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface RegisterData {
  username: string;
  password: string;
  firstName: string;
}

export enum DogSize {
  Small = 1,
  Medium = 2,
  Large = 3
}

export interface Appointment {
  id: number;
  userId: number;
  customerName: string;
  appointmentTime: string;
  dogSize: string;
  durationMinutes: number;
  price: number;
  finalPrice: number;
  discountApplied: boolean;
  createdAt: string;
}

export interface CreateAppointmentData {
  appointmentTime: string;
  dogSize: DogSize;
}

export interface AppointmentFilters {
  startDate?: string;
  endDate?: string;
  customerName?: string;
}
