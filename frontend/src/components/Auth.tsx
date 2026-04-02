import React, { useState } from "react";
import axios from "axios";
import Button from "../common/components/Button";
import { LoginRequest, RegisterRequest, AuthResponse } from "../types";

interface AuthProps {
  onLogin: (token: string, userId: string) => void;
  onClose: () => void;
}

const Auth: React.FC<AuthProps> = ({ onLogin, onClose }) => {
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    email: "",
    password: "",
    username: "",
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const endpoint = isLogin ? "/api/auth/login" : "/api/auth/register";
      const payload: LoginRequest | RegisterRequest = isLogin
        ? { email: formData.email, password: formData.password }
        : {
            username: formData.username,
            email: formData.email,
            password: formData.password,
          };

      const response = await axios.post<AuthResponse>(endpoint, payload);

      // Decode JWT to get userId
      const tokenPayload = JSON.parse(atob(response.data.token.split(".")[1]));
      const userId = tokenPayload.nameid || tokenPayload.sub;

      onLogin(response.data.token, userId);
    } catch (err: any) {
      setError(err.response?.data?.message || "An error occurred");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-70 flex justify-center items-center z-50"
      onClick={onClose}
    >
      <div
        className="bg-white p-10 rounded-2xl max-w-md w-11/12 relative shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <button
          className="absolute top-4 right-4 bg-transparent border-none text-4xl cursor-pointer text-gray-400 leading-none hover:text-gray-800 p-0"
          onClick={onClose}
        >
          &times;
        </button>

        {error && (
          <div className="bg-red-50 text-red-700 p-3 rounded-lg mb-5 text-center">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="mt-2">
          {!isLogin && (
            <div className="mb-5">
              <label className="block mb-1 font-medium text-gray-800">
                Username
              </label>
              <input
                type="text"
                name="username"
                value={formData.username}
                onChange={handleChange}
                required={!isLogin}
                className="w-full p-3 border-2 border-gray-300 rounded-lg text-base transition-colors focus:outline-none focus:border-primary"
              />
            </div>
          )}

          <div className="mb-5">
            <label className="block mb-1 font-medium text-gray-800">
              Email
            </label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              className="w-full p-3 border-2 border-gray-300 rounded-lg text-base transition-colors focus:outline-none focus:border-primary"
            />
          </div>

          <div className="mb-5">
            <label className="block mb-1 font-medium text-gray-800">
              Password
            </label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              required
              minLength={6}
              className="w-full p-3 border-2 border-gray-300 rounded-lg text-base transition-colors focus:outline-none focus:border-primary"
            />
          </div>

          <Button
            type="submit"
            variant="primary"
            size="lg"
            disabled={loading}
            isLoading={loading}
            className="w-full"
          >
            {isLogin ? "Login" : "Register"}
          </Button>
        </form>

        <p className="text-center mt-5 text-gray-600">
          {isLogin ? "Don't have an account? " : "Already have an account? "}
          <Button variant="link" onClick={() => setIsLogin(!isLogin)}>
            {isLogin ? "Register" : "Login"}
          </Button>
        </p>
      </div>
    </div>
  );
};

export default Auth;
