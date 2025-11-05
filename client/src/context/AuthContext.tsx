import {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from "react";
import { authService } from "../services/api";
import type { User, LoginCredentials, RegisterData } from "../types";

interface AuthContextType {
  user: User | null;
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const validateToken = async () => {
      console.log("validate func is called");

      const token = authService.getToken();
      if (token) {
        const userData = await authService.validateToken();
        if (userData) {
          setUser(userData);
        }
      }
      setLoading(false);
    };

    validateToken();
  }, []);

  const login = async (credentials: LoginCredentials) => {
    const userData = await authService.login(credentials);
    setUser(userData);
  };

  const register = async (data: RegisterData) => {
    const userData = await authService.register(data);
    setUser(userData);
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <AuthContext.Provider
      value={{ user, login, register, logout, isAuthenticated: !!user }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return context;
}
