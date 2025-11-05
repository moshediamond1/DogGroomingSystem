import axios from "axios";
import type {
  LoginCredentials,
  RegisterData,
  User,
  Appointment,
  CreateAppointmentData,
  AppointmentFilters,
} from "../types";

const API_URL = "http://localhost:5000/api";

const api = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const authService = {
  async login(credentials: LoginCredentials): Promise<User> {
    const { data } = await api.post<User>("/auth/login", credentials);
    localStorage.setItem("token", data.token);
    return data;
  },

  async register(userData: RegisterData): Promise<User> {
    const { data } = await api.post<User>("/auth/register", userData);
    localStorage.setItem("token", data.token);
    return data;
  },

  logout() {
    localStorage.removeItem("token");
  },

  getToken(): string | null {
    return localStorage.getItem("token");
  },
};

export const appointmentService = {
  async getAll(filters?: AppointmentFilters): Promise<Appointment[]> {
    const params = new URLSearchParams();
    if (filters?.startDate) params.append("startDate", filters.startDate);
    if (filters?.endDate) params.append("endDate", filters.endDate);
    if (filters?.customerName)
      params.append("customerName", filters.customerName);

    const { data } = await api.get<Appointment[]>("/appointments", { params });
    return data;
  },

  async getById(id: number): Promise<Appointment> {
    const { data } = await api.get<Appointment>(`/appointments/${id}`);
    return data;
  },

  async create(appointmentData: CreateAppointmentData): Promise<Appointment> {
    const { data } = await api.post<Appointment>(
      "/appointments",
      appointmentData
    );
    return data;
  },

  async update(
    id: number,
    appointmentData: CreateAppointmentData
  ): Promise<void> {
    await api.put(`/appointments/${id}`, appointmentData);
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/appointments/${id}`);
  },
};
